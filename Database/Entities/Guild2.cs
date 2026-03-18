using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

public class Guild
{
    [Key]
    public long Id {get;set;}
    public string Name {get; set;} = string.Empty;
}

public class User
{
    [Key]
    public long UserId {get; set;}
    public string Name {get; set;} = string.Empty;

}

public class GuildUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set;}
    public User User {get; set;}
    public required Guild Guild {get; set;}
}