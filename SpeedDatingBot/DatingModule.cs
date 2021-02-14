using System;
using System.Collections.Generic;
using System.Data;
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
        private DatingSession _session;

        public DatingModule(DiscordSocketClient client, DatingSession session)
        {
            _client = client;
            _session = session;
        }

        [Command("startdating", RunMode = RunMode.Async)]
        [Alias("startdate", "date")]
        [Summary("See if the bot's running with this simple command")]
        public async Task StartDatingAsync(int minutes = 10, int sessions = 10)
        {
            IList<SocketGuildUser> boyList =
                Context.Guild.Roles.FirstOrDefault(x => x.Name == "Boy")?.Members.ToList() ?? throw new DataException();
            IList<SocketGuildUser> girlList =
                Context.Guild.Roles.FirstOrDefault(x => x.Name == "Girl")?.Members.ToList() ??
                throw new DataException();


            int channelNumber = Math.Max(boyList.Count, girlList.Count);

            ulong botRoleId = Context.Guild.Roles.FirstOrDefault(
                x => x.Members.Any(y => y.Id == _client.CurrentUser.Id) && x.IsManaged)?.Id ?? 0;

            ICategoryChannel voiceCategory = await Context.Guild.CreateCategoryChannelAsync("Dating Rooms", x =>
                x.PermissionOverwrites = new List<Overwrite>()
                {
                    new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, Overwrites.FullDeny),
                    new Overwrite(botRoleId, PermissionTarget.Role, new OverwritePermissions(
                        moveMembers: PermValue.Allow,
                        viewChannel: PermValue.Allow,
                        manageChannel: PermValue.Allow,
                        manageRoles: PermValue.Allow
                    ))
                }
            );

            for (int i = 0; i < channelNumber; i++)
            {
                SocketGuildUser boy, girl
                Random rand = new Random();
                int randBoyNumber = rand.Next(0, boyList.Count);
                int randGirlNumber = rand.Next(0, girlList.Count);

                var overwrites = new List<Overwrite>();
                if (boyList.Count > 0)
                {
                    boy = boyList[randBoyNumber];
                    overwrites.Add(new Overwrite(boy.Id, PermissionTarget.User,
                        new OverwritePermissions(
                            connect: PermValue.Allow,
                            speak: PermValue.Allow,
                            useVoiceActivation: PermValue.Allow
                        )));
                }

                if (girlList.Count > 0)
                {
                    girl = girlList[randGirlNumber];
                    overwrites.Add(new Overwrite(girl.Id, PermissionTarget.User,
                        new OverwritePermissions(
                            connect: PermValue.Allow,
                            speak: PermValue.Allow,
                            useVoiceActivation: PermValue.Allow
                        )));
                }

                var newChannel = await Context.Guild.CreateVoiceChannelAsync($"Breakout room {i}",
                    x =>
                    {
                        x.PermissionOverwrites = overwrites;
                        x.UserLimit = 2;
                        x.CategoryId = voiceCategory.Id;
                    }
                );
                
            }
        }
    }
}