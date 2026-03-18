using Botty;
using Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Fluxify.Bot;
using System.Reflection.Metadata;
using Fluxify.Application.Entities.Messages;
using Fluxify.Application.Entities.Channels;
using System.Net.WebSockets;
using Botty.Modules;
using Fluxify.Commands;
using Fluxify.Commands.CommandCollection;
using Database.Services;
public class LadgerService(Bot bot, IConfiguration config, ILogger<LadgerService> logger) : BackgroundService
{

    private readonly IConfiguration _config = config;
    private readonly ILogger<LadgerService> _logger = logger;

    public override void Dispose()
    {
        base.Dispose();
    }

    private TModule ProvideModule<TModule>(CommandContext commandContext) where TModule : notnull
    {
        commandContext.Services.GetRequiredService<ContextProvider>()
            .Context.Value = commandContext;

        return commandContext.Services.GetRequiredService<TModule>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bot.MessageReceived += HandleExpAsync;

        bot.Commands
            .Module("utils", m =>
            {
               m.Command("ping", (CommandContext ctx) => ProvideModule<UtilCommands>(ctx).PingCommand());
               m.Command("pong", (CommandContext ctx) => ProvideModule<UtilCommands>(ctx).PongCommand());
            })
            .Command("leaderboard", (CommandContext ctx) => ProvideModule<LevelCommands>(ctx).LeaderboardCommand())
            .Command("rank", (CommandContext ctx) => ProvideModule<LevelCommands>(ctx).RankCommand())
            .Command("xp", (CommandContext ctx, AppDbContext db ) => ProvideModule<LevelCommands>(ctx).XpCommand(db));

        await bot.RunAsync(stoppingToken);
    }

    private async Task HandleExpAsync(Message data)
    {
        if (data.Content?.Length <= 5) return;
        using var scope = ServiceLocator.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var guildService = scope.ServiceProvider.GetRequiredService<GuildDbService>();
        var leaderboardService = scope.ServiceProvider.GetRequiredService<LeaderboardDbService>();

        // Check if this even is a guild
        if (data.Channel is not GuildTextChannel guildTextChannel) return ;

        var userId = (long)(ulong)data.Author.Id;
        var guildId = (long)(ulong)guildTextChannel.GuildId;

        var guild = await guildService.GetOrCreateGuildAsync(guildId);
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);


        if (!guildSettings.Active) {await db.SaveChangesAsync(); return;}
        var guildUser = await guildService.GetOrCreateGuildUserAsync(guild, userId);
        var userXp = await leaderboardService.GetOrCreateUserRankAsync(guildUser);
        await db.SaveChangesAsync();
        if (userXp.IsOnCooldown) {await db.SaveChangesAsync(); return;}
        userXp.AddExp();
            
        await db.SaveChangesAsync();
    }


}