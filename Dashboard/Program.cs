using Dashboard.Components;
using Database;
using Database.Services;
using Microsoft.EntityFrameworkCore;
using Fluxify.Core;
using Fluxify.Bot;
using Fluxify.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Fluxify.Core.Credentials;
using Microsoft.AspNetCore.HttpOverrides;
using Fluxify.Rest;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var token = builder.Configuration["token"]
    ?? throw new InvalidOperationException("TOKEN is missing from configuration.");
builder.Services.AddScoped(sp =>
{
    var botToken = builder.Configuration["token"]!;

    return new FluxerConfig
    {
        CredentialProvider = async () =>
        {
            return new BotTokenCredentials(botToken);
        },
        LoggerFactory = sp.GetRequiredService<ILoggerFactory>(),
        ServiceProvider = sp
    };
});
builder.Services.AddFluxifyCore(sp => new FluxerConfig
{
    CredentialProvider = sp.GetRequiredService<IAccessTokenProvider>().GetAuthenticationTokenAsync
});

builder.Services.AddScoped(sp =>
{
    var config = new FluxerConfig
    {
        CredentialProvider = async () => new BotTokenCredentials(builder.Configuration["token"]!),
        LoggerFactory = sp.GetRequiredService<ILoggerFactory>(),
        ServiceProvider = sp
    };
    return new BotRestClient(config);
});

builder.Services.AddScoped<UserRestClient>(sp => 
{
    return new UserRestClient(new FluxerConfig
    {
        CredentialProvider = async () => 
        {
            var authState = await sp.GetRequiredService<AuthenticationStateProvider>().GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                var tokenProvider = sp.GetRequiredService<IAccessTokenProvider>();
                var token = await tokenProvider.GetAuthenticationTokenAsync();
                return token;
            }
            return new BotTokenCredentials(builder.Configuration["token"]!);
        },
        ServiceProvider = sp
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Fluxer";
})
.AddFluxer(o =>
    {
        o.ClientId = builder.Configuration["Fluxer:ClientId"]!;
        o.ClientSecret = builder.Configuration["Fluxer:ClientSecret"]!;

        o.Scope.Add("identify");
        o.Scope.Add("guilds");

        o.SaveTokens = true;
    })
    .AddCookie();
    var config = new BotConfig("l!")
        {
            Credentials = new BotTokenCredentials(token),
        };
builder.Services.AddSingleton(sp => new Bot(config));
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<LeaderboardDbService>();
builder.Services.AddScoped<GuildDbService>();


builder.Services.AddHttpContextAccessor();
var app = builder.Build();
app.UseForwardedHeaders();
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/login", () => Results.Challenge(
    new AuthenticationProperties { RedirectUri = "/" },
    ["Fluxer"]
));

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});
app.Run();

public class BotRestClient(FluxerConfig config) : RestClient(config);
public class UserRestClient(FluxerConfig config) : RestClient(config);