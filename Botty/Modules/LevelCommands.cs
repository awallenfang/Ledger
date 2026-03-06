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
using Database.Services;
using Microsoft.Extensions.Hosting;

namespace Botty.Modules;

public class LevelCommands : ModuleBase
{
    private static int CalculateLevel(int exp) => (int)(exp / 200.0);

    [Command("leaderboard")]
    [Summary("A link to the online leaderboard")]
    [RequireContext(ContextType.Guild)]
    public async Task LeaderboardCommand()
    {
        using var scope = ServiceLocator.Services.CreateScope();
        var _env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (_env.IsDevelopment())
        {
            await ReplyAsync($"This server's leaderboard is available at http://127.0.0.1:5248/leaderboard/{Context.GuildId}");
        }
        else
        {
            await ReplyAsync($"This server's leaderboard is available at https://botty.ritzin.dev/leaderboard/{Context.GuildId}");
        }
    }

    [Command("rank")]
    [Summary("Returns your rank")]
    [RequireContext(ContextType.Guild)]
    public async Task RankCommand()
    {
        using var scope = ServiceLocator.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var guildService = scope.ServiceProvider.GetRequiredService<GuildDbService>();
        var leaderboardService = scope.ServiceProvider.GetRequiredService<LeaderboardDbService>();

        // Check if this even is a guild
        var guildId = Context.GuildId!;
        var userId = Context.Message.Author.Id;
        
        // Fetch the corresponding database object and create it if it doesn't exist
        var guild = await guildService.GetOrCreateGuildAsync((long)guildId);

        // Fetch the corresponding settings and create it if it doesn't exist
        var guildSettings = await leaderboardService.GetOrCreateSettingsAsync(guild);

        if (guildSettings.Active)
        {
            var guildUser = await db.GuildUsers.FirstOrDefaultAsync(user => user.Guild == guild && user.Id == (long)userId)
            ?? db.GuildUsers.Add(new Database.GuildUser{ Guild = guild, Id = (long)userId }).Entity;

            var userXp = await db.XpGuildUsers.FirstOrDefaultAsync(user => user.User == guildUser)
            ?? db.XpGuildUsers.Add(new Database.XpGuildUserRank{ User = guildUser, Exp = 0 }).Entity;

            await ReplyAsync($"You have {userXp.Exp} experience. And are thus level {CalculateLevel(userXp.Exp)}");
        } else
        {
            await ReplyAsync($"Leveling is currently disabled on this server");
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
        var guildId = Context.GuildId;
        if (guildId == null)
        {
            await ReplyAsync($"This seems to not be a guild, so leveling is disabled.");
            return;
        }

        var guildService = scope.ServiceProvider.GetRequiredService<GuildDbService>();
        var leaderboardService = scope.ServiceProvider.GetRequiredService<LeaderboardDbService>();

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
                    await ReplyAsync("Leveling is already enabled on this server.");
                else
                {
                    guildSettings.Active = true;
                    await db.SaveChangesAsync();
                    await ReplyAsync("Leveling has been enabled on this server.");

                }
                break;
            case "off":
                if (!guildSettings.Active)
                    await ReplyAsync("Leveling is already disabled.");
                else
                {
                    guildSettings.Active = false;
                    await db.SaveChangesAsync();
                    await ReplyAsync("Leveling has been disabled on this server.");
                }
                break;
            default:
                await ReplyAsync("I do not understand what you're saying.");
                break;

        }

    }
}