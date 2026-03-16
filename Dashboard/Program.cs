using System.ComponentModel.DataAnnotations.Schema;
using Dashboard.Components;
using Database;
using Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Botty.Services;
using Fluxify.Bot;
using Fluxify.Commands;
using Fluxify.Core;
using Fluxify.Dto.Users;
using Fluxify.Gateway;
using Fluxify.Gateway.Model.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http.Headers;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<LeaderboardDbService>();
builder.Services.AddSingleton(sp => new FluxerConfig()
{
    ServiceProvider = sp,
    Credentials = new BotTokenCredentials(sp.GetRequiredService<IConfiguration>()["token"] ?? throw new InvalidOperationException("TOKEN is missing from configuration."))
});
builder.Services.AddSingleton<HttpClient>(sp => sp.GetRequiredService<FluxerConfig>() is { HttpClientFactory: {} factory } cfg ? factory(cfg) : throw new InvalidOperationException());
builder.Services.AddSingleton(new GatewayConfig()
{
});
builder.Services.AddSingleton(sp => new Bot("!", sp.GetRequiredService<FluxerConfig>(), sp.GetRequiredService<GatewayConfig>()));
// var token = builder.Configuration["TOKEN"];

// // Create the clients
// var client = new FluxerClient(token, new FluxerConfig
// {
//     RestSerilog = Log.Logger as Logger,
//     GatewaySerilog = null,
//     EnableRateLimiting = true,
//     ReconnectAttemptDelay = 2,
//     IgnoredGatewayEvents = new()
//     {
//         "PRESENCE_UPDATE"   // Ignore users online/offlince changes
//     },
//     Presence = new PresenceUpdateGatewayData(Status.Online),

// });

// builder.Services.AddSingleton(client);
builder.Services.AddScoped<BottyAPIService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
