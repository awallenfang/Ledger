using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
public class FluxerOAuthHandler : OAuthHandler<FluxerOAuthOptions>
{
    public FluxerOAuthHandler(
        IOptionsMonitor<FluxerOAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }
    private static string GenerateState()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLower();
    }
    protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
    {
        var state = GenerateState();

        // Store state in a short-lived cookie to validate on callback
        Context.Response.Cookies.Append("oauth_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(15)
        });
        var parameters = new Dictionary<string, string?>
            {
                { "client_id",     Options.ClientId },
                { "scope",         Options.Scope },
                { "response_type", "code" },
                { "redirect_uri",  redirectUri },
                { "state",         state },
            };

        return QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, parameters);
    }

    protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
    {
        var form = new MultipartFormDataContent
    {
        { new StringContent("authorization_code"), "grant_type" },
        { new StringContent(context.Code),         "code" },
        { new StringContent(context.RedirectUri),  "redirect_uri" },
        { new StringContent(Options.ClientId),     "client_id" },
        { new StringContent(Options.ClientSecret), "client_secret" },
    };

        var response = await Backchannel.PostAsync(Options.TokenEndpoint, form, Context.RequestAborted);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError("Token exchange failed with status {Status}", response.StatusCode);
            return OAuthTokenResponse.Failed(new Exception("Token exchange failed."));
        }

        return OAuthTokenResponse.Success(JsonDocument.Parse(body));
    }


    // Fetch user info and map all claims
    protected override async Task<AuthenticationTicket> CreateTicketAsync(
        ClaimsIdentity identity,
        AuthenticationProperties properties,
        OAuthTokenResponse tokens)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await Backchannel.SendAsync(request, Context.RequestAborted);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Userinfo request failed: {body}");

        using var document = JsonDocument.Parse(body);
        var user = document.RootElement.Clone();

        // Map all available claims
        var mappings = new Dictionary<string, string>
        {
            { "sub",         ClaimTypes.NameIdentifier },
            { "id",          "fluxer:id" },
            { "username",    ClaimTypes.Name },
            { "global_name", "fluxer:global_name" },
            { "discriminator","fluxer:discriminator" },
            { "avatar",      "fluxer:avatar" },
            { "email",       ClaimTypes.Email },
            { "verified",    "fluxer:verified" },
            { "flags",       "fluxer:flags" },
        };

        foreach (var (key, claimType) in mappings)
        {
            if (user.TryGetProperty(key, out var value))
                identity.AddClaim(new Claim(claimType, value.ToString()));
        }

        var principal = new ClaimsPrincipal(identity);
        var context = new OAuthCreatingTicketContext(principal, properties, Context, Scheme, Options, Backchannel, tokens, user);
        await Events.CreatingTicket(context);

        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        var code = Request.Query["code"].ToString();
        if (string.IsNullOrEmpty(code))
            return HandleRequestResult.Fail("No code was returned from the OAuth provider.");

        // Validate state
        var returnedState = Request.Query["state"].ToString();
        var storedState = Request.Cookies["oauth_state"];

        if (string.IsNullOrEmpty(storedState) || storedState != returnedState)
            return HandleRequestResult.Fail("State mismatch — potential CSRF attack.");

        // Clear the state cookie
        Context.Response.Cookies.Delete("oauth_state");

        var properties = new AuthenticationProperties { RedirectUri = "/" };
        var redirectUri = BuildRedirectUri(Options.CallbackPath);
        var context = new OAuthCodeExchangeContext(properties, code, redirectUri);
        var tokens = await ExchangeCodeAsync(context);

        if (tokens.Error != null)
            return HandleRequestResult.Fail(tokens.Error);

        var identity = new ClaimsIdentity(Scheme.Name);
        var ticket = await CreateTicketAsync(identity, properties, tokens);

        return HandleRequestResult.Success(ticket);
    }
}