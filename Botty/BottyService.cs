using Botty;
using Database;
using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Data.Enums;
using Fluxer.Net.Data.Models;
using Fluxer.Net.Gateway.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

public class BottyService : BackgroundService
{
    private readonly IServiceProvider _services;


    public BottyService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Your Fluxer token
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var token = config["TOKEN"];

        // Create the clients
        var client = new FluxerClient(token, new FluxerConfig
        {
            RestSerilog = Log.Logger as Logger,
            GatewaySerilog = Log.Logger as Logger,
            EnableRateLimiting = true,
            ReconnectAttemptDelay = 2,
            IgnoredGatewayEvents = new()
            {
                "PRESENCE_UPDATE"   // Ignore users online/offlince changes
            },
            Presence = new PresenceUpdateGatewayData(Status.Online)
        });
        // var apiClient = new ApiClient(token, new() 
        // {
        //     Serilog = Log.Logger as Logger,
        //     EnableRateLimiting = true,
        // });

        // var gatewayClient = new GatewayClient(token, new() 
        // {
        //     Serilog = Log.Logger as Logger,
        //     EnableRateLimiting = true,
        //     ReconnectAttemptDelay = 2,
        //     IgnoredGatewayEvents = new()
        //     {
        //         "PRESENCE_UPDATE"   // Ignore users online/offlince changes
        //     },
        //     Presence = new PresenceUpdateGatewayData(Status.Online)
        // });

        var commands = new CommandService(
            Log.Logger as Logger,
            _services
        );
        // var commands = new CommandService(
        //     '!',
        //     Log.Logger as Logger,
        //     null
        // );

        await commands.AddModulesAsync(Assembly.GetExecutingAssembly());

        Log.Information("Registered {ModuleCount} command module(s) with {CommandCount} command(s)",
            commands.Modules.Count, commands.Commands.Count());
        client.Gateway.Ready += (data) =>
        {
            Log.Information("Bot is ready! Logged in as {Username}", data.User.Username);
            Log.Information("Connected to {GuildCount} guilds!", data.Guilds.Count());
        };

        client.Gateway.MessageCreate += async (data) =>
        {
            if (data.Author == null || data.Author.IsBot) return;


            int argPos = 0;
            if (data.Content?.StartsWith('!') == true)
            {
                argPos = 1;

                var context = new CommandContext(client, data);

                var result = await commands.ExecuteAsync(context, argPos);

                if (!result.IsSuccess)
                {
                    Log.Warning("Command failed: {Error}", result);
                }
            }
            else
            // Leaderboard handling
            if (data.Content?.Count() > 5)
            {
                using var scope = ServiceLocator.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Check if this even is a guild
                var guild_id = data.GuildId;
                if (guild_id != null)
                {
                    // Fetch the corresponding database object and create it if it doesn't exist
                    var guild = await db.Guilds.FindAsync((long)guild_id)
                        ?? db.Guilds.Add(new Database.Guild {Id = (long)guild_id }).Entity;

                    // Fetch the corresponding settings and create it if it doesn't exist
                    var guild_settings = await db.XpGuildSettings.FirstOrDefaultAsync(settings => settings.Guild == guild)
                    ?? db.XpGuildSettings.Add(new Database.XpGuildSettings{ Guild = guild, active = false }).Entity;

                    if (guild_settings.active)
                    {
                        var guild_user = await db.GuildUsers.FirstOrDefaultAsync(user => user.Guild == guild)
                        ?? db.GuildUsers.Add(new Database.GuildUser{ Guild = guild }).Entity;

                        var user_xp = await db.XpGuildUsers.FirstOrDefaultAsync(user => user.User == guild_user)
                        ?? db.XpGuildUsers.Add(new Database.XpGuildUserRank{ User = guild_user, Exp = 0 }).Entity;

                        user_xp.Exp += Random.Shared.Next(15, 25);
                        await db.SaveChangesAsync();
                    }
                }
                
            }
 
        };

        // Connect to the gateway
        await client.Gateway.ConnectAsync();

        Log.Information("Bot is running! Press Ctrl+C to exit.");

        // Keep the application running
        await Task.Delay(Timeout.Infinite, stoppingToken);

    }
}