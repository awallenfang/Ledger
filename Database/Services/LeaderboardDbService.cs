using Microsoft.EntityFrameworkCore;

namespace Database.Services;

public class LeaderboardEntry
{
    public required User User { get; set; }
    public int Exp { get; set; }
    public int Level => (int)(Exp / 100.0) + 1;
}

public class LeaderboardDbService
{
    private readonly AppDbContext _db;

    public LeaderboardDbService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<XpGuildUserRank>?> GetGuildLeaderboard(long guildId)
    {
        var guild = await _db.Guilds.FindAsync(guildId);
        if (guild == null) return null;

        var guild_settings = await GetOrCreateSettingsAsync(guild);

        return await _db.XpGuildUsers
        .Include(u => u.User)              
            .ThenInclude(gu => gu.User)
        .Where(u => u.User.GuildId == guildId)
        .OrderByDescending(u => u.Exp)
        .ToListAsync();
    }

    public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard()
    {
        var ranks = await _db.XpGuildUsers
            .Include(u => u.User)
                .ThenInclude(gu => gu.User)
            .Where(u => u.User.User != null)
            .ToListAsync();

        return ranks
            .GroupBy(u => u.User.UserId)
            .Select(g => new LeaderboardEntry
            {
                User = g.First().User.User,
                Exp = g.Sum(u => u.Exp)
            })
            .OrderByDescending(e => e.Exp)
            .ToList();
    }

    public async Task<XpGuildSettings> GetOrCreateSettingsAsync(Guild guild) =>
        await _db.XpGuildSettings.FirstOrDefaultAsync(s => s.Guild == guild)
        ?? _db.XpGuildSettings.Add(new XpGuildSettings { Guild = guild, Active = false }).Entity;

    public async Task<XpGuildUserRank> GetOrCreateUserRankAsync(GuildUser guildUser) =>
        await _db.XpGuildUsers.FirstOrDefaultAsync(u => u.User == guildUser)
        ?? _db.XpGuildUsers.Add(new XpGuildUserRank { GuildUserId = guildUser.Id, User = guildUser, Exp = 0 }).Entity;

}