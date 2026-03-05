using Fluxer.Net;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

namespace Botty.Services;

public class BottyAPIService
{
    private readonly FluxerClient _client;

    public BottyAPIService(FluxerClient client)
    {
        _client = client;
    }

    public async Task<string> GetUserName(long user_id)
    {
        var user = await _client.Rest.GetUser((ulong)user_id);
        return user.Username;
    }
}