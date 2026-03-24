using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

public class Guild
{
    [Key]
    public long GuildId {get;set;}
    public string Name {get; set;} = string.Empty;
    public ICollection<GuildUser> GuildUsers { get; set; } = [];
    public string Prefix {get; set;} = "l!";

}

public class User
{
    [Key]
    public long UserId {get; set;}
    public string Name {get; set;} = string.Empty;

    public ICollection<GuildUser> GuildUsers { get; set; } = [];

}

public class GuildUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}

    public long UserId { get; set; }
    public long GuildId { get; set; }
    
    [ForeignKey("UserId")]
    public required User User {get; set;}
    [ForeignKey("GuildId")]
    public required Guild Guild {get; set;}
}