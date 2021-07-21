using Nerd_STF.Lists;
using System;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class User
    {
        public Mute currentMute;
        public Level level;
        public int tickets;
        public ulong userID;
        public List<Warn> warns = new();
    }
}
