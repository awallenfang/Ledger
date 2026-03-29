using Microsoft.EntityFrameworkCore;

namespace Database;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Guild> Guilds { get; set; }
    public DbSet<GuildUser> GuildUsers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<XpGuildSettings> XpGuildSettings { get; set; }
    public DbSet<XpUserSettings> XpUserSettings { get; set; }
    public DbSet<XpGuildUserRank> XpGuildUsers { get; set; }
    public DbSet<VoiceXpGuildUserRank> VCXpGuildUsers { get; set; }
    public DbSet<VoiceChatSession> VCSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}