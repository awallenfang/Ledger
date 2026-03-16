using Microsoft.AspNetCore.Authentication.OAuth;

public class FluxerOAuthOptions : OAuthOptions
{
    public string GuildId { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
    public new string Scope { get; set; } = "identify email";
}