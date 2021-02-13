using System;

namespace SpeedDatingBot
{
    public static class Helpers
    {
        public static string ENV(string key)
        {
            string toReturn = Environment.GetEnvironmentVariable(key.ToUpper());
            if(string.IsNullOrWhiteSpace(toReturn))
            {
                return null;
            }
            return toReturn.Trim();
        }
    }
}