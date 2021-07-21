using System;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class Level
    {
        public int MaxXP => level * 100 + 100;
        public int TotalXP
        {
            get
            {
                int ret = 0;
                for (int i = 0; i < level; i++) ret += 1 * 100 + 100;
                ret += xp;

                return ret;
            }
        }

        public int countedMsgs;
        public DateTime lastCountedMsg;
        public int level;
        public int msgs;
        public int xp;
    }
}
