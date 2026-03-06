using Botty;
using Database;
using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Data.Enums;
using Fluxer.Net.Gateway.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
public class BottyService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<BottyService> _logger;

    private FluxerClient _client;
    private CommandService _commands;

    public BottyService(IServiceProvider services, IConfiguration config, ILogger<BottyService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Your Fluxer token
        var token = _config["TOKEN"]
            ?? throw new InvalidOperationException("TOKEN is missing from configuration."); ;

        // Create the clients
        _client = new FluxerClient(token, new FluxerConfig
        {
            RestSerilog = (Logger)Log.Logger,
            GatewaySerilog = (Logger)Log.Logger,
            EnableRateLimiting = true,
            ReconnectAttemptDelay = 2,
            IgnoredGatewayEvents = new()
            {
                "PRESENCE_UPDATE"   // Ignore users online/offlince changes
            },
            Presence = new PresenceUpdateGatewayData(Status.Online)
        });

        _commands = new CommandService(
            (Logger)Log.Logger,
            _services
        );

        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly());

        _logger.LogInformation("Registered {ModuleCount} command module(s) with {CommandCount} command(s)",
            _commands.Modules.Count, _commands.Commands.Count());


        _client.Gateway.Ready += (data) =>
        {
            _logger.LogInformation("Bot is ready! Logged in as {Username}", data.User.Username);
            _logger.LogInformation("Connected to {GuildCount} guilds!", data.Guilds.Count());
        };

        _client.Gateway.MessageCreate += HandleMessageAsync;

        // Connect to the gateway
        await _client.Gateway.ConnectAsync();

        _logger.LogInformation("Bot is running! Press Ctrl+C to exit.");

        // Keep the application running
        await Task.Delay(Timeout.Infinite, stoppingToken);

    }

    private async void HandleMessageAsync(MessageGatewayData data)
    {
        try
        {
            if (data.Author == null || data.Author.IsBot) return;

            if (data.Content?.StartsWith('!') == true)
            {
                await HandleCommandAsync(data);
            }
            else
            {
                await HandleExpAsync(data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in message handler");

        }
    }

    private async Task HandleCommandAsync(MessageGatewayData data)
    {
        int argPos = 1;

        var context = new CommandContext(_client, data);

        var result = await _commands.ExecuteAsync(context, argPos);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Command failed: {Error}", result);
        }
    }
    private async Task HandleExpAsync(MessageGatewayData data)
    {
        if (data.Content?.Length <= 5) return;
        using var scope = ServiceLocator.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check if this even is a guild
        var guildId = data.GuildId;
        var userId = data.Author.Id;
        if (guildId != null)
        {
            // Fetch the corresponding database object and create it if it doesn't exist
            var guild = await db.Guilds.FindAsync((long)guildId)
                ?? db.Guilds.Add(new Database.Guild { Id = (long)guildId }).Entity;

            // Fetch the corresponding settings and create it if it doesn't exist
            var guildSettings = await db.XpGuildSettings.FirstOrDefaultAsync(settings => settings.Guild == guild)
            ?? db.XpGuildSettings.Add(new Database.XpGuildSettings { Guild = guild, Active = false }).Entity;

            if (guildSettings.Active)
            {
                var guildUser = await db.GuildUsers.FirstOrDefaultAsync(user => user.Guild == guild && user.Id == (long)userId)
                ?? db.GuildUsers.Add(new Database.GuildUser { Guild = guild, Id = (long)userId }).Entity;

                var userXp = await db.XpGuildUsers.FirstOrDefaultAsync(user => user.User == guildUser)
                ?? db.XpGuildUsers.Add(new Database.XpGuildUserRank { User = guildUser, Exp = 0 }).Entity;
                if ((DateTime.UtcNow - userXp.LastExp).TotalSeconds >= 60)
                {
                    userXp.Exp += Random.Shared.Next(15, 25);
                    userXp.LastExp = DateTime.UtcNow;

                }
            }
            await db.SaveChangesAsync();
        }

    }

}