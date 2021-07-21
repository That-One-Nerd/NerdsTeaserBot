using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Models;
using Nerd_STF.Lists;
using System;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Name("Text")]
    [Summary("Commands related to messages sent")]
    public class TextModule : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes a certain number of messages in the current channel")]
        public async Task Purge([Summary("The amount of messages to delete")] int amount) => await Purge((ITextChannel)Context.Channel, amount);

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes a certain number of messages in a given channel")]
        public async Task Purge([Summary("The text channel to delete the messages in")] ITextChannel channel, [Summary("The amount of messages to delete")] int amount)
        {
            List<IMessage> msgs = new(await channel.GetMessagesAsync(amount).FlattenAsync());

            if (msgs.Length < amount)
            {
                LogModule.LogMessage(LogSeverity.Warning, "Amount is more than all messages in this channel. Defaulting to max");
                amount = msgs.Length;
            }

            int failed = msgs.FindAll(x => x.CreatedAt < DateTime.Now - TimeSpan.FromDays(14)).Length, deleted = amount - failed;

            await channel.DeleteMessagesAsync(msgs.FindAll(x => x.CreatedAt > DateTime.Now - TimeSpan.FromDays(14)).ToArray());

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Statistics of the purge are below:",
                Timestamp = DateTime.Now,
                Title = deleted + " Message",
            };
            if (deleted != 1) e.Title += "s";
            e.Title += " Deleted";

            string add = amount + " Message";
            if (amount != 1) add += "s";

            e.AddField("Attempted", Code(add), true);

            add = failed + " Message";
            if (failed != 1) add += "s";

            e.AddField("Failed", Code(add), true);

            await ReplyAsync("", false, e.Build());
        }
    }
}