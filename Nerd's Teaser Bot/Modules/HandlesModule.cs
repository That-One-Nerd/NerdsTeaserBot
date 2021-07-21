using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules
{
    public static class HandlesModule
    {
        internal static Func<SocketUserMessage, Task> OnUserMessageRecieved;
    }
}
