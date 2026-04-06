using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class XpGuildSettingsConfiguration : IEntityTypeConfiguration<XpGuildSettings>
{
    public void Configure(EntityTypeBuilder<XpGuildSettings> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Guild)
               .WithMany()
               .HasForeignKey(e => e.GuildId);
        builder.Property(e => e.ExpMin).HasDefaultValue(15);
        builder.Property(e => e.ExpMax).HasDefaultValue(25);
        builder.Property(e => e.Cooldown).HasDefaultValue(60);
        builder.Property(e => e.Formula)
            .HasConversion<string>()
            .HasDefaultValue(XpFormula.Polynomial);
        builder.Property(e => e.FormulaBase).HasDefaultValue(100.0);
        builder.Property(e => e.FormulaMultiplier).HasDefaultValue(1.5);
        builder.Property(e => e.FormulaExponent).HasDefaultValue(2.0);
    }
}
public class XpUserSettingsConfiguration : IEntityTypeConfiguration<XpUserSettings>
{
    public void Configure(EntityTypeBuilder<XpUserSettings> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId);
    }
}

public class XpGuildUserSettingsConfiguration : IEntityTypeConfiguration<XpGuildUserSettings>
{
    public void Configure(EntityTypeBuilder<XpGuildUserSettings> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.GuildUserId);
    }
}

public class XpGuildUserRankConfiguration : IEntityTypeConfiguration<XpGuildUserRank>
{
    public void Configure(EntityTypeBuilder<XpGuildUserRank> builder)
    {
        builder.HasKey(e => e.GuildUserId);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.GuildUserId);
        builder.Property(e => e.LastExp).HasDefaultValueSql("NOW()");
    }
}