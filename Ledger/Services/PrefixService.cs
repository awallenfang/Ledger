using System.Collections.Concurrent;
using Database.Services;
using Fluxify.Core.Types;

public class PrefixService
{
    ConcurrentDictionary<Snowflake, string> prefixCache;

    public async Task InitAsync(GuildDbService guildDb)
    {
        
    }
}