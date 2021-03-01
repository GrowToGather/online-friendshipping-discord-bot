using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Rest;
using Discord.WebSocket;
using SpeedDatingBot.Models;

namespace SpeedDatingBot.Module
{
    public class Welcome : InteractiveBase
    {
        [Command("welcome", RunMode = RunMode.Async)]
        public async Task WelcomeAsync()
        {
            SocketUser messageAuthor = Context.Message.Author;
            DiscordContext context = new DiscordContext();
            User newuser = (await context.Users.FirstOrDefaultAsync(x => x.Id == messageAuthor.Id)).IfDefaultGiveMe(new User());
            SocketMessage response;
            DateTime birthday;


            await ReplyAsync("What is your First Name");
            response = await NextMessageAsync();
            newuser.FirstName = response.Content;

            await ReplyAsync("What is your Last Name");
            response = await NextMessageAsync();
            newuser.LastName = response.Content;

            while (true)
            {
                await ReplyAsync("When is your birthday? DD.MM.YYYY");
                response = await NextMessageAsync();
                if (DateTime.TryParseExact(response.Content, "dd.M.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out birthday))
                {
                    break;
                }

                await ReplyAsync("Please make sure your birthday uses the correct format");
            }

            newuser.Birthday = birthday;

            while (true)
            {
                await ReplyAsync("What is your Gender? M or F");
                response = await NextMessageAsync();
                if (response.Content.ToLower().StartsWith("m") || response.Content.ToLower().StartsWith("f"))
                {
                    break;
                }

                await ReplyAsync("Please make sure you're using M or F");
            }

            newuser.IsGirl = response.Content.ToLower() == "f";
            newuser.Id = messageAuthor.Id;
            await context.SaveChangesAsync();
        }
    }
}