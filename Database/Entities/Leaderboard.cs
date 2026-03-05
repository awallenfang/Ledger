using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

public class XpGuildSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}
    public required Guild Guild { get; set;}
    public bool active {get; set;} = false;
}

public class XpGuildUserRank
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}
    public required GuildUser User {get; set;}
    public int Exp {get; set;}
    public DateTime LastExp {get; set;} = DateTime.UtcNow;
}