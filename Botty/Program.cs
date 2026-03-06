using Botty;
using Botty.Services;
using Database;
using Database.Services;
using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Data.Enums;
using Fluxer.Net.Gateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();
            


var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Services(services)
            .WriteTo.Console(theme: AnsiConsoleTheme.Code))
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is missing from configuration.");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddHostedService<BottyService>();
        services.AddScoped<GuildDbService>();
        services.AddScoped<LeaderboardDbService>();
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