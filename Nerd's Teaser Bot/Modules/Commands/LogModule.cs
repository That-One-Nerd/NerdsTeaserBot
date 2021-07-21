using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules
{
    [Group("log")]
    [Name("Logging")]
    [Summary("Commands for setting log channels and information")]
    public class LogModule : ModuleBase<SocketCommandContext>
    {
        [Command("channel get")]
        [Summary("The current logging channel of the bot")]
        public async Task ChannelGet()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The current logging channel is: " + Italics("<#" + Data.misc.Data.logChannel + ">"),
                Timestamp = DateTime.Now,
                Title = "Current Logging Channel",
            };

            if (Data.misc.Data.logChannel == default) e.Description = "No current logging channel found. Use " + Code("n;log channel set") + " to set it!";

            await ReplyAsync("", false, e.Build());
        }

        [Command("channel reset")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Resets the logging channel of the bot back to none")]
        public async Task ChannelReset()
        {
            Data.misc.Data.logChannel = default;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the current logging channel to: " + Code("None"),
                Timestamp = DateTime.Now,
                Title = "Reset Logging Channel",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("channel set")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Sets the given channel to the logging channel of the bot")]
        public async Task ChannelSet([Summary("The Text Channel to set the logging channel to")] ITextChannel channel)
        {
            if (channel.Guild.Id != Context.Guild.Id)
            {
                LogMessage(LogSeverity.Error, "Text channel is not in this server");
                return;
            }

            ulong old = Data.misc.Data.logChannel;
            Data.misc.Data.logChannel = channel.Id;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set logging channel to: " + Italics(channel.Mention),
                Timestamp = DateTime.Now,
                Title = "Set Logging Channel",
            };

            if (old != default) e.Description += "\n(Previous: " + Italics("<#" + old + ">") + ")";

            await ReplyAsync("", false, e.Build());
        }

        // end commands

        public static void LogMessage(Exception exe) => LogMessage(LogSeverity.Error, exe.Message, exe.Source);
        public static void LogMessage(LogSeverity severity, Exception exe) => LogMessage(severity, exe.Message, exe.Source);
        public static void LogMessage(LogMessage arg) => LogMessage(arg.Severity, arg.Message, arg.Source);
        public static void LogMessage(LogSeverity severity, string message, string source = "", IUserMessage edit = null, IUserMessage reply = null)
        {
            if (Internals.context == null) return;

            EmbedBuilder e = new()
            {
                Color = Colors.SeverityColors[(int)severity],
                Description = message,
                Footer = new() { Text = source },
                Timestamp = DateTime.Now,
                Title = severity.ToString() + " Message",
            };

            if (edit == null)
            {
                if (reply != null) Internals.context.Channel.SendMessageAsync("", false, e.Build(), messageReference: new(reply.Id));
                else Internals.context.Channel.SendMessageAsync("", false, e.Build());
            }
            else edit.ModifyAsync(x =>
            {
                x.Content = "";
                x.Embed = e.Build();
            });

            Log.Write("The bot has thrown a(n) " + severity.ToString() + " Message: '" + message + "' Source: '" + source + "'");
        }

        // begin log handlers

        /*public static async Task ChannelCreatedHandler(SocketChannel channel)
        {
            
        }
        public static async Task ChannelDeletedHandler(SocketChannel channel)
        {

        }
        public static async Task ChannelModifiedHandler(SocketChannel beforeC, SocketChannel afterC)
        {

        }
        public static async Task RoleCreatedHandler(SocketRole role)
        {

        }
        public static async Task RoleDeletedHandler(SocketRole role)
        {

        }
        public static async Task RoleModifiedHandler(SocketRole beforeR, SocketRole afterR)
        {

        }
        public static async Task MessageDeletedHandler(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {

        }
        public static async Task MessageEditedHandler(Cacheable<IMessage, ulong> beforeMsg, SocketMessage afterMsg, ISocketMessageChannel channel)
        {

        }
        public static async Task MessagesPurgedHandler(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
        {

        }
        public static async Task GuildModifiedHandler(SocketGuild beforeG, SocketGuild afterG)
        {

        }
        public static async Task UserBannedHandler(SocketUser user, SocketGuild guild)
        {

        }
        public static async Task UserJoinedHandler(SocketUser user)
        {

        }
        public static async Task UserLeftHandler(SocketUser user)
        {

        }
        public static async Task UserModifiedHandler(SocketUser beforeU, SocketUser afterU)
        {

        }
        public static async Task UserUnbannedHandler(SocketUser user, SocketGuild guild)
        {

        }*/
    }
}