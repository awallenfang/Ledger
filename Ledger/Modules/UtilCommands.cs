
using Database.Services;
using Fluxify.Commands;

namespace Ledger.Modules;

public class UtilCommands(CommandContext ctx, GuildDbService guildDb)
{
    public async Task PingCommand()
    {
        await ctx.ReplyAsync($"Pong {DateTime.Now - ctx.Message.CreatedAt}");
    }
    public async Task HelpCommand()
    {
        await ctx.ReplyAsync("""
        My commands:

        `l!rank`: Display your current rank in this guild
        `l!leaderboard`: Get a link to this guild's leaderboard
        `l!xp <on|off>`: Enable/Disable leveling in this guild (Admin)
        `l!prefix <new_prefix>`: Set the prefix for this server (Admin)
        """);
    }

    public async Task PrefixCommand()
    {
        var parts = ctx.Message.Content!.Split(" ", 2);
        if (parts.Length < 2)
        {
            await ctx.ReplyAsync("Usage: !prefix <new_prefix>");
            return;
        }
        var normalizedMessage = parts[1].ToLower().Trim();

        if (normalizedMessage.Length > 0)
        {
            await guildDb.UpdatePrefix(normalizedMessage, (long)ctx.Guild!.Id);
            await ctx.ReplyAsync($"The prefix was updated to `{normalizedMessage}`");
            return;
        }
        else
        {
            await ctx.ReplyAsync($"The prefix was invalid and wasn't used");
            return;
        }
    }

}