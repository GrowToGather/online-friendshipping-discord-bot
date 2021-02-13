using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static SpeedDatingBot.Helpers;

namespace SpeedDatingBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        
        static void Main(string[] args)  => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            // _client.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            
            await _client.LoginAsync(TokenType.Bot, ENV("TOKEN"));
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
                return;
            
            await _commands.ExecuteAsync(new SocketCommandContext(_client, message), argPos, _services);
        }
    }
}