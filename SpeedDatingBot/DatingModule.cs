using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace SpeedDatingBot
{
    public class DatingModule : ModuleBase<SocketCommandContext>
    {
        [Command("startdating", RunMode = RunMode.Async)]
        [Alias("startdate", "date")]
        [Summary("See if the bot's running with this simple command")]
        public async Task StartDating()
        {
            await ReplyAsync("Dating has begun.");
        }
    }
}