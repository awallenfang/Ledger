using Database;
using Fluxify.Bot;
using Prometheus;

namespace Ledger.Services;

public class LedgerAPIService
{
    private static readonly Histogram TaskDuration = Metrics.CreateHistogram(
    "username_fetch", "Fetch username for dashboard");
    private readonly Bot _bot;

    public LedgerAPIService(Bot bot)
    {
        _bot = bot;
    }

    public async Task<string> GetUserName(User user)
    {
        using (TaskDuration.NewTimer())
        {

            var fluxer_user = await _bot.Rest.Users[(ulong)user.UserId].GetAsync();
            if (fluxer_user == null)
            {
                return user.UserId.ToString();
            }
            return fluxer_user!.Username;
        }
    }
}