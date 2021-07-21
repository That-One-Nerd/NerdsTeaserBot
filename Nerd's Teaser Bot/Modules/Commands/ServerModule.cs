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
    [Name("Server")]
    [Summary("Commands about the server and it's functions")]
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        [Command("announce")]
        [RequireUserPermission(GuildPermission.ManageChannels & GuildPermission.ManageMessages)]
        [Summary("Sends a message into all channels in the announcement channel list")]
        public async Task Announce([Remainder][Summary("The message to be announced")] string text)
        {
            bool ded = false;
            if (Data.misc.Data.announceChannels is null) ded = true;
            else if (Data.misc.Data.announceChannels.Length == 0) ded = true;

            if (ded)
            {
                LogModule.LogMessage(LogSeverity.Error, "No Channels are in the Announcement Channel List");
                return;
            }

            int removedCount = 0;
            string[] txt = new[] { "", "has", "it" };

            for (int i = 0; i < Data.misc.Data.announceChannels.Length; i++)
            {
                ulong u = Data.misc.Data.announceChannels[i];
                if (Context.Guild.GetTextChannel(u) == null)
                {
                    Data.misc.Data.announceChannels.Remove(u);
                    removedCount++;
                    if (i >= 1) txt = new[] { "s", "have", "them" };
                }
            }

            if (removedCount != 0) LogModule.LogMessage(LogSeverity.Warning, removedCount + " Announcement Channel" + txt[0] + " " + txt[1] + " been removed for no longer being found in the server. This is because the channel" + txt[0] + " may have been deleted, or the bot can no longer see " + txt[2]);

            EmbedAuthorBuilder a = new()
            {
                IconUrl = Context.User.GetAvatarUrl(),
                Name = Context.User.Username + "#" + Context.User.Discriminator,
            };
            EmbedBuilder e = new()
            {
                Author = a,
                Color = Colors.DefaultColor,
                Description = text,
                Timestamp = DateTime.Now,
                Title = "Annoucement for " + Context.Guild.Name,
            };

            foreach (ulong u in Data.misc.Data.announceChannels)
            {
                ITextChannel c = Context.Guild.GetTextChannel(u);
                if (c == null) continue;

                await c.SendMessageAsync("", false, e.Build());
            }

            e.Author.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
            e.Author.Name = "Message Sent. A preview is below";

            await ReplyAsync("", false, e.Build());
        }

        [Command("announcements add")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Add a channel to the list of announcement channels")]
        public async Task AnnouncementsAdd([Summary("The text channel to add to the list")] ITextChannel channel)
        {
            if (channel.Guild.Id != Context.Guild.Id)
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel is not in this server");
                return;
            }

            if (Data.misc.Data.announceChannels.Contains(channel.Id))
            {
                LogModule.LogMessage(LogSeverity.Error, "Text channel already exists in list");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The bot will now send a message in " + channel.Mention + " when the command " + Code("n;announce") + " is triggered",
                Timestamp = DateTime.Now,
                Title = "Added Channel to Announcement Channels",
            };

            Data.misc.Data.announceChannels.Add(channel.Id);

            await ReplyAsync("", false, e.Build());
        }

        [Command("announcements list")]
        [Summary("Shows the list of announcement channels")]
        public async Task AnnouncementsList()
        {
            int removedCount = 0;
            string[] text = new[] { "", "has", "it" };

            for (int i = 0; i < Data.misc.Data.announceChannels.Length; i++)
            {
                ulong u = Data.misc.Data.announceChannels[i];
                if (Context.Guild.GetTextChannel(u) == null)
                {
                    Data.misc.Data.announceChannels.Remove(u);
                    removedCount++;
                    if (i >= 1) text = new[] { "s", "have", "them" };
                }
            }

            if (removedCount != 0) LogModule.LogMessage(LogSeverity.Warning, removedCount + " Announcement Channel" + text[0] + " " + text[1] + " been removed for no longer being found in the server. This is because the channel" + text[0] + " may have been deleted, or the bot can no longer see " + text[2]);

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = Data.misc.Data.announceChannels.Length + " Announcement Channel",
            };
            if (Data.misc.Data.announceChannels.Length != 1) e.Title += "s";

            foreach (ulong u in Data.misc.Data.announceChannels) e.Description += "<#" + u + ">\n";

            await ReplyAsync("", false, e.Build());
        }

        [Command("announcements remove")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Removes a channel from the list of announcement channels")]
        public async Task AnnouncementsRemove([Summary("The text channel to remove from the list")] ITextChannel channel)
        {
            if (!Data.misc.Data.announceChannels.Contains(channel.Id))
            {
                LogModule.LogMessage(LogSeverity.Error, "Channel not found in list");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = "The bot will no longer send a message in " + channel.Mention + " during an announcement",
                Timestamp = DateTime.Now,
                Title = "Announcement Channel Removed"
            };

            Data.misc.Data.announceChannels.Remove(channel.Id);

            await ReplyAsync("", false, e.Build());
        }

        [Command("serverinfo")]
        [Summary("Show info about the server executed in")]
        public async Task Serverinfo()
        {
            SocketGuild server = Context.Guild;
            string add;
            Embed e1, e2;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
                Title = "Server Info about " + server.Name + " (Page 1 of 2)",
            };

            if (server.AFKChannel == null) add = "No AFK Voice Channel";
            else add = server.AFKChannel.Name;
            e.AddField("AFK Voice Channel", Code(add), true);

            e.AddField("AFK Timeout", Code(server.AFKTimeout + " Seconds"), true);

            if (server.BannerUrl == null || server.BannerUrl == "") add = Code("No Banner");
            else add = Url("Click Here", server.BannerUrl);
            e.AddField("Banner", add, true);

            add = server.CategoryChannels.Count + " Categor";
            if (server.CategoryChannels.Count == 1) add += "y";
            else add += "ies";
            e.AddField("Category Count", Code(add), true);

            add = server.Channels.Count + " Channel";
            if (server.Channels.Count != 1) add += "s";
            e.AddField("Total Channel Count", Code(add), true);

            e.AddField("Created At", Code(server.CreatedAt.ToString()), true);

            e.AddField("Default Text Channel", server.DefaultChannel.Mention, true);

            e.AddField("Default Message Notifications", Code(server.DefaultMessageNotifications.ToString()), true);

            if (server.Description == null || server.Description == "") add = "No Description";
            else add = server.Description;
            e.AddField("Server Description", Code(add), true);

            if (server.DiscoverySplashUrl == null || server.DiscoverySplashUrl == "") add = Code("No Discovery Splash");
            else add = Url("Click Here", server.DiscoverySplashUrl);
            e.AddField("Discovery Splash", add, true);

            add = server.Emotes.Count + " Emote";
            if (server.Emotes.Count != 1) add += "s";
            e.AddField("Emote Count", Code(add), true);

            add = "";
            foreach (string s in server.Features) add += Code(s) + "\n";
            if (add == "") add = Code("No Server Features");
            e.AddField("Server Features", add);

            e.ThumbnailUrl = server.IconUrl;

            e.AddField("Server ID", Code(server.Id.ToString()), true);

            e.AddField("Max Bitrate", Code((server.MaxBitrate / 1000) + "kbps"), true);

            if (server.MaxMembers == null) add = "No Limit";
            else
            {
                add = server.MaxMembers + " User";
                if (server.MaxMembers != 1) add += "s";
            }
            e.AddField("Max Member Count", Code(add), true);

            if (server.MaxPresences == null) add = "No Limit";
            else
            {
                add = server.MaxPresences + " Presence";
                if (server.MaxPresences != 1) add += "s";
            }
            e.AddField("Max Presence Count", Code(add), true);

            add = server.MemberCount + " User";
            if (server.MemberCount != 1) add += "s";
            e.AddField("Current Member Count", Code(add), true);

            e.AddField("2FA Required to Moderate", Code(Convert.ToBoolean((int)server.MfaLevel).ToString()), true);

            e.AddField("Server Name", Code(server.Name), true);

            e.AddField("Server Owner", server.Owner.Mention, true);

            e.AddField("Preferred Culture", Code(server.PreferredCulture.Name), true);

            add = server.PremiumSubscriptionCount + " User";
            if (server.PremiumSubscriptionCount != 1) add += "s";
            e.AddField("Server Booster Count", Code(add), true);

            e.AddField("Boost Tier", Code(server.PremiumTier.ToString()), true);

            if (server.PublicUpdatesChannel == null) add = Code("No Public Updates Channel");
            else add = server.PublicUpdatesChannel.Mention;
            e.AddField("Public Updates Channel", add, true);

            add = server.Roles.Count + " Role";
            if (server.Roles.Count != 1) add += "s";
            e.AddField("Role Count", Code(add), true);

            // embed 1 done

            e1 = e.Build();

            e.Fields = new();
            e.Title = "Server Info about " + server.Name + " (Page 2 of 2)";

            // begin embed 2 fields

            if (server.RulesChannel == null) add = Code("No Rule Channel");
            else add = server.RulesChannel.Mention;
            e.AddField("Rule Channel", add, true);

            if (server.SplashUrl == null || server.SplashUrl == "") add = Code("No Splash");
            else add = Url("Click Here", server.SplashUrl);
            e.AddField("Splash", add, true);

            if (server.SystemChannel == null) add = Code("No System Channel");
            else add = server.SystemChannel.Mention;
            e.AddField("System Channel", add, true);

            add = server.TextChannels.Count + " Channel";
            if (server.TextChannels.Count != 1) add += "s";
            e.AddField("Text Channel Count", Code(add), true);

            if (server.VanityURLCode == null) add = Code("No Vanity URL");
            else add = EscapeUrl("https://discord.gg/" + server.VanityURLCode);
            e.AddField("Vanity URL", add, true);

            e.AddField("Verification Level", Code(server.VerificationLevel.ToString()), true);

            add = server.VoiceChannels.Count + " Channel";
            if (server.VoiceChannels.Count != 1) add += "s";
            e.AddField("Voice Channel Count", Code(add), true);

            e2 = e.Build();

            await ReplyAsync("", false, e1);
            await ReplyAsync("", false, e2);
        }
    }
}
