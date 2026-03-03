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
        };

        // Connect to the gateway
        await client.Gateway.ConnectAsync();

        Log.Information("Bot is running! Press Ctrl+C to exit.");

        // Keep the application running
        await Task.Delay(Timeout.Infinite, stoppingToken);

    }
}