using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Services;

public class GuildDbService
{
    private readonly AppDbContext _db;

    public GuildDbService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guild> GetOrCreateGuildAsync(long guildId) =>
        await _db.Guilds.FindAsync(guildId)
        ?? _db.Guilds.Add(new Guild { Id = guildId }).Entity;
}