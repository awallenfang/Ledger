using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GuildConfiguration : IEntityTypeConfiguration<Guild>
{
    public void Configure(EntityTypeBuilder<Guild> builder)
    {
        builder.HasKey(e => e.GuildId);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
    }
}

public class GuildUserConfiguration : IEntityTypeConfiguration<GuildUser>
{
    public void Configure(EntityTypeBuilder<GuildUser> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany(u => u.GuildUsers)
               .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.Guild)
               .WithMany(g => g.GuildUsers)
               .HasForeignKey(e => e.GuildId);
    }
}