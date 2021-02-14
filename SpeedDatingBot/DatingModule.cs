using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System.Text.Json;

namespace SpeedDatingBot
{
    public class DatingModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;

        public DatingModule(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("startdating", RunMode = RunMode.Async)]
        [Alias("startdate", "date")]
        [Summary("See if the bot's running with this simple command")]
        public async Task StartDatingAsync(int minutes, int sessions)
        {
            await ReplyAsync("Dating has begun.");
        }
    }
}