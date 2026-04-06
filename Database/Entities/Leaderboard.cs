using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

public enum XpFormula
{
    Linear = 0,
    Polynomial = 1,
    Exponential = 2,
    SquareRoot = 3,
}

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
    public XpFormula Formula { get; set; } = XpFormula.Polynomial;
    public double FormulaBase { get; set; } = 100;
    public double FormulaMultiplier { get; set; } = 1.5;
    public double FormulaExponent { get; set; } = 2.0;

    public int GetTotalXpForLevel(int level) => Formula switch
    {
        XpFormula.Linear => (int)Math.Round(FormulaBase * level),
        XpFormula.Polynomial => (int)Math.Round(5 * Math.Pow(level, 2) + 50 * level),
        XpFormula.Exponential => (int)Math.Round(FormulaBase * (Math.Pow(FormulaMultiplier, level) - 1)),
        XpFormula.SquareRoot => (int)Math.Round(FormulaBase * Math.Pow(level, 2)),
        _ => (int)Math.Round(5 * Math.Pow(level, 2) + 50 * level),
    };

    public int GetLevel(int totalXp)
    {
        int level = Formula switch
        {
            XpFormula.Linear => (int)(totalXp / FormulaBase),
            XpFormula.Polynomial => (int)((-50 + Math.Sqrt(2500 + 20 * totalXp)) / 10),
            XpFormula.Exponential => (int)(Math.Log(totalXp / FormulaBase + 1) / Math.Log(FormulaMultiplier)),
            XpFormula.SquareRoot => (int)Math.Sqrt(totalXp / FormulaBase),
            _ => (int)((-50 + Math.Sqrt(2500 + 20 * totalXp)) / 10),
        };

        // Correct for floating point drift in either direction
        while (GetTotalXpForLevel(level + 1) <= totalXp) level++;
        while (level > 0 && GetTotalXpForLevel(level) > totalXp) level--;

        return level;
    }

    public int GetXpToNextLevel(int totalXp)
    {
        int currentLevel = GetLevel(totalXp);
        return GetTotalXpForLevel(currentLevel + 1) - totalXp;
    }
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


    // [NotMapped]
    // public int Level => (int)(Exp / 100.0) + 1;
    public int GetLevel(XpGuildSettings settings) => settings.GetLevel(Exp);
    public int GetXpToNextLevel(XpGuildSettings settings) => settings.GetXpToNextLevel(Exp);

    public bool IsOnCooldown(long cooldown)
    {
        return (DateTime.UtcNow - LastExp).TotalSeconds < cooldown;
    }

    public void AddExp(XpGuildSettings settings)
    {
        if (!IsOnCooldown(60))
        {
            Messages += 1;
        }
        if (!IsOnCooldown(settings.Cooldown))
        {
            Exp += Random.Shared.Next(settings.ExpMin, settings.ExpMax);

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

    public void AddExp()
    {
        Exp += Random.Shared.Next(15, 25);
    }
    public int GetLevel(XpGuildSettings settings) => settings.GetLevel(Exp);
    public int GetXpToNextLevel(XpGuildSettings settings) => settings.GetXpToNextLevel(Exp);

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
