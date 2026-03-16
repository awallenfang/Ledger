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

        var guild_settings = await GetOrCreateSettingsAsync(guild);

        var guild_user = await _db.GuildUsers.FirstOrDefaultAsync(user => user.Guild.Id == guild.Id)
            ?? _db.GuildUsers.Add(new Database.GuildUser{ Guild = guild }).Entity;

        return await _db.XpGuildUsers.Where(user => user.User.Guild.Id == guildId).Include(u => u.User).OrderByDescending(u => u.Exp).ToListAsync();
    }

    public async Task<XpGuildSettings> GetOrCreateSettingsAsync(Guild guild) =>
        await _db.XpGuildSettings.FirstOrDefaultAsync(s => s.Guild == guild)
        ?? _db.XpGuildSettings.Add(new XpGuildSettings { Guild = guild, Active = false }).Entity;

    public async Task<XpGuildUserRank> GetOrCreateUserRankAsync(GuildUser guildUser) =>
        await _db.XpGuildUsers.FirstOrDefaultAsync(u => u.User == guildUser)
        ?? _db.XpGuildUsers.Add(new XpGuildUserRank { User = guildUser, Exp = 0 }).Entity;

}