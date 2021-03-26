using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SpeedDatingBot
{
    public static class Helpers
    {
        private static Random rng = new Random();
        public static string Env(string key)
        {
            string toReturn = Environment.GetEnvironmentVariable(key.ToUpper())?.Trim();
             
            if(string.IsNullOrWhiteSpace(toReturn))
            {
                return null;
            }
            return toReturn;
        }
        
        public static void Shuffle<T>(this T[] list)  
        {  
            int n = list.Length;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }

        public static async Task RemoveVoiceChannelAsync(this SocketVoiceChannel toRemove, IVoiceChannel moveTo = null)
        {
            foreach (var user in toRemove.Users)
            {
                await user.ModifyAsync(u => u.Channel = Optional.Create(moveTo));
            }

            await toRemove.DeleteAsync();
        }
    }
}