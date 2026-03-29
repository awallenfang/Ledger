using Fluxify.Core.Types;
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

    public async Task<List<LeaderboardEntry>?> GetGuildLeaderboard(long guildId, int page, int pageSize, int take)
    {
        var guild = await _db.Guilds.FindAsync(guildId);
        if (guild == null) return null;

        var guild_settings = await GetOrCreateSettingsAsync(guild);

        var ranks =  await _db.XpGuildUsers
        .Include(u => u.User)
            .ThenInclude(gu => gu.User)
        .Where(u => u.User.GuildId == guildId)
        .OrderByDescending(u => u.Exp)
        .Select(u => new LeaderboardEntry
            {
                User = u.User.User,
                Exp = u.Exp
            })
        .Skip(page * pageSize)
            .Take(take)
        .ToListAsync();

        return ranks;
    }
    public async Task<List<LeaderboardEntry>?> GetGuildVCLeaderboard(long guildId, int page, int pageSize, int take)
    {
        var guild = await _db.Guilds.FindAsync(guildId);
        if (guild == null) return null;

        var guild_settings = await GetOrCreateSettingsAsync(guild);

        var ranks =  await _db.VCXpGuildUsers
        .Include(u => u.User)
            .ThenInclude(gu => gu.User)
        .Where(u => u.User.GuildId == guildId)
        .OrderByDescending(u => u.Exp)
        .Select(u => new LeaderboardEntry
            {
                User = u.User.User,
                Exp = u.Exp
            })
        .Skip(page * pageSize)
            .Take(take)
        .ToListAsync();

        return ranks;
    }

    public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard(int page, int pageSize, int take)
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
            .Skip(page * pageSize)
            .Take(take)
            .ToList();
    }

    public async Task<XpUserSettings> UpdateUserSettings(long userId, bool global)
    {

        var settings = await _db.XpUserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);


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
        }
        else
        {
            settings.Global = global;
        }

        var rows = await _db.SaveChangesAsync();

        return settings;
    }

    public async Task<int> GetGuildRankAsync(XpGuildUserRank guildUser)
    {
        var rank = await _db.XpGuildUsers
            .Where(u => u.User.Guild == guildUser.User.Guild && u.Exp > guildUser.Exp)
            .CountAsync();
        
        return rank + 1;
    }
    public async Task<XpGuildSettings> GetOrCreateSettingsAsync(Guild guild)
    {
        var settings = await _db.XpGuildSettings.FirstOrDefaultAsync(s => s.Guild == guild)
        ?? _db.XpGuildSettings.Add(new XpGuildSettings { Guild = guild, Active = false }).Entity;
        await _db.SaveChangesAsync();
        return settings;
    }
    public async Task<XpUserSettings> GetOrCreateUserSettingsAsync(User user)
    {
        var settings = await _db.XpUserSettings.FirstOrDefaultAsync(s => s.User == user)
        ?? _db.XpUserSettings.Add(new XpUserSettings { User = user, Active = false, Global = false }).Entity;
        await _db.SaveChangesAsync();
        return settings;
    }
        

    public async Task<XpGuildUserRank> GetOrCreateUserRankAsync(GuildUser guildUser)
    {
        var rank = await _db.XpGuildUsers.FirstOrDefaultAsync(u => u.User == guildUser)
        ?? _db.XpGuildUsers.Add(new XpGuildUserRank { GuildUserId = guildUser.Id, User = guildUser, Exp = 0 }).Entity;
        await _db.SaveChangesAsync();
        return rank;
        
    }
    public async Task<VoiceXpGuildUserRank> GetOrCreateUserVcRankAsync(GuildUser guildUser)
    {
        var rank = await _db.VCXpGuildUsers.FirstOrDefaultAsync(s => s.User == guildUser)
                ?? _db.VCXpGuildUsers.Add(new VoiceXpGuildUserRank { GuildUserId = guildUser.Id, User = guildUser, Exp = 0 }).Entity;
        await _db.SaveChangesAsync();
        return rank;
        
    }

    public async Task UpdateVCSession(Snowflake userId, Snowflake guildId, bool inVc)
    {
        var guildUser = await _db.GuildUsers.Where(u => (u.GuildId == (long)guildId) &&(u.UserId == (long) userId)).FirstOrDefaultAsync();
        if (guildUser == null) return;
        if (inVc)
        {
            var session = await _db.VCSessions.FirstOrDefaultAsync(s => s.User == guildUser)
                ?? _db.VCSessions.Add(new VoiceChatSession { GuildUserId = guildUser.Id, User = guildUser }).Entity;
        } else
        {
            var session = await _db.VCSessions.FirstAsync(s => s.User == guildUser);
            _db.VCSessions.Remove(session);
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<VoiceChatSession>> GetVCSessions()
    {
        return await _db.VCSessions
            .Include(u => u.User)
            .ThenInclude(gu => gu.User)
            .Include(u => u.User)
            .ThenInclude(gu => gu.Guild)
            .ToListAsync();
    }

}