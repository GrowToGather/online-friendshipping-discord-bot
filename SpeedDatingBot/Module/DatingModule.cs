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
        public async Task StartDatingAsync(int minutes = 10, int sessions = 12)
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

            _session.DatingCategoryId = datingCategory.Id;
            _session.InSession = true;
            await StartBreakoutRooms(datingCategory.Id);
        }

        private async Task StartBreakoutRooms(ulong datingCategoryId)
        {
            Random rand = new Random();
            var waitingRoomUsers = Context.Guild.GetVoiceChannel(_waitingRoomId).Users;
            User[] dbUsers;
            await using (DiscordContext context = new DiscordContext())
            {
                dbUsers = await context.Users.ToArrayAsync();
            }
            
            int[] randomNumbers = dbUsers.Select(x => rand.Next()).ToArray();
            var people = (from dbUser in dbUsers
                join guildUser in waitingRoomUsers on dbUser.Id equals guildUser.Id
                select new
                {
                    GuildUser = guildUser,
                    IsGirl = dbUser.IsGirl
                }).ToArray();
            var boys = people.Where(p => !p.IsGirl).Zip(randomNumbers,
                    (person, order) => new {GuildUser = person.GuildUser, Order = order})
                .OrderBy(person => person.Order)
                .Select(person => person.GuildUser);
            var girls = people.Where(p => p.IsGirl).Select(person => person.GuildUser);
            foreach ((SocketGuildUser Boy, SocketGuildUser Girl) couple in boys.Zip(girls))
            {
                await MoveToNewRoomAsync(couple.Boy, couple.Girl, datingCategoryId);
            }
        }

        private async Task TimeSwapRooms(int minutes, int sessions)
        {
            // any negative number will cause this to go infinitely. It will then have to be manually stopped
            for (int i = 0; i != sessions; i++)
            {
                await Task.Delay((minutes - 1) * 60 * 1000);
                if (!_session.InSession) return;
                await AnnounceOneMinuteLeftAndWaitOneMinute();
                if (!_session.InSession) return;
                await SwapRooms();
            }

            await StopDatingSession(); //stop dating if the session loop exits without returning from a manual command
        }

        public async Task MoveToNewRoomAsync(SocketGuildUser boy, SocketGuildUser girl, ulong voiceCategoryId)
        {
            RestVoiceChannel voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Breakout room",
                x => x.CategoryId = voiceCategoryId);

            await voiceChannel.AddPermissionOverwriteAsync(boy, Overwrites.ConnectVoice);
            await voiceChannel.AddPermissionOverwriteAsync(girl, Overwrites.ConnectVoice);
            await boy.ModifyAsync(user => user.Channel = voiceChannel);
            await girl.ModifyAsync(user => user.Channel = voiceChannel);
        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task PingAsync()
        {
            using var context = new DiscordContext();
            User[] users = await context.Users.ToArrayAsync();
            await ReplyAsync(users.Length.ToString());
        }

        [Command("stopdating", RunMode = RunMode.Async)]
        [Alias("enddate", "enddating", "stopdate")]
        [Summary("End the dating session")]
        public async Task StopDatingSession()
        {
            var datingCategory = Context.Guild.GetCategoryChannel(_session.DatingCategoryId);
            var socketVoiceChannels =
                (from channel in datingCategory.Channels select channel as SocketVoiceChannel).ToArray();

            // SocketVoiceChannel[] socketVoiceChannels = channels as SocketVoiceChannel[] ?? channels.ToArray();
            foreach (SocketGuildUser users in socketVoiceChannels.SelectMany(c => c.Users))
            {
                await users.ModifyAsync(u =>
                    u.Channel = Optional.Create((IVoiceChannel) Context.Guild.GetVoiceChannel(_waitingRoomId)));
            }

            foreach (SocketVoiceChannel channel in socketVoiceChannels)
            {
                await channel.DeleteAsync();
            }

            await datingCategory.DeleteAsync();
            _session.InSession = false;
        }

        [Command("swap", RunMode = RunMode.Async)]
        [Summary("Swap people to another room")]
        public async Task SwapRooms()
        {
            var datingCategory = Context.Guild.GetCategoryChannel(_session.DatingCategoryId);
            var socketVoiceChannels =
                (from channel in datingCategory.Channels
                    orderby channel.Position
                    select channel as SocketVoiceChannel).ToArray();

            ulong previousUserId = 0;
            for (int i = 0; i < socketVoiceChannels.Length; i++)
            {
                IGuildUser toBeMoved = socketVoiceChannels[i].Users
                    .FirstOrDefault(x => x.Roles.Any(y => y.Name == "Boy") && x.Id != previousUserId);
                if (toBeMoved == null) continue;
                previousUserId = toBeMoved.Id;
                SocketVoiceChannel newChannel = socketVoiceChannels[(i + 1) % socketVoiceChannels.Length];
                await newChannel.AddPermissionOverwriteAsync(toBeMoved, Overwrites.ConnectVoice);
                await socketVoiceChannels[i].RemovePermissionOverwriteAsync(toBeMoved);
                await toBeMoved.ModifyAsync(user => user.Channel = newChannel);
            }
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