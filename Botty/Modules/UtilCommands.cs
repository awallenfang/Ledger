using System.Reflection;
using Fluxer.Net.Commands;
using Fluxer.Net.Commands.Attributes;
using Fluxer.Net.Data.Models;
using Fluxer.Net.EmbedBuilder;
using Serilog;

namespace Botty.Modules;

public class UtilCommands : ModuleBase
{
    [Command("ping")]
    [Summary("Checks if the bot is responding")]
    public async Task PingCommand()
    {
        Log.Debug("Timestamp: {a} {b}", DateTime.Now, Context.Message.Timestamp);
        await ReplyAsync($"Pong {DateTime.Now - Context.Message.Timestamp}");
    }

    [Command("pong")]
    [Summary("Checks if the bot is responding")]
    public async Task PongCommand()
    {
        await ReplyAsync($"Pong");
    }

}