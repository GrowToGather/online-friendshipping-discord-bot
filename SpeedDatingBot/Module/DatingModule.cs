using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using SpeedDatingBot.Models;

namespace SpeedDatingBot.Module
{
    [RequireRole("Moderator")]
    public class DatingModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private DatingSession _session;
        private ulong _waitingRoomId;
        private string _datingRoomCategoryName;

        public DatingModule(DiscordSocketClient client, DatingSession session)
        {
            _client = client;
            _session = session;
            _waitingRoomId = 810729843280183316;
            _datingRoomCategoryName = "Dating Rooms";
        }

        [Command("startdating", RunMode = RunMode.Async)]
        [Alias("startdate", "date")]
        [Summary("Start the dating session")]
        public async Task StartDatingAsync(int minutes = 0, int sessions = -1)
        {
            ulong botRoleId = Context.Guild.Roles.FirstOrDefault(
                x => x.Members.Any(y => y.Id == _client.CurrentUser.Id) && x.IsManaged)?.Id ?? 0;
            ICategoryChannel datingCategory = await Context.Guild.CreateCategoryChannelAsync(_datingRoomCategoryName,
                x =>
                    x.PermissionOverwrites = new List<Overwrite>()
                    {
                        new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, Overwrites.FullDeny),
                        new Overwrite(botRoleId, PermissionTarget.Role, Overwrites.BotPermissions)
                    }
            );
            
            _session.DatingCategory = datingCategory;
            _session.InSession = true;
            await StartBreakoutRooms();
            if (minutes > 0)
            {
                await TimeSwapRooms(minutes, sessions);
                await EndDatingSession();
            }
        }

        private async Task StartBreakoutRooms()
        {
            Random rand = new Random();
            var waitingRoomUsers = Context.Guild.GetVoiceChannel(_waitingRoomId).Users;
            User[] dbUsers;
            await using (DiscordContext context = new DiscordContext())
            {
                dbUsers = await context.Users.ToArrayAsync();
            }

            int[] randomNumbers = dbUsers.Select(_ => rand.Next(6)).ToArray();
            var people = dbUsers.Join(waitingRoomUsers, dbUser => dbUser.Id, guildUser => guildUser.Id,
                    (dbUser, guildUser) => new
                    {
                        GuildUser = guildUser,
                        IsGirl = dbUser.IsGirl,
                        Age = dbUser.Age
                    })
                .Zip(randomNumbers, (person, rand) => new {Person = person, Order = rand + person.Age})
                .OrderBy(x => x.Order)
                .Select(x => x.Person).ToArray();
            var boys = from person in people where !person.IsGirl select person.GuildUser;
            var girls = from person in people where person.IsGirl select person.GuildUser;
            foreach (var (boy, girl) in boys.Zip(girls))
            {
                await MoveToNewRoomAsync(boy, girl);
            }
        }

        private async Task TimeSwapRooms(int minutes, int sessions)
        {
            // any negative number will cause this to go infinitely. It will then have to be manually stopped
            for (int i = 1; i != sessions; i++)
            {
                await WaitForBreakoutRooms(minutes);
                if (!_session.InSession) return;
                await SwapRooms();
            }
            await WaitForBreakoutRooms(minutes);
        }

        private async Task WaitForBreakoutRooms(int minutes)
        {
            await Task.Delay((minutes - 1) * 60 * 1000);
            if (!_session.InSession) return;
            await AnnounceOneMinuteLeftAndWaitOneMinute();
        }

        public async Task MoveToNewRoomAsync(params SocketGuildUser[] users)
        {
            RestVoiceChannel voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Breakout room",
                x => x.CategoryId = _session.DatingCategory.Id);

            foreach (var user in users)
            {
                await voiceChannel.AddPermissionOverwriteAsync(user, Overwrites.ConnectVoice);
                await user.ModifyAsync(u => u.Channel = voiceChannel);
            }
        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong! :ping_pong:");
        }

        [Command("stopdating", RunMode = RunMode.Async)]
        [Alias("enddate", "enddating", "stopdate")]
        [Summary("End the dating session")]
        public async Task EndDatingSession()
        {
            if (!_session.InSession) return;
            
            await EndBreakoutSession();
            await _session.DatingCategory.DeleteAsync();
            _session.DatingCategory = null;
            _session.InSession = false;
        }

        private async Task EndBreakoutSession()
        {
            SocketCategoryChannel datingSocketCategory = Context.Guild.GetCategoryChannel(_session.DatingCategory.Id);

            var socketVoiceChannels =
                from channel in datingSocketCategory.Channels select channel as SocketVoiceChannel;

            foreach (SocketVoiceChannel channel in socketVoiceChannels)
            {
                await channel.RemoveVoiceChannelAsync(Context.Guild.GetVoiceChannel(_waitingRoomId));
            }
        }

        

        [Command("swap", RunMode = RunMode.Async)]
        [Summary("Swap people to another room")]
        public async Task SwapRooms()
        {
            await EndBreakoutSession();
            await Task.Delay(3000);
            await StartBreakoutRooms();
        }

        private async Task AnnounceOneMinuteLeftAndWaitOneMinute()
        {
            SocketTextChannel announcementChannel = Context.Guild.GetTextChannel(810308969985212436);
            var message = await announcementChannel.SendMessageAsync("60 seconds left @here");
            await Task.Delay(5000);
            for (int i = 55; i > 0; i = i - 5)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                await message.ModifyAsync(x => x.Content = $"{i} seconds left @here");
                watch.Stop();
                var delay = 5000 - (int) watch.ElapsedMilliseconds;
                await Task.Delay(delay > 0 ? delay : 0);
                if (!_session.InSession)
                {
                    break;
                }
            }

            await message.DeleteAsync();
        }
    }
}