using Discord;
using Discord.Net;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.Extensions
{
    public static class IUserExtension
    {
        public static async Task<(IUserMessage, bool)> DMUserAsync(this IUser user, string msg = "", bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent component = null)
        {
            (IUserMessage, bool) val = (null, false);
            try { val = (await user.SendMessageAsync(msg, isTTS, embed, options, allowedMentions, component), true); }
            catch (HttpException exe) when (exe.DiscordCode == 50007) { LogModule.LogMessage(LogSeverity.Warning, "Cannot DM this user. This is because the user most likely has the bot blocked"); }

            return val;
        }
    }
}
