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

        public void StartDatingSession()
        {
            InSession = true;
        }

        public void EndDatingSession()
        {
            InSession = false;
        }
        
        public bool InSession { get; private set; }
        public ulong DatingCategoryId { get; set; }
    }
}