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

namespace Botty.Modules;

public class LevelCommands(CommandContext ctx, IHostEnvironment env, AppDbContext db, GuildDbService guildService, LeaderboardDbService leaderboardService)
{
    public async Task LeaderboardCommand()
    {
        // Check if this even is a guild
        if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
        {
            throw new CommandException("This seems to not be a guild, so leveling is disabled.");    
        }

        var guildId = (ulong)guildTextChannel.GuildId!;

        if (env.IsDevelopment())
        {
            await ctx.ReplyAsync($"This server's leaderboard is available at http://127.0.0.1:5248/leaderboard/{guildId}");
        }
        else
        {
            await ctx.ReplyAsync($"This server's leaderboard is available at https://botty.ritzin.dev/leaderboard/{guildId}");
        }
    }

    public async Task GlobalLeaderboardCommand() {

    }

    public async Task RankCommand()
    {

        // Check if this even is a guild
        if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
        {
            throw new CommandException("Command must be executed from a guild!");    
        }

        var guildId = (ulong)guildTextChannel.GuildId!;
        var userId = (ulong)ctx.Message.Author.Id;
        
        // Fetch the corresponding database object and create it if it doesn't exist
        var guild = await guildService.GetOrCreateGuildAsync((long)guildId);

        // Fetch the corresponding settings and create it if it doesn't exist
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);

        if (guildSettings.Active)
        {
            var guildUser = await db.GuildUsers.FirstOrDefaultAsync(user => user.Guild == guild && user.UserId == (long)userId)
            ?? db.GuildUsers.Add(new Database.GuildUser{ Guild = guild, UserId = (long)userId }).Entity;

            var userXp = await db.XpGuildUsers.FirstOrDefaultAsync(user => user.User == guildUser)
            ?? db.XpGuildUsers.Add(new Database.XpGuildUserRank{ User = guildUser, Exp = 0 }).Entity;

            await ctx.ReplyAsync($"You have {userXp.Exp} experience. And are thus level {userXp.Level}");
        } else
        {
            await ctx.ReplyAsync($"Leveling is currently disabled on this server");
        }
        await db.SaveChangesAsync();
    }

    public async Task XpCommand(AppDbContext db)
    {
        var message = ctx.Message.Content.Split(" ", 2)[1];

        // Check if this even is a guild
        if (ctx.Message.Channel is not GuildTextChannel guildTextChannel)
        {
            throw new CommandException("This seems to not be a guild, so leveling is disabled.");    
        }

        var guildId = (ulong)guildTextChannel.GuildId!;

        // Fetch the corresponding database object and create it if it doesn't exist
        var guild = await guildService.GetOrCreateGuildAsync((long)guildId);

        // Fetch the corresponding settings and create it if it doesn't exist
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);

        var normalizedMessage = message.ToLower().Trim();
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