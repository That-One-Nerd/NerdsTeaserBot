using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class Mute
    {
        public bool IsMuted
        {
            get
            {
                bool ret = release < DateTime.Now;
                if (unmute != null) if (unmute.moderator != default) ret |= unmute.time < DateTime.Now;

                return !ret;
            }
        }

        public ulong moderator;
        public string reason;
        public DateTime release;
        public DateTime start;
        public Unmute unmute;

        [Serializable]
        public class Unmute
        {
            public ulong moderator;
            public string reason;
            public DateTime time;
        }
    }
}
