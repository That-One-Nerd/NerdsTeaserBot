using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Attributes;
using NerdsTeaserBot.Modules.Models;
using Nerd_STF.Extensions;
using Nerd_STF.File.Saving;
using Nerd_STF.Lists;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Group("message")]
    [Name("Messaging")]
    [Summary("Commands about messaging services")]
    public class MessagingModule : ModuleBase<SocketCommandContext>
    {
        [Command("channel get")]
        [Summary("The current message channel of the bot")]
        public async Task ChannelGet()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The current message channel is: " + Italics("<#" + Data.misc.Data.messagingC + ">"),
                Timestamp = DateTime.Now,
                Title = "Current Message Channel",
            };

            if (Data.misc.Data.messagingC == default) e.Description = "No current message channel found. Use " + Code("n;message channel set") + " to set it!";

            await ReplyAsync("", false, e.Build());
        }

        [Command("channel reset")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Resets the message channel of the bot back to none")]
        public async Task ChannelReset()
        {
            Data.misc.Data.messagingC = default;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the current message channel to: " + Code("None"),
                Timestamp = DateTime.Now,
                Title = "Reset Message Channel",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("channel set")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Sets the given channel to the message channel of the bot")]
        public async Task ChannelSet([Summary("The Text Channel to set the message channel to")] ITextChannel channel)
        {
            if (channel.Guild.Id != Context.Guild.Id)
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel is not in this server");
                return;
            }

            ulong old = Data.misc.Data.messagingC;
            Data.misc.Data.messagingC = channel.Id;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set message channel to: " + Italics(channel.Mention),
                Timestamp = DateTime.Now,
                Title = "Set Message Channel",
            };

            if (old != default) e.Description += "\n(Previous: " + Italics("<#" + old + ">") + ")";

            await ReplyAsync("", false, e.Build());
        }

        // end commands

        public static async Task UserJoinedHandler(SocketGuildUser user)
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Welcome, " + user.Mention + ", to " + Bold(user.Guild.Name) + "!",
                Timestamp = DateTime.Now,
            };

            await user.Guild.GetTextChannel(Data.misc.Data.messagingC).SendMessageAsync("", false, e.Build());
        }
        public static async Task UserLeftHandler(SocketGuildUser user)
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Goodbye, " + user.Mention + "! We hope you come back soon!",
                Timestamp = DateTime.Now,
            };

            await user.Guild.GetTextChannel(Data.misc.Data.messagingC).SendMessageAsync("", false, e.Build());
        }
    }
}
