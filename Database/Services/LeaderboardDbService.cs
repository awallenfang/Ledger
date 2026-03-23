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
            .Where(u => _db.XpUserSettings
                .Any(s => s.UserId == u.User.UserId && s.Global == true))
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

    public async Task<XpUserSettings> UpdateUserSettings(long userId, bool global)
    {
        Console.WriteLine($"UpdateUserSettings called: userId={userId}, global={global}");

        var settings = await _db.XpUserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        Console.WriteLine($"Existing settings: {(settings is null ? "null" : $"id={settings.Id}, global={settings.Global}")}");

        if (settings is null)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) throw new InvalidOperationException($"User {userId} not found.");

            settings = new XpUserSettings
            {
                User = user,
                UserId = userId,
                Global = global
            };
            _db.XpUserSettings.Add(settings);
            Console.WriteLine("Created new settings entry");
        }
        else
        {
            settings.Global = global;
            Console.WriteLine($"Updated existing entry, new global={settings.Global}");
        }

        var rows = await _db.SaveChangesAsync();
        Console.WriteLine($"Rows affected: {rows}");

        return settings;
    }
    public async Task<XpGuildSettings> GetOrCreateSettingsAsync(Guild guild) =>
        await _db.XpGuildSettings.FirstOrDefaultAsync(s => s.Guild == guild)
        ?? _db.XpGuildSettings.Add(new XpGuildSettings { Guild = guild, Active = false }).Entity;
    public async Task<XpUserSettings> GetOrCreateUserSettingsAsync(User user) =>
        await _db.XpUserSettings.FirstOrDefaultAsync(s => s.User == user)
        ?? _db.XpUserSettings.Add(new XpUserSettings { User = user, Active = false, Global = false }).Entity;

    public async Task<XpGuildUserRank> GetOrCreateUserRankAsync(GuildUser guildUser) =>
        await _db.XpGuildUsers.FirstOrDefaultAsync(u => u.User == guildUser)
        ?? _db.XpGuildUsers.Add(new XpGuildUserRank { GuildUserId = guildUser.Id, User = guildUser, Exp = 0 }).Entity;

}