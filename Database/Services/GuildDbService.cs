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
        ?? _db.Guilds.Add(new Guild { GuildId = guildId }).Entity;


    public async Task<GuildUser> GetOrCreateGuildUserAsync(Guild guild, User user) =>
        await _db.GuildUsers.FirstOrDefaultAsync(u => u.Guild == guild && u.User == user)
        ?? _db.GuildUsers.Add(new GuildUser { Guild = guild, User = user }).Entity;

    public async Task<User> GetOrCreateUserAsync(long userId) =>
        await _db.Users.FirstOrDefaultAsync( u => u.UserId == userId)
        ?? _db.Users.Add(new User { UserId = userId }).Entity;


}