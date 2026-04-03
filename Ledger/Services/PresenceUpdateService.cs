using Database;
using Database.Services;
using Fluxify.Application.Entities.Users;
using Fluxify.Dto.Users;
using Fluxify.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ledger.Services;

public class PresenceUpdateService : BackgroundService
{private readonly IServiceScopeFactory _scopeFactory;

    public PresenceUpdateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var guildCount = db.Guilds.Count();
            var userCount = db.Users.Count();
            var bot = scope.ServiceProvider.GetRequiredService<Bot>();
            await bot.Gateway.UpdatePresenceAsync(
                status: (Fluxify.Gateway.Model.Data.UserStatus)UserStatus.Online,
                customStatus: new Fluxify.Dto.Users.CustomStatus{Text= $"l! | Serving {userCount} users on {guildCount} guilds!"});
        }
    }
}