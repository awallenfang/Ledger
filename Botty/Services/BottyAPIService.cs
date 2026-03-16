using Fluxify.Bot;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

namespace Botty.Services;

public class BottyAPIService
{
    private readonly Bot _bot;

    public BottyAPIService(Bot bot)
    {
        _bot = bot;
    }

    public async Task<string> GetUserName(long user_id)
    {   
        var user = await _bot.Rest.Users[(ulong)user_id].GetAsync();
        return user!.Username;
    }
}