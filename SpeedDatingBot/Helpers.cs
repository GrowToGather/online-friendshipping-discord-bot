using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SpeedDatingBot
{
    public static class Helpers
    {
        public static string Env(string key)
        {
            string toReturn = Environment.GetEnvironmentVariable(key.ToUpper())?.Trim();
             
            if(string.IsNullOrWhiteSpace(toReturn))
            {
                return null;
            }
            return toReturn;
        }

        public static async Task RemoveVoiceChannel(this SocketVoiceChannel toRemove, IVoiceChannel moveTo = null)
        {
            foreach (var user in toRemove.Users)
            {
                await user.ModifyAsync(u => u.Channel = Optional.Create(moveTo));
            }

            await toRemove.DeleteAsync();
        }
    }
}