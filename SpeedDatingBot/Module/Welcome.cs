using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using SpeedDatingBot.Models;

namespace SpeedDatingBot.Module
{
    public class Welcome : InteractiveBase
    {
        private Config _config;
        private DiscordSocketClient _client;

        public Welcome(Config config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("welcome", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task WelcomeAsync()
        {
            SocketUser messageAuthor = Context.Message.Author;
            DiscordContext context = new DiscordContext();
            bool isNew;
            User newUser;
            const string timeOutMessage = "You took too long. Please rerun the command by typing welcome.";
            
            var userFromDatabase = await context.Users.FindAsync(messageAuthor.Id);
            if (userFromDatabase != null)
            {
                newUser = userFromDatabase;
                isNew = false;
            }
            else
            {
                newUser = new User();
                isNew = true;
            }

            SocketMessage response;
            DateTime birthday;


            await ReplyAsync("What is your First Name");
            response = await NextMessageAsync();
            if (response == null)
            {
                await ReplyAsync(timeOutMessage);
                return;
            }
            newUser.FirstName = response.Content;

            await ReplyAsync("What is your Last Name");
            response = await NextMessageAsync();
            if (response == null)
            {
                await ReplyAsync(timeOutMessage);
                return;
            }
            newUser.LastName = response.Content;

            while (true)
            {
                await ReplyAsync("When is your birthday? DD.MM.YYYY");
                response = await NextMessageAsync();
                if (response == null)
                {
                    await ReplyAsync(timeOutMessage);
                    return;
                }
                if (DateTime.TryParseExact(response.Content, "dd.M.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out birthday))
                {
                    await ReplyAsync(
                        $"Please confirm your birthday is {birthday.ToString("MMMM")} {birthday.Day} {birthday.Year}? Type Y or N");
                    response = await NextMessageAsync();
                    if (response == null)
                    {
                        await ReplyAsync(timeOutMessage);
                    }
                    if (!response.Content.ToLower().StartsWith("y")) continue;

                    break;
                }

                await ReplyAsync("Please make sure your birthday uses the correct format");
            }

            newUser.Birthday = birthday;

            while (true)
            {
                await ReplyAsync("What is your Gender? M or F");
                response = await NextMessageAsync();
                if (response == null)
                {
                    await ReplyAsync(timeOutMessage);
                    return;
                }
                if (response.Content.ToLower().StartsWith("m") || response.Content.ToLower().StartsWith("f"))
                {
                    break;
                }

                await ReplyAsync("Please make sure you're using M or F");
            }

            newUser.IsGirl = response.Content.ToLower() == "f";
            newUser.Id = messageAuthor.Id;

            if (isNew)
            {
                await context.Users.AddAsync(newUser);
            }
            await context.SaveChangesAsync();
            await UpdateUserRole(messageAuthor, newUser.IsGirl, $"{newUser.FirstName} {newUser.LastName}");
            await ReplyAsync("Thank you! Enjoy Online Friendshipping");
        }

        public async Task UpdateUserRole(SocketUser user, bool isGirl, string nickname)
        {
            SocketGuild guild = _client.Guilds.First(x => x.Id == _config.ConfigData.GuildId);
            SocketGuildUser guildUser = guild.GetUser(user.Id);


            await guildUser.RemoveRolesAsync(
                guildUser.Roles.Where(role => role.Name != "Moderator" && !role.IsEveryone));
            await guildUser.AddRoleAsync(guild.Roles.FirstOrDefault(role => role.Name == (isGirl ? "Girl" : "Boy")));
            await guildUser.ModifyAsync(x => x.Nickname = nickname);
        }
    }
}