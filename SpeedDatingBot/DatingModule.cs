using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using SpeedDatingBot.Models;

namespace SpeedDatingBot
{
    [RequireRole("Moderator")]
    public class DatingModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private DatingSession _session;
        private ulong _id;
        private string _datingRoomCategoryName;

        public DatingModule(DiscordSocketClient client, DatingSession session)
        {
            _client = client;
            _session = session;
            _id = 810729843280183316;
            _datingRoomCategoryName = "Dating Rooms";
        }

        [Command("startdating", RunMode = RunMode.Async)]
        [Alias("startdate", "date")]
        [Summary("Start the dating session")]
        public async Task StartDatingAsync(int minutes = 10, int sessions = 12)
        {
            SocketVoiceChannel waitingRoom = Context.Guild.GetVoiceChannel(_id);


            ulong botRoleId = Context.Guild.Roles.FirstOrDefault(
                x => x.Members.Any(y => y.Id == _client.CurrentUser.Id) && x.IsManaged)?.Id ?? 0;
            ICategoryChannel datingCategory = await Context.Guild.CreateCategoryChannelAsync(_datingRoomCategoryName,
                x =>
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

            _session.DatingCategoryId = datingCategory.Id;
            _session.InSession = true;

            var boy = 1;
            var girl = 1;

            foreach (var user in waitingRoom.Users)
            {
                int channelIndex;
                if (user.Roles.Any(x => x.Name == "Boy"))
                {
                    channelIndex = boy;
                    boy++;
                }
                else if (user.Roles.Any(x => x.Name == "Girl"))
                {
                    channelIndex = girl;
                    girl++;
                }
                else
                {
                    continue;
                }

                IVoiceChannel newChannel = await FindOrCreateVoiceChannelAsync(channelIndex, user, datingCategory.Id);
                await newChannel.AddPermissionOverwriteAsync(user, Overwrites.ConnectVoice);
                await user.ModifyAsync(prop => prop.Channel = Optional.Create(newChannel));
            }

            await TimeSwapRooms(minutes, sessions);
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
            var socketVoiceChannels = (from channel in datingCategory.Channels select channel as SocketVoiceChannel).ToArray();

            // SocketVoiceChannel[] socketVoiceChannels = channels as SocketVoiceChannel[] ?? channels.ToArray();
            foreach (SocketGuildUser users in socketVoiceChannels.SelectMany(c => c.Users))
            {
                await users.ModifyAsync(u =>
                    u.Channel = Optional.Create((IVoiceChannel) Context.Guild.GetVoiceChannel(_id)));
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

        private async Task<IVoiceChannel> FindOrCreateVoiceChannelAsync(int offset, IGuildUser user,
            ulong voiceCategoryId)
        {
            SocketCategoryChannel datingCategory = Context.Guild.GetCategoryChannel(_session.DatingCategoryId);

            IVoiceChannel voiceChannel =
                (IVoiceChannel) datingCategory.Channels.FirstOrDefault(x => x.Position == offset + 1) ??
                await Context.Guild.CreateVoiceChannelAsync("Breakout room",
                    x => x.CategoryId = voiceCategoryId);

            await voiceChannel.AddPermissionOverwriteAsync(user, Overwrites.ConnectVoice);
            return voiceChannel;
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