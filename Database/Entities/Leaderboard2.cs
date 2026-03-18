using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

public class XpGuildSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}
    public required Guild Guild { get; set;}
    public bool Active {get; set;} = false;
}

public class XpUserSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}
    public required User User { get; set;}
    public bool Active {get; set;} = false;
}


public class XpGuildUserSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}
    public required Guild Guild { get; set;}
    public required User User { get; set;}
    public bool Active {get; set;} = false;
}

public class XpGuildUserRank
{
   
    public long Id {get; set;}
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required GuildUser User {get; set;}
    public int Exp {get; set;}
    public DateTime LastExp {get; set;} = DateTime.UtcNow;


    [NotMapped]
    public int Level => (int)(Exp / 100.0) + 1;

    [NotMapped]
    public bool IsOnCooldown => (DateTime.UtcNow - LastExp).TotalSeconds < 60;

    public void AddExp()
    {
        if (!IsOnCooldown)
        {
            Exp += Random.Shared.Next(15, 25);
            LastExp = DateTime.UtcNow;
        }
    }
}

