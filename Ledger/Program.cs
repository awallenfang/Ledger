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
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(l => l.AddConsole())
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is missing from configuration.");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddHostedService<LadgerService>();
        services.AddScoped<GuildDbService>();
        services.AddScoped<LeaderboardDbService>();

        services.AddTransient<UtilCommands>();
        services.AddTransient<LevelCommands>();

        services.AddScoped<ContextProvider>()
            .AddScoped(sp => sp.GetRequiredService<ContextProvider>().Context.Value!);
        services.AddSingleton(sp => new FluxerConfig()
        {
            ServiceProvider = sp,
            Credentials = new BotTokenCredentials(sp.GetRequiredService<IConfiguration>()["token"] ?? throw new InvalidOperationException("TOKEN is missing from configuration."))
        });
        services.AddSingleton<HttpClient>(sp => sp.GetRequiredService<FluxerConfig>() is { HttpClientFactory: {} factory } cfg ? factory(cfg) : throw new InvalidOperationException());
        services.AddSingleton(new GatewayConfig()
        {
            IgnoredGatewayEvents = ["PRESENCE_UPDATE"],
            DefaultPresence = new(UserStatus.Online, CustomStatus: new CustomStatus(Text: "Ladger is listening!"))
        });
        services.AddSingleton(sp => new Bot("!", sp.GetRequiredService<FluxerConfig>(), sp.GetRequiredService<GatewayConfig>()));
    })
    .Build();

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

class ContextProvider
{
    public AsyncLocal<CommandContext> Context { get; } = new();
}