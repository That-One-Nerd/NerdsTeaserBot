using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class Warn
    {
        public string hash;
        public ulong moderator;
        public string reason;
        public DateTime time;
    }
}
