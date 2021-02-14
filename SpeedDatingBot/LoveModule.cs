using System.Threading.Tasks;
using Discord.Commands;

namespace SpeedDatingBot
{
    public class LoveModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping", RunMode = RunMode.Async)]
        [Summary("See if the bot's running with this simple command")]
        public async Task PingAsync()
        {
            await ReplyAsync(":ping_pong: Pong!");
        }
    }
}