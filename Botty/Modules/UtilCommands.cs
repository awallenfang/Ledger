
using Fluxify.Commands;
using Serilog;

namespace Botty.Modules;

public class UtilCommands(CommandContext ctx)
{
    public async Task PingCommand()
    {
        Log.Debug("Timestamp: {a} {b}", DateTime.Now, ctx.Message.CreatedAt);
        await ctx.ReplyAsync($"Pong {DateTime.Now - ctx.Message.CreatedAt}");
    }

    public async Task PongCommand()
    {
        await ctx.ReplyAsync($"Pong");
    }

}