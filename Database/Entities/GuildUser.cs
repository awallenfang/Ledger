namespace Database;

public class GuildUser
{
    public int Id {get;set;}
    public string Name {get; set;} = string.Empty;
    public Guild Guild { get; set; }
}
