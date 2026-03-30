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

    public async Task<Guild> GetOrCreateGuildAsync(long guildId)
    {
        var guild = await _db.Guilds.FindAsync(guildId)
        ?? _db.Guilds.Add(new Guild { GuildId = guildId }).Entity;
        await _db.SaveChangesAsync();
        return guild;
    }


    public async Task<GuildUser> GetOrCreateGuildUserAsync(Guild guild, User user)
    {
        var guildUser = await _db.GuildUsers.FirstOrDefaultAsync(u => u.Guild == guild && u.User == user)
        ?? _db.GuildUsers.Add(new GuildUser { Guild = guild, User = user }).Entity;
        await _db.SaveChangesAsync();
        return guildUser;
    }

    public async Task<User> GetOrCreateUserAsync(long userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
        ?? _db.Users.Add(new User { UserId = userId }).Entity;
        await _db.SaveChangesAsync();
        return user;
    }

    public string GetPrefix(long guildId)
    {
        var guild = _db.Guilds.FirstOrDefault(g => g.GuildId == guildId)
        ?? _db.Guilds.Add(new Guild { GuildId = guildId }).Entity;
        _db.SaveChanges();
        return guild.Prefix;
    }


    public async Task UpdatePrefix(string prefix, long guildId)
    {
        var guild = _db.Guilds.FirstOrDefault(g => g.GuildId == guildId)
        ?? _db.Guilds.Add(new Guild { GuildId = guildId }).Entity;
        guild.Prefix = prefix;
        _db.SaveChanges();
    }


}