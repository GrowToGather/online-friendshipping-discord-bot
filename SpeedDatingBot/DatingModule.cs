﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
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
        private ulong _id;
        private string _name;

        public DatingModule(DiscordSocketClient client, DatingSession session)
        {
            _client = client;
            _session = session;
            _id = 810729843280183316;
            _name = $"Breakout room ";
        }

        [Command("startdating", RunMode = RunMode.Async)]
        [Alias("startdate", "date")]
        [Summary("Start the dating session")]
        public async Task StartDatingAsync(int minutes = 10, int sessions = 10)
        {
            SocketVoiceChannel waitingRoom = Context.Guild.GetVoiceChannel(_id);


            ulong botRoleId = Context.Guild.Roles.FirstOrDefault(
                x => x.Members.Any(y => y.Id == _client.CurrentUser.Id) && x.IsManaged)?.Id ?? 0;

            ICategoryChannel datingCategory = await Context.Guild.CreateCategoryChannelAsync("Dating Rooms", x =>
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

            int boy = 1;
            int girl = 1;

            foreach (var user in waitingRoom.Users)
            {
                int channelIndex = 0;
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

                Console.WriteLine($"Channel Index is {channelIndex}");

                IVoiceChannel newChannel = await FindOrCreateVoiceChannelAsync(channelIndex, user, datingCategory.Id);
                await newChannel.AddPermissionOverwriteAsync(user, Overwrites.ConnectVoice);
                await user.ModifyAsync(prop => prop.Channel = Optional.Create(newChannel));
            }
        }

        [Command("stopdating", RunMode = RunMode.Async)]
        [Alias("enddate", "enddating", "stopdate")]
        [Summary("End the dating session")]
        public async Task StopDatingSession()
        {
            var datingCategory = Context.Guild.GetCategoryChannel(_session.DatingCategoryId);
            var channels = from channel in datingCategory.Channels select channel as SocketVoiceChannel;

            SocketVoiceChannel[] socketVoiceChannels = channels as SocketVoiceChannel[] ?? channels.ToArray();
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
            _session.EndDatingSession();
        }

        [Command("swap", RunMode = RunMode.Async)]
        [Summary("Swap people to another room")]
        public async Task SwapRooms()
        {
            var datingCategory = Context.Guild.GetCategoryChannel(_session.DatingCategoryId);
            var channels = from channel in datingCategory.Channels
                orderby channel.Position
                select channel as SocketVoiceChannel;
            List<SocketVoiceChannel> socketVoiceChannels = new List<SocketVoiceChannel>();
            foreach (var channel in channels)
            {
                socketVoiceChannels.Add(channel);
            }

            // IVoiceChannel[] socketVoiceChannels = channels as SocketVoiceChannel[] ?? list.ToArray();
            ulong previousUserId = 0;
            int count = socketVoiceChannels.Count;
            for (int i = 0; i < count; i++)
            {
                IGuildUser toBeMoved = socketVoiceChannels[i].Users
                    .FirstOrDefault(x => x.Roles.Any(y => y.Name == "Boy") && x.Id != previousUserId);
                if (toBeMoved == null) continue;
                previousUserId = toBeMoved.Id;
                SocketVoiceChannel newChannel = socketVoiceChannels[(i + 1) % count];
                await newChannel.AddPermissionOverwriteAsync(toBeMoved, Overwrites.ConnectVoice);
                await socketVoiceChannels[i].RemovePermissionOverwriteAsync(toBeMoved);
                await toBeMoved.ModifyAsync(user => user.Channel = newChannel);
            }
        }

        private async Task<IVoiceChannel> FindOrCreateVoiceChannelAsync(int offset, IGuildUser user,
            ulong voiceCategoryId)
        {
            string name = _name + offset;
            Console.WriteLine(name);
            IVoiceChannel voiceChannel = (IVoiceChannel) Context.Guild.Channels.FirstOrDefault(x => x.Name == name) ??
                                         await Context.Guild.CreateVoiceChannelAsync(name,
                                             x => x.CategoryId = voiceCategoryId);
            await voiceChannel.AddPermissionOverwriteAsync(user, Overwrites.ConnectVoice);
            return voiceChannel;
        }
    }
}