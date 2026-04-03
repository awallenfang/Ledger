using Ledger;
using Ledger.Modules;
using Ledger.Services;
using Database;
using Database.Services;
using Fluxify.Bot;
using Fluxify.Commands;
using Fluxify.Core;
using Fluxify.Dto.Users;
using Fluxify.Gateway;
using Fluxify.Gateway.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fluxify.Core.Credentials;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prometheus;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(l =>
    {
        l.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is missing from configuration.");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<GuildDbService>();
        services.AddScoped<LeaderboardDbService>();
        services.AddScoped<RankCardService>();
        services.AddHostedService<VoiceUpdateService>();
        services.AddHostedService<PresenceUpdateService>();

        services.AddSingleton<ContextProvider>()
            .AddScoped(sp => sp.GetRequiredService<ContextProvider>().Context.Value!);
        services.AddScoped<UtilCommands>();
        services.AddScoped<LevelCommands>();
        var token = context.Configuration["token"]
            ?? throw new InvalidOperationException("TOKEN is missing from configuration.");
        services.TryAddSingleton<FluxerConfig>(sp =>
        {

            return new FluxerConfig()
            {
                Credentials = new BotTokenCredentials(token),
                LoggerFactory = sp.GetRequiredService<ILoggerFactory>(),
                ServiceProvider = sp
            };
        });

        services.TryAddTransient<AuthenticationHeaderHandler>();
        services.TryAddTransient(sp =>
        {
            var config = sp.GetRequiredService<FluxerConfig>();

            return config.HttpClientFactory.Invoke(config);
        });


        var gatewayConfig = new GatewayConfig()
        {
            IgnoredGatewayEvents = ["PRESENCE_UPDATE"],
            DefaultPresence = new(UserStatus.Online, CustomStatus: new CustomStatus(Text: "l! | Ledger is listening!"))
        };
        services.AddSingleton(gatewayConfig);
        services.AddSingleton<PrefixService>();
        services.AddSingleton((sp) =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

            var config = new BotConfig("l!")
            {
                Credentials = new BotTokenCredentials(token),
                FluxerConfig = {
                    LoggerFactory = sp.GetRequiredService<ILoggerFactory>(),
                    ServiceProvider = sp
                },
                GatewayConfig = sp.GetRequiredService<GatewayConfig>(),
                CommandConfig =
                {
                    DetermineCommandStart = (m) => 
                    {
                        using var scope = scopeFactory.CreateScope();
                        var prefixService = scope.ServiceProvider.GetRequiredService<PrefixService>();
                        return prefixService.CheckPrefix(m);
                    }
                }
            };
            return config;
        });
        services.AddSingleton(sp => new Bot(sp.GetRequiredService<BotConfig>()));
        services.AddHostedService<LedgerService>();
    })
    .Build();
var server = new MetricServer(port: 1234);
server.Start();
using (var scope = host.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Migration successful!");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to apply migrations");
        throw;
    }
}

ServiceLocator.Initialize(host.Services);

await host.RunAsync();

public class ContextProvider
{
    public AsyncLocal<CommandContext> Context { get; } = new();
}