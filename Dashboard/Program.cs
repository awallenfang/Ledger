using System.ComponentModel.DataAnnotations.Schema;
using Dashboard.Components;
using Database;
using Database.Services;
using Fluxer.Net;
using Fluxer.Net.Data.Enums;
using Fluxer.Net.Gateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Botty.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<LeaderboardDbService>();

var token = builder.Configuration["TOKEN"];

// Create the clients
var client = new FluxerClient(token, new FluxerConfig
{
    RestSerilog = Log.Logger as Logger,
    GatewaySerilog = null,
    EnableRateLimiting = true,
    ReconnectAttemptDelay = 2,
    IgnoredGatewayEvents = new()
    {
        "PRESENCE_UPDATE"   // Ignore users online/offlince changes
    },
    Presence = new PresenceUpdateGatewayData(Status.Online),

});

builder.Services.AddSingleton(client);
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
