using System.Security.Claims;

public static class FluxerUserExtensions
{
    public static string? GetFluxerId(this ClaimsPrincipal user)
        => user.FindFirstValue("fluxer:id");

    public static string? GetUsername(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Name);

    public static string? GetGlobalName(this ClaimsPrincipal user)
        => user.FindFirstValue("fluxer:global_name");

    public static string? GetAvatar(this ClaimsPrincipal user)
        => user.FindFirstValue("fluxer:avatar");

    public static string? GetEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email);

    public static bool IsVerified(this ClaimsPrincipal user)
        => user.FindFirstValue("fluxer:verified") == "True";
}