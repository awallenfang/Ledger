using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class XpGuildConfiguration : IEntityTypeConfiguration<XpGuildSettings>
{
    public void Configure(EntityTypeBuilder<XpGuildSettings> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
public class XpUserConfiguration : IEntityTypeConfiguration<XpGuildUserRank>
{
    public void Configure(EntityTypeBuilder<XpGuildUserRank> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.LastExp).HasDefaultValueSql("NOW()");
    }
}