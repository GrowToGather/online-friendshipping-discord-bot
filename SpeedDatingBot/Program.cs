using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SpeedDatingBot.Models;
using static SpeedDatingBot.Helpers;

namespace SpeedDatingBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private Config _config;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
        // static void Main(string[] args)
        // {
        //     using (var context = new DiscordContext())
        //     {
        //         var users = context.Users.ToArray();
        //         Console.WriteLine(users.Length);
        //     }
        // }
        private Program()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _config = new Config();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(new DatingSession())
                .BuildServiceProvider();
        }

        private async Task MainAsync()
        {
            _client.Log += LogAsync;
            _client.UserJoined += OnUserJoin;
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _client.LoginAsync(TokenType.Bot, _config.ConfigData.Token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task LogAsync(LogMessage msg)
        {
            await Console.Out.WriteLineAsync(msg.ToString());
            if (msg.Exception != null)
            {
                await Console.Error.WriteLineAsync(msg.Exception?.Message);
                await Console.Error.WriteLineAsync(msg.Exception?.StackTrace ?? "");
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                await LogAsync(new LogMessage(LogSeverity.Info, "CommandExecution", result.ErrorReason));
            }

            string commandName = command.IsSpecified ? command.Value.Name : "A command";
            await LogAsync(new LogMessage(LogSeverity.Info, "CommandExecution",
                $"{commandName} was executed at {DateTime.UtcNow}"));
        }

        private async Task OnUserJoin(SocketGuildUser user)
        {
            Console.WriteLine(user.Username);
            await user.SendMessageAsync("Hello new user!");
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
                return;

            await _commands.ExecuteAsync(new SocketCommandContext(_client, message), argPos, _services);
        }
    }
}