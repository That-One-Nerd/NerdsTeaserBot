using Discord;
using Discord.WebSocket;
using Nerd_STF.Lists;

namespace NerdsTeaserBot.Modules.Extensions
{
    public static class SocketGuildUserExtension
    {
        public static IRole GetHighestRole(this SocketGuildUser user)
        {
            List<IRole> roles = new(user.Roles);

            IRole ret = roles[0];

            foreach (IRole r in roles) if (r.Position > ret.Position) ret = r;

            return ret;
        }
    }
}
