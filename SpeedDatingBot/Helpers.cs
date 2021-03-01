using System;
using System.Collections.Generic;

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
        
        public static T IfDefaultGiveMe<T>(this T value, T alternate)
        {
            if (value.Equals(default(T))) return alternate;
            return value;
        }
    }
}