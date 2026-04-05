using Database;
using Database.Services;
using Microsoft.Extensions.Hosting;
using Fluxify.Commands;
using Fluxify.Commands.Exceptions;
using Fluxify.Bot;
using Fluxify.Application.Model.Messages;
using Fluxify.Application.Entities.Channels.Guilds;
using SkiaSharp;
using Prometheus;

namespace Ledger.Modules;

public class LevelCommands(CommandContext ctx, IHostEnvironment env, AppDbContext db, GuildDbService guildService, LeaderboardDbService leaderboardService, RankCardService rankCardService, Bot bot)
{
    private static readonly Histogram LeaderboardDuration = Metrics.CreateHistogram(
    "leaderboard_command", "Sending leaderboard command");
    public async Task LeaderboardCommand()
    {
        using (LeaderboardDuration.NewTimer())
        {
            // Check if this even is a guild
            if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
            {
                throw new CommandException("This seems to not be a guild, so leveling is disabled.");
            }

            var guildId = (ulong)guildTextChannel.Guild.Id!;

            if (env.IsDevelopment())
            {
                await ctx.ReplyAsync($"This server's leaderboard is available at http://127.0.0.1:5248/leaderboard/{guildId}");
            }
            else
            {
                await ctx.ReplyAsync($"This server's leaderboard is available at https://ledger.ritzin.dev/leaderboard/{guildId}");
            }
        }
    }

    public async Task GlobalLeaderboardCommand()
    {

    }
    private static readonly Histogram RankDuration = Metrics.CreateHistogram(
    "rank_command", "Sending rank command");
    public async Task RankCommand()
    {
        using (RankDuration.NewTimer())
        {

            // Check if this even is a guild
            if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
            {
                throw new CommandException("Command must be executed from a guild!");
            }

            var guildId = (ulong)guildTextChannel.Guild.Id!;

            var userId = (ulong)ctx.Message.Author.Id;
            // Fetch the corresponding database object and create it if it doesn't exist
            var guild = await guildService.GetOrCreateGuildAsync((long)guildId);
            var guildUser = (Fluxify.Application.Entities.Users.GuildMember)ctx.Message.Author;
            // Fetch the corresponding settings and create it if it doesn't exist
            var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);

            if (guildSettings.Active)
            {
                var user = await guildService.GetOrCreateUserAsync((long)userId);
                var guildUserDb = await guildService.GetOrCreateGuildUserAsync(guild, user);
                var userXp = await leaderboardService.GetOrCreateUserRankAsync(guildUserDb);
                var VcUserXp = await leaderboardService.GetOrCreateUserVcRankAsync(guildUserDb);
                var rankCardData = new RankCardData
                {
                    Username = ctx.Message.Author.Username,
                    Level = userXp.Level,
                    CurrentXp = userXp.Exp,
                    Position = await leaderboardService.GetGuildRankAsync(userXp),
                    AvatarBitmap = await LoadBitmapFromWebAsync(guildUser.GetAvatarUri().ToString())
                };
                var rankCard = rankCardService.GenerateRankCard(rankCardData);

                var builder = new MessageBuilder()
                .WithAttachment(rankCard, "rank.png", "image/png")
            .WithContent($"Chat: Level {userXp.Level}, {userXp.Exp} Exp\nVoice: Level {VcUserXp.Level}, {VcUserXp.Exp} Exp");

                await ctx.ReplyAsync(builder.Build());
            }
            else
            {
                await ctx.ReplyAsync($"Leveling is currently disabled on this server. Use `l!xp on` to enable it");
            }
        }
        await db.SaveChangesAsync();
    }

    private static readonly Histogram XpDuration = Metrics.CreateHistogram(
    "xp_command", "Sending xp command");
    public async Task XpCommand()
    {
        using (XpDuration.NewTimer())
        {

            var parts = ctx.Message.Content.Split(" ", 3);
            if (parts.Length < 2)
            {
                await ctx.ReplyAsync("Usage: !xp <on|off|init|cooldown|expmin|expmax>");
                return;
            }
            var normalizedMessage = parts[1].ToLower().Trim();

            if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
            {
                throw new CommandException("This seems to not be a guild, so leveling is disabled.");
            }

            var guildId = (ulong)guildTextChannel.Guild.Id!;
            // Fetch the corresponding database object and create it if it doesn't exist
            var guild = await guildService.GetOrCreateGuildAsync((long)guildId);

            // Fetch the corresponding settings and create it if it doesn't exist
            var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);

            switch (normalizedMessage)
            {
                case "init":
                case "on":
                    if (guildSettings.Active)
                        await ctx.ReplyAsync("Leveling is already enabled on this server.");
                    else
                    {
                        guildSettings.Active = true;
                        await ctx.ReplyAsync("Leveling has been enabled on this server.");

                    }
                    break;
                case "off":
                    if (!guildSettings.Active)
                        await ctx.ReplyAsync("Leveling is already disabled.");
                    else
                    {
                        guildSettings.Active = false;
                        await ctx.ReplyAsync("Leveling has been disabled on this server.");
                    }
                    break;
                case "cooldown":
                    {
                        if (parts.Length < 3)
                        {
                            await ctx.ReplyAsync($"Current cooldown: {guildSettings.Cooldown}s\nUsage: !xp cooldown <seconds>");
                            break;
                        }
                        if (!long.TryParse(parts[2].Trim(), out var cooldown) || cooldown < 0)
                        {
                            await ctx.ReplyAsync("Cooldown must be a positive number of seconds.");
                            break;
                        } 
                        guildSettings.Cooldown = cooldown;
                        await ctx.ReplyAsync($"Cooldown set to {cooldown} seconds.");
                        break;
                    }
                case "expmin":
                    {
                        if (parts.Length < 3)
                        {
                            await ctx.ReplyAsync($"Current max exp: {guildSettings.ExpMin}\nUsage: !xp expmin <amount>");
                            break;
                        }
                        if (!long.TryParse(parts[2].Trim(), out var expMin) || expMin < 0)
                        {
                            await ctx.ReplyAsync("Minimum exp must be a positive number.");
                            break;
                        }
                        if (expMin > guildSettings.ExpMax)
                        {
                            await ctx.ReplyAsync($"Minimum exp cannot be greater than the current maximum ({guildSettings.ExpMax}).");
                            break;
                        }
                        guildSettings.ExpMin = (int)expMin;
                        await ctx.ReplyAsync($"Minimum exp set to {expMin}.");
                        break;
                    }
                case "expmax":
                    {
                        if (parts.Length < 3)
                        {
                            await ctx.ReplyAsync($"Current max exp: {guildSettings.ExpMax}\nUsage: !xp expmax <amount>");
                            break;
                        }
                        if (!long.TryParse(parts[2].Trim(), out var expMax) || expMax < 0)
                        {
                            await ctx.ReplyAsync("Maximum exp must be a positive number.");
                            break;
                        }
                        if (expMax < guildSettings.ExpMin)
                        {
                            await ctx.ReplyAsync($"Maximum exp cannot be less than the current minimum ({guildSettings.ExpMin}).");
                            break;
                        }
                        guildSettings.ExpMax = (int)expMax;
                        await ctx.ReplyAsync($"Maximum exp set to {expMax}.");
                        break;
                    }
                default:
                    await ctx.ReplyAsync("I do not understand what you're saying.");
                    break;

            }
            await db.SaveChangesAsync();
        }

    }

    private async Task<SKBitmap?> LoadBitmapFromWebAsync(string url)
    {
        var client = new HttpClient();
        try
        {
            using (Stream stream = await client.GetStreamAsync(url))
            using (MemoryStream memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                var bitmap = SKBitmap.Decode(memStream);
                return bitmap;
            }
            ;
        }
        catch
        {
            // Handle error silently
            return null;
        }
    }
}