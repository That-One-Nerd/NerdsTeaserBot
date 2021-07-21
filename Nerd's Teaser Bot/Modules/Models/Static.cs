using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Models
{
    public static class Static
    {
        public static Credit[] Credits
        {
            get
            {
                return new Credit[]
                {
                    new()
                    {
                        id = 478210457816006666,
                        reason = "Main development of the bot.",
                    },

                    new()
                    {
                        id = 533776358980059137,
                        reason = "Gave suggestions.",
                    },

                    new() { id = 750087661569703957 },

                    new() { id = 550845244137275453 },
                };
            }
        }
    }
}
