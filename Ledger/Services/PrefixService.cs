using System.Collections.Concurrent;
using Database.Services;
using Fluxify.Application.Entities.Messages;
using Fluxify.Core.Types;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

public class PrefixService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly Histogram TaskDuration = Metrics.CreateHistogram(
    "prefix_fetching", "Duration of prefix fetching");

    public PrefixService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public int? CheckPrefix(Message m)
    {
        using (TaskDuration.NewTimer())
        {
            using var scope = _scopeFactory.CreateScope();
            var guildDb = scope.ServiceProvider.GetRequiredService<GuildDbService>();
            var guildId = m.Guild!.Id;
            var prefix = guildDb.GetPrefix((long)guildId);
            if (m.Author.Bot is not true && m.Content!.StartsWith(prefix))
            {
                return prefix.Length;
            } else
            {
                return null;
            }
        }
        
    }
}