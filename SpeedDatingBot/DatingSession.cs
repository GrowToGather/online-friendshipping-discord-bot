using System.Collections.Generic;
using System.Collections.ObjectModel;
using Discord.Commands;
using Discord.WebSocket;

namespace SpeedDatingBot
{
    public class DatingSession
    {
        public DatingSession()
        {
            InSession = false;
        }

        public bool InSession { get; set; }
        public ulong DatingCategoryId { get; set; }
    }
}