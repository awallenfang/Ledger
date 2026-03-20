using Dashboard.Components;
using Database;
using Database.Services;
using Microsoft.EntityFrameworkCore;
using Ledger.Services;
using Fluxify.Bot;
using Fluxify.Core;
using Fluxify.Gateway;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Fluxify.Core.Credentials;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Fluxer";
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
})
.AddOAuth<FluxerOAuthOptions, FluxerOAuthHandler>("Fluxer", options =>
{
    options.ClientId     = builder.Configuration["Fluxer:ClientId"]!;
    options.ClientSecret = builder.Configuration["Fluxer:ClientSecret"]!;
    options.GuildId      = builder.Configuration["Fluxer:GuildId"] ?? string.Empty;
    options.Permissions  = builder.Configuration["Fluxer:Permissions"] ?? string.Empty;
    options.Scope        = "identify guilds";

    options.AuthorizationEndpoint = "https://web.fluxer.app/oauth2/authorize";
    options.TokenEndpoint = "https://api.fluxer.app/oauth2/token";
    options.UserInformationEndpoint = "https://api.fluxer.app/oauth2/userinfo";

    options.CallbackPath = "/signin-fluxer";
    options.SaveTokens   = true;
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<LeaderboardDbService>();
builder.Services.AddFluxifyCore(sp => new FluxerConfig()
{
    Credentials = new BotTokenCredentials(sp.GetRequiredService<IConfiguration>()["token"] ?? throw new InvalidOperationException("TOKEN is missing from configuration."))
});
builder.Services.AddSingleton(new GatewayConfig()
{
});
builder.Services.AddSingleton(sp => new Bot("!", sp.GetRequiredService<FluxerConfig>(), sp.GetRequiredService<GatewayConfig>()));

builder.Services.AddScoped<LedgerAPIService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FluxerApiService>();
var app = builder.Build();


using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.MigrateAsync();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
