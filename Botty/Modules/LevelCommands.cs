using System.Reflection;
using Fluxer.Net.Commands;
using Fluxer.Net.Commands.Attributes;
using Fluxer.Net.Data.Models;
using Fluxer.Net.EmbedBuilder;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Database;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Fluxer.Net.Data.Enums;

namespace Botty.Modules;

public class LevelCommands : ModuleBase
{
    [Command("leaderboard")]
    [Summary("A link to the online leaderboard")]
    [RequireContext(ContextType.Guild)]
    public async Task LeaderboardCommand()
    {
        await ReplyAsync($"This server's leaderboard is available at https://botty.ritzin.dev/leaderboard/{Context.GuildId}");
    }

    [Command("rank")]
    [Summary("Returns your rank")]
    [RequireContext(ContextType.Guild)]
    public async Task RankCommand()
    {
        using var scope = ServiceLocator.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check if this even is a guild
        var guild_id = Context.GuildId;
        if (guild_id != null)
        {
            // Fetch the corresponding database object and create it if it doesn't exist
            var guild = await db.Guilds.FindAsync((long)guild_id)
                ?? db.Guilds.Add(new Database.Guild {Id = (long)guild_id }).Entity;

            // Fetch the corresponding settings and create it if it doesn't exist
            var guild_settings = await db.XpGuildSettings.FirstOrDefaultAsync(settings => settings.Guild == guild)
            ?? db.XpGuildSettings.Add(new Database.XpGuildSettings{ Guild = guild, active = false }).Entity;

            if (guild_settings.active)
            {
                var guild_user = await db.GuildUsers.FirstOrDefaultAsync(user => user.Guild == guild)
                ?? db.GuildUsers.Add(new Database.GuildUser{ Guild = guild }).Entity;

                var user_xp = await db.XpGuildUsers.FirstOrDefaultAsync(user => user.User == guild_user)
                ?? db.XpGuildUsers.Add(new Database.XpGuildUserRank{ User = guild_user, Exp = 0 }).Entity;

                await ReplyAsync($"You have {user_xp.Exp} experience. And are thus level {(int)(user_xp.Exp / 200.0)}");
            } else
            {
                await ReplyAsync($"Leveling is currently disabled on this server");
            }
        }
    }

    [Command("xp")]
    [Summary("Used to access the settings for the leveling")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(Permissions.ManageGuild | Permissions.Administrator)]
    public async Task XpCommand([Remainder] string message)
    {
        using var scope = ServiceLocator.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check if this even is a guild
        var guild_id = Context.GuildId;
        if (guild_id == null)
        {
            await ReplyAsync($"This seems to not be a guild, so leveling is disabled.");
            return;
        }
        
        // Fetch the corresponding database object and create it if it doesn't exist
        var guild = await db.Guilds.FindAsync((long)guild_id)
            ?? db.Guilds.Add(new Database.Guild {Id = (long)guild_id }).Entity;

        // Fetch the corresponding settings and create it if it doesn't exist
        var guild_settings = await db.XpGuildSettings.FirstOrDefaultAsync(global => global.Guild == guild)
            ?? db.XpGuildSettings.Add(new Database.XpGuildSettings{ Guild = guild, active = false }).Entity;
 
        if (guild_settings.active == false)
        {
            if (message.ToLower().Trim() == "init" || message.ToLower().Trim() == "on")
            {
                guild_settings.active = true;
                await db.SaveChangesAsync();

                await ReplyAsync($"Leveling has been enabled on this server");
            } 
            else if (message.ToLower().Trim() == "off")
            {
                await ReplyAsync($"Leveling is already disabled");
            } 
            else
            {
                await ReplyAsync($"I do not understand what you're saying");
            }
        } else
        {
            if (message.ToLower().Trim() == "off")
            {
                 guild_settings.active = false;
                await db.SaveChangesAsync();

                await ReplyAsync($"Leveling has been disabled on this server");
            } 
            else if (message.ToLower().Trim() == "init" || message.ToLower().Trim() == "on")
            {
                await ReplyAsync($"Leveling is already enabled on this server");
            } 
            else
            {
                await ReplyAsync($"I do not understand what you're saying");
            }
        }

    }
}