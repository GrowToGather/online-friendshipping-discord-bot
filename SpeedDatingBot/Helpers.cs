using System;

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
    }
}