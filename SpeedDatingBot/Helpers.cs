using System;
using System.Collections.Generic;

namespace SpeedDatingBot
{
    public static class Helpers
    {
        private static readonly Random Rng = new Random();  
        public static string Env(string key)
        {
            string toReturn = Environment.GetEnvironmentVariable(key.ToUpper())?.Trim();
             
            if(string.IsNullOrWhiteSpace(toReturn))
            {
                return null;
            }
            return toReturn;
        }
        
        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = Rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}