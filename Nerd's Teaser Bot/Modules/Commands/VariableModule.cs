using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Name("Variables")]
    [Summary("Includes commands for getting or setting variables in the bot that don't fit any other module")]
    public class VariableModule : ModuleBase<SocketCommandContext>
    {
        [Command("autopublish add")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Adds a text channel to the list of autopublishing channels")]
        public async Task AutopublishAdd([Summary("The text channel to add to the list")] ITextChannel channel)
        {
            if (channel.Guild.Id != Context.Guild.Id)
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel is not in this server");
                return;
            }

            if (Data.misc.Data.publishChannels.Contains(channel.Id))
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel already exists in list");
                return;
            }

            RestTextChannel c = await (await Context.Client.Rest.GetGuildAsync(Context.Guild.Id)).GetTextChannelAsync(channel.Id); 
            if (c is not RestNewsChannel)
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel is not a " + Url("Discord Announcement Channel", "https://support.discord.com/hc/en-us/articles/360032008192"));
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Any message sent in " + channel.Mention + " will now become automatically published to other servers",
                Timestamp = DateTime.Now,
                Title = "Channel added to Autopublishing Channels",
            };

            Data.misc.Data.publishChannels.Add(channel.Id);

            await ReplyAsync("", false, e.Build());
        }

        [Command("autopublish list")]
        [Summary("Shows the current list of autopublishing channels")]
        public async Task AutopublishList()
        {
            int removedCount = 0;
            string[] text = new[] { "", "has", "it", "it is" };

            for (int i = 0; i < Data.misc.Data.publishChannels.Length; i++)
            {
                ulong u = Data.misc.Data.publishChannels[i];
                RestTextChannel c = await (await Context.Client.Rest.GetGuildAsync(Context.Guild.Id)).GetTextChannelAsync(u);
                if (c == null || c is not RestNewsChannel)
                {
                    Data.misc.Data.publishChannels.Remove(u);
                    removedCount++;
                    if (i >= 1) text = new[] { "s", "have", "them", "they are" };
                }
            }

            if (removedCount != 0) LogModule.LogMessage(LogSeverity.Warning, removedCount + " Autopublishing Channel" + text[0] + " " + text[1] + " been removed for no longer being found in the server. This is because the channel" + text[0] + " may have been deleted, the bot can no longer see " + text[2] + ", or " + text[3] + " not a " + Url("Discord Announcement Channel", "https://support.discord.com/hc/en-us/articles/360032008192"));

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = Data.misc.Data.publishChannels.Length + " Autopublishing Channel",
            };

            if (Data.misc.Data.publishChannels.Length != 1) e.Title += "s";

            foreach (ulong u in Data.misc.Data.publishChannels) e.Description += "<#" + u + ">\n";

            await ReplyAsync("", false, e.Build());
        }

        [Command("autopublish remove")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Removes a channel from the list of autopublish channels")]
        public async Task AutopublishRemove([Summary("The text channel to remove from the list")] ITextChannel channel)
        {
            if (!Data.misc.Data.announceChannels.Contains(channel.Id))
            {
                LogModule.LogMessage(LogSeverity.Error, "Channel not found in list");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = "The bot will no longer automatically publish any message in " + channel.Mention,
                Timestamp = DateTime.Now,
                Title = "Autopublish Channel Removed"
            };

            Data.misc.Data.announceChannels.Remove(channel.Id);

            await ReplyAsync("", false, e.Build());
        }

        [Command("changelog get")]
        [Summary("The current changelog channel of the bot")]
        public async Task ChangelogGet()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The current changelog channel is: " + Italics("<#" + Data.misc.Data.changelogChannel + ">"),
                Timestamp = DateTime.Now,
                Title = "Current changelog channel",
            };

            if (Data.misc.Data.changelogChannel == default) e.Description = "No current changelog channel found. Use " + Code("n;changelog set") + " to set it!";

            await ReplyAsync("", false, e.Build());
        }

        [Command("changelog reset")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Resets the changelog channel of the bot back to none")]
        public async Task ChangelogReset()
        {
            Data.misc.Data.changelogChannel = default;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the current changelog channel to: " + Code("None"),
                Timestamp = DateTime.Now,
                Title = "Reset changelog channel",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("changelog set")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Sets the given channel to the changelog channel of the bot")]
        public async Task ChangelogSet([Summary("The Text Channel to set the changelog to")] ITextChannel channel)
        {
            if (channel.Guild.Id != Context.Guild.Id)
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel is not in this server");
                return;
            }

            ulong old = Data.misc.Data.changelogChannel;
            Data.misc.Data.changelogChannel = channel.Id;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set changelog channel to: " + Italics(channel.Mention),
                Timestamp = DateTime.Now,
                Title = "Set changelog channel",
            };

            if (old != default) e.Description += "\n(Previous: " + Italics("<#" + old + ">") + ")";

            await ReplyAsync("", false, e.Build());
        }

        [Command("prefix get")]
        [Summary("The current prefix of the bot. Use it in commands")]
        public async Task PrefixGet()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The current bot prefix is: " + Code(Data.misc.Data.prefix),
                Footer = new() { Text = "If all else fails, you can also mention me to execute a command" },
                Timestamp = DateTime.Now,
                Title = "Current Bot Prefix",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("prefix reset")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Resets the prefix of the bot back to the default, 'n;'")]
        public async Task PrefixReset()
        {
            Data.misc.Data.prefix = "n;";

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the current bot prefix to: " + Code("n;"),
                Footer = new() { Text = "If all else fails, you can also mention me to execute a command" },
                Timestamp = DateTime.Now,
                Title = "Reset Bot Prefix",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("prefix set")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Sets the given string to be the prefix of the bot")]
        public async Task PrefixSet([Summary("The new prefix to set the bot's prefix to")] string newPrefix)
        {
            string old = Data.misc.Data.prefix;
            Data.misc.Data.prefix = newPrefix;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the bot's prefix to: " + Code(newPrefix),
                Timestamp = DateTime.Now,
                Title = "Set Bot Prefix",
            };

            e.Description += "\n(Previous: " + Code(old) + ")";

            await ReplyAsync("", false, e.Build());
        }

        // handles

        public static async Task AutopublishHandler(SocketUserMessage msg)
        {
            if (!Data.misc.Data.publishChannels.Contains(msg.Channel.Id)) return;

            RestTextChannel c = await (await Internals.context.Client.Rest.GetGuildAsync(Internals.context.Guild.Id)).GetTextChannelAsync(msg.Channel.Id);
            if (c is RestNewsChannel)
            {
                try { await msg.CrosspostAsync(); }
                catch(HttpException exe) when (exe.DiscordCode == 403) { LogModule.LogMessage(LogSeverity.Error, "Attempted to publish message, but failed. Error code 403"); }
            }
        }
    }
}