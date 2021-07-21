using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class Tag
    {
        public string name;
        public ulong owner;
        public string response;
    }
}
