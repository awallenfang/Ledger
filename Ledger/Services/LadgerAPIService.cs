using Database;
using Fluxify.Bot;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Services;

public class LedgerAPIService
{
    private readonly Bot _bot;

    public LedgerAPIService(Bot bot)
    {
        _bot = bot;
    }

    public async Task<string> GetUserName(User user)
    {   
        var fluxer_user = await _bot.Rest.Users[(ulong)user.UserId].GetAsync();
        if (fluxer_user == null)
        {
            return user.UserId.ToString();
        }
        return fluxer_user!.Username;
    }
}