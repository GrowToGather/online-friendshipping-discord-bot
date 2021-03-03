using System.Collections.Generic;
using System.Collections.ObjectModel;
using Discord;
using Discord.Commands;
using Discord.Rest;
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
        public ICategoryChannel DatingCategory { get; set; }
    }
}