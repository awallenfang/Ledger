using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

public class XpGuildSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long GuildId { get; set; }

    [ForeignKey("GuildId")]
    public required Guild Guild { get; set; }
    public bool Active { get; set; } = false;
    public int ExpMin { get; set; } = 15;
    public int ExpMax { get; set; } = 25;
    public long Cooldown { get; set; } = 60;
}

public class XpUserSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long UserId { get; set; }

    [ForeignKey("UserId")]
    public required User User { get; set; }
    public bool Active { get; set; } = false;
    public bool Global { get; set; } = false;
}


public class XpGuildUserSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public required long GuildUserId { get; set; }
    [ForeignKey("GuildUserId")]
    public required GuildUser User { get; set; }
    public bool Active { get; set; } = true;
    public bool Leaderboard { get; set; } = true;
}

public class XpGuildUserRank
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public required long GuildUserId { get; set; }
    [ForeignKey("GuildUserId")]
    public required GuildUser User { get; set; }
    public int Exp { get; set; }
    public long Messages { get; set; }
    public DateTime LastExp { get; set; } = DateTime.UtcNow;


    [NotMapped]
    public int Level => (int)(Exp / 100.0) + 1;

    public bool IsOnCooldown(long cooldown)
    {
        return (DateTime.UtcNow - LastExp).TotalSeconds < cooldown;
    }

    public void AddExp(XpGuildSettings settings)
    {
        if (!IsOnCooldown(settings.Cooldown))
        {
            Exp += Random.Shared.Next(settings.ExpMin, settings.ExpMax);
            Messages += 1;
            LastExp = DateTime.UtcNow;
        }
    }
}
public class VoiceXpGuildUserRank
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public required long GuildUserId { get; set; }
    [ForeignKey("GuildUserId")]
    public required GuildUser User { get; set; }
    public int Exp { get; set; }

    [NotMapped]
    public int Level => (int)(Exp / 100.0) + 1;

    public void AddExp()
    {
        Exp += Random.Shared.Next(15, 25);
    }
}

public class VoiceChatSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public required long GuildUserId { get; set; }
    [ForeignKey("GuildUserId")]
    public required GuildUser User { get; set; }
    public DateTime LastTick { get; set; } = DateTime.UtcNow;
}
