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

        public void StartDatingSession(IList<ulong> boys, IList<ulong> girls)
        {
            Boys = boys;
            Girls = girls;
            InSession = true;
        }

        public void EndDatingSession()
        {
            InSession = false;
            Boys = new List<ulong>();
            Girls = new List<ulong>();
        }

        public bool PersonDisconnect(ulong person) => Boys.Remove(person) || Girls.Remove(person);
        
        public IList<ulong> Boys { get; private set; }
        public IList<ulong> Girls { get; private set; }
        public bool InSession { get; private set; }
    }
}