using Ledger;
using Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fluxify.Bot;
using Fluxify.Application.Entities.Messages;
using Fluxify.Application.Entities.Channels;
using Ledger.Modules;
using Fluxify.Commands;
using Fluxify.Commands.CommandCollection;
using Database.Services;
using Fluxify.Commands.Model;
using Fluxify.Core.Types;
public class LedgerService(Bot bot, IConfiguration config, ILogger<LedgerService> logger) : BackgroundService
{

    private readonly IConfiguration _config = config;
    private readonly ILogger<LedgerService> _logger = logger;

    private readonly Bot _bot = bot;

    private TModule ProvideModule<TModule>(CommandContext commandContext) where TModule : notnull
    {
        try
        {
            // Try to get the Provider from the context
            var provider = commandContext.Services;

            // Manually set the context in the provider
            var contextProvider = provider.GetRequiredService<ContextProvider>();
            contextProvider.Context.Value = commandContext;

            var module = provider.GetRequiredService<TModule>();
            _logger.LogInformation("Successfully resolved module {Module}", typeof(TModule).Name);
            return module;
        }
        catch (Exception ex)
        {
            // THIS WILL FINALLY SHOW YOU WHY IT IS FAILING
            _logger.LogCritical(ex, "FAILED to resolve {Module}!", typeof(TModule).Name);
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.MessageReceived += HandleExpAsync;


        _bot.Commands
            // .Command("ping", (CommandContext ctx) => ProvideModule<UtilCommands>(ctx).PingCommand())
            .Command("ping", (CommandContext ctx) => ProvideModule<UtilCommands>(ctx).PingCommand())
            .Command("pong", async (CommandContext ctx) => await ctx.ReplyAsync("Pong"))
            .Command("leaderboard", (CommandContext ctx) => ProvideModule<LevelCommands>(ctx).LeaderboardCommand())
            .Command("rank", (CommandContext ctx) => ProvideModule<LevelCommands>(ctx).RankCommand())
            .Command("xp", (CommandContext ctx) => ProvideModule<LevelCommands>(ctx).XpCommand()//, Preconditions.RequireAuthorPermissions(Permissions.Administrator)
);

        await _bot.RunAsync(stoppingToken);
    }

    private async Task HandleExpAsync(Message data)
    {
        if (data.Content?.Length <= 5) return;
        // Check if this even is a guild
        if (data.Channel is not GuildTextChannel guildTextChannel) return;
        if (data is {Author.Bot: true}) return;
        using var scope = ServiceLocator.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var guildService = scope.ServiceProvider.GetRequiredService<GuildDbService>();
        var leaderboardService = scope.ServiceProvider.GetRequiredService<LeaderboardDbService>();

        var userId = (long)(ulong)data.Author.Id;
        var guildId = (long)(ulong)guildTextChannel.Guild.Id;

        var guild = await guildService.GetOrCreateGuildAsync(guildId);
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);


        if (!guildSettings.Active) { await db.SaveChangesAsync(); return; }
        var user = await guildService.GetOrCreateUserAsync(userId);
        var userSettings = await leaderboardService.GetOrCreateUserSettingsAsync(user);
        var guildUser = await guildService.GetOrCreateGuildUserAsync(guild, user);
        var userXp = await leaderboardService.GetOrCreateUserRankAsync(guildUser);
        await db.SaveChangesAsync();
        if (userXp.IsOnCooldown) return;
        userXp.AddExp();

        await db.SaveChangesAsync();
    }


}