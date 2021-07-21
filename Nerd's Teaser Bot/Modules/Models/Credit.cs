using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class Credit
    {
        public ulong id;
        public string reason = "Honerable mention.";
    }
}
