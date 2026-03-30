using Database;
using Database.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ledger.Services;

public class VoiceUpdateService : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;

    public VoiceUpdateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var leaderboardDb = scope.ServiceProvider.GetRequiredService<LeaderboardDbService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var session in await leaderboardDb.GetVCSessions())
            {
                // Check for stale sessions and remove them
                if (session.LastTick > DateTime.Now.AddMinutes(-5))
                {
                    var user = session.User;
                    var rank = await leaderboardDb.GetOrCreateUserVcRankAsync(user);
                    rank.AddExp();
                    
                } else
                {
                    db.VCSessions.Remove(session);
                }
                await db.SaveChangesAsync();
            }
        }
    }
}