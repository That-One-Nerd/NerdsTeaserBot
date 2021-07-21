using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Models;
using System;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules
{
    public class ButtonModule
    {
        public static Task ButtonHandler(SocketInteraction arg)
        {
            if (arg is SocketMessageComponent but)
            {
                Internals.context = new(Internals.client, (SocketUserMessage)but.Message);

                string id = but.Data.CustomId;

                string text = id.Trim().ToLower();

                if (text.StartsWith("delete")) DeleteMsg(but);
                else if (text.StartsWith("poll")) CheckPoll(but);
            }

            return Task.CompletedTask;
        }

        public static Task DeleteMsg(SocketMessageComponent but)
        {
            if (but.Data.CustomId.Length == "delete".Length) but.Message.DeleteAsync();
            else
            {
                string user = but.Data.CustomId["delete".Length..].TrimStart();

                if (user == but.User.Username + "#" + but.User.Discriminator) but.Message.DeleteAsync();
                else
                {
                    IUserMessage msg = (IUserMessage)but.Message;
                    if (msg != null) LogModule.LogMessage(LogSeverity.Error, but.User.Mention + ", you are not allowed to delete this message.", reply: msg);
                    else LogModule.LogMessage(LogSeverity.Error, but.User.Mention + ", you are not allowed to delete this message.");
                }
            }

            return Task.CompletedTask;
        }

        public static Task CheckPoll(SocketMessageComponent but)
        {
            if (but.Data.CustomId.Length == "poll".Length) return Task.CompletedTask;

            string[] options = but.Data.CustomId["poll".Length..].TrimStart().Split(" ");

            byte option = Convert.ToByte(options[0]);
            string hash = options[1];

            Poll poll = Data.misc.Data.polls.FindOrDefault(x => x.hash == hash);
            if (poll == default) return Task.CompletedTask;

            int usr = poll.voters.FindIndex(x => x.id == but.User.Id);

            if (usr == -1) poll.voters.Add(new() { id = but.User.Id, option = option, });
            else poll.voters[usr].option = option;

            poll.Update(but.Message as SocketUserMessage);

            return Task.CompletedTask;
        }
    }
}
