using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Services;

public class LeaderboardDbService
{
    private readonly AppDbContext _db;

    public LeaderboardDbService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<XpGuildUserRank>?> GetGuildLeaderboard(long guildId)
    {
        var guild = await _db.Guilds.FindAsync((long)guildId);

        if (guild == null)
        {
            return null;
        }

        var guild_settings = await _db.XpGuildSettings.FirstOrDefaultAsync(settings => settings.Guild.Id == guild.Id)
            ?? _db.XpGuildSettings.Add(new Database.XpGuildSettings{ Guild = guild, active = false }).Entity;

        var guild_user = await _db.GuildUsers.FirstOrDefaultAsync(user => user.Guild.Id == guild.Id)
            ?? _db.GuildUsers.Add(new Database.GuildUser{ Guild = guild }).Entity;

        return await _db.XpGuildUsers.Where(user => user.User.Guild.Id == guildId).ToListAsync();
    }
}