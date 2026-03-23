using System.Reflection;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Database;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Database.Services;
using Microsoft.Extensions.Hosting;
using Fluxify.Commands;
using Fluxify.Application.Entities.Channels;
using Fluxify.Commands.Exceptions;
using Fluxify.Bot;
using System.Collections.Frozen;
using Fluxify.Core.Types;

namespace Ledger.Modules;

public class LevelCommands(CommandContext ctx, IHostEnvironment env, AppDbContext db, GuildDbService guildService, LeaderboardDbService leaderboardService, Bot bot)
{
    public async Task LeaderboardCommand()
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

    public async Task GlobalLeaderboardCommand() {

    }

    public async Task RankCommand()
    {
        Console.WriteLine("Running rank");
        // Check if this even is a guild
        if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
        {
            throw new CommandException("Command must be executed from a guild!");    
        }

        var guildId = (ulong)guildTextChannel.Guild.Id!;
        var userId = (ulong)ctx.Message.Author.Id;
        // Fetch the corresponding database object and create it if it doesn't exist
        var guild = await guildService.GetOrCreateGuildAsync((long)guildId);

        // Fetch the corresponding settings and create it if it doesn't exist
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);

        if (guildSettings.Active)
        {
            var user = await guildService.GetOrCreateUserAsync((long)userId);
            var guildUser = await guildService.GetOrCreateGuildUserAsync(guild, user);
            var userXp = await leaderboardService.GetOrCreateUserRankAsync(guildUser);

            await ctx.ReplyAsync($"You have {userXp.Exp} experience. And are thus level {userXp.Level}");
        } else
        {
            await ctx.ReplyAsync($"Leveling is currently disabled on this server");
        }
        await db.SaveChangesAsync();
    }

    public async Task XpCommand()
    {
        Console.WriteLine("AAAAAAAAAAAAAA");
        var parts = ctx.Message.Content.Split(" ", 2);
        if (parts.Length < 2)
        {
            await ctx.ReplyAsync("Usage: !xp <on|off|init>");
            return;
        }
        var normalizedMessage = parts[1].ToLower().Trim();
        Console.WriteLine("BBBBBBBBBBBBBB");

        if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
        {
            throw new CommandException("This seems to not be a guild, so leveling is disabled.");
        }

        var guildId = (ulong)guildTextChannel.Guild.Id!;
        // Fetch the corresponding database object and create it if it doesn't exist
        var guild = await guildService.GetOrCreateGuildAsync((long)guildId);
        Console.WriteLine("CCCCCCCCCCCCCCCC");

        // Fetch the corresponding settings and create it if it doesn't exist
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);
        Console.WriteLine("DDDDDDDDDDDDDDDDDDD");

        switch (normalizedMessage)
        {
            case "init":
            case "on":
                if (guildSettings.Active) 
                    await ctx.ReplyAsync("Leveling is already enabled on this server.");
                else
                {
                    guildSettings.Active = true;
                    await db.SaveChangesAsync();
                    await ctx.ReplyAsync("Leveling has been enabled on this server.");

                }
                break;
            case "off":
                if (!guildSettings.Active)
                    await ctx.ReplyAsync("Leveling is already disabled.");
                else
                {
                    guildSettings.Active = false;
                    await db.SaveChangesAsync();
                    await ctx.ReplyAsync("Leveling has been disabled on this server.");
                }
                break;
            default:
                await ctx.ReplyAsync("I do not understand what you're saying.");
                break;

        }

    }
}