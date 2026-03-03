using Botty;
using Database;
using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Data.Enums;
using Fluxer.Net.Gateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();
            
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(context.Configuration.GetConnectionString("Default")));
        services.AddHostedService<BottyService>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

ServiceLocator.Initialize(host.Services);

await host.RunAsync();