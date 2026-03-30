using System.Collections.Concurrent;
using Database.Services;
using Fluxify.Application.Entities.Messages;
using Fluxify.Core.Types;
using Microsoft.Extensions.DependencyInjection;

public class PrefixService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PrefixService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public bool CheckPrefix(Message m)
    {
        using var scope = _scopeFactory.CreateScope();
        var guildDb = scope.ServiceProvider.GetRequiredService<GuildDbService>();
        var guildId = m.Guild!.Id;
        var prefix = guildDb.GetPrefix((long)guildId);
        return m.Author.Bot is not true && m.Content!.StartsWith(prefix);
    }
}