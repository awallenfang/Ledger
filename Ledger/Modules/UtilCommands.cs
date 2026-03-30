
using Fluxify.Commands;

namespace Ledger.Modules;

public class UtilCommands(CommandContext ctx)
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
        `l!xp <on|off>`: Enable/Disable leveling in this guild
        """);
    }



}