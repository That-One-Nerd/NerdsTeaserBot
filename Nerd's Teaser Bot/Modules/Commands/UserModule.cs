using Discord;
using Discord.Commands;
using Discord.Rest;
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
    [Name("User")]
    [Summary("Commands about a specific user")]
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        [Command("avatar")]
        [Summary("The avatar of the user executing the command")]
        public async Task Avatar() => await Avatar(Context.User);
        [Command("avatar")]
        [Summary("The avatar of a user")]
        public async Task Avatar([Summary("The mention of the user")] SocketUser user)
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
                Title = "Avatar for " + user.Username + "#" + user.Discriminator,
                ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 2048),
            };

            await ReplyAsync("", false, e.Build());
        }
        [Command("avatar")]
        [Summary("The avatar of a user (found by their ID)")]
        public async Task Avatar([Summary("The user ID of the user")] ulong userID) => await Avatar(Context.Client.GetUser(userID));

        [Command("userinfo")]
        [Summary("Info about the user executing the command")]
        public async Task Userinfo() => await Userinfo((RestUser)(IUser)Context.User);
        [Command("userinfo")]
        [Summary("Info about a user")]
        public async Task Userinfo([Summary("The mention of the user")] RestUser user)
        {
            string add = "";

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "*" + user.Mention + " ",
                Timestamp = DateTime.Now,
                Title = "User Info about " + user.Username + "#" + user.Discriminator,
                ThumbnailUrl = user.GetAvatarUrl(ImageFormat.Auto, 512),
            };

            if (Context.Guild.GetUser(user.Id) != null) e.Description += Bold("IS");
            else e.Description += "is " + Bold("NOT");

            e.Description += " in this server*";

            e.AddField("Full Name:", Code(user.Username + "#" + user.Discriminator), true);
            e.AddField("User ID:", Code(user.Id.ToString()), true);
            e.AddField("Mention:", user.Mention, true);

            add = "";
            if (user.ActiveClients.Count != 0) add = Code("None");
            foreach (ClientType c in user.ActiveClients) add += Code(c.ToString()) + "\n";
            e.AddField("Open Clients:", add, true);

            add = Code("None");

            if (user.Activity != null)
            {
                add = Italics("Activity:") + " " + Code(user.Activity.ToString()) + "\n";
                if (user.Activity.Name != null) add += Italics("Name:") + " " + Code(user.Activity.Name) + "\n";
                add += Italics("Type:") + " " + Code(user.Activity.Type.ToString()) + "\n";
                if (user.Activity.Details != null) add += Italics("Details:") + " " + Code(user.Activity.Details) + "\n";
                add += Italics("Flag:") + " " + Code(user.Activity.Flags.ToString());
            }

            e.AddField("Activity:", add, true);

            e.AddField("Account Created At:", Code(user.CreatedAt.DateTime.ToString()), true);
            e.AddField("Is Bot:", Code(user.IsBot.ToString()), true);
            e.AddField("Is Webhook:", Code(user.IsBot.ToString()), true);

            e.AddField("Status:", Code(user.Status.ToString()), true);

            SocketGuildUser usr = Context.Guild.GetUser(user.Id);
            if (usr != null)
            {
                add = "`" + Nerd_STF.Misc.PlaceMaker(usr.Hierarchy);
                if (usr.Hierarchy == int.MaxValue) add = "`Server Owner";
                add += "`";

                e.AddField("Hierarchy:", add, true);

                e.AddField("Deafened:", Code(usr.IsSelfDeafened.ToString()), true);
                e.AddField("Server Deafened:", Code(usr.IsDeafened.ToString()), true);
                e.AddField("Muted:", Code(usr.IsSelfDeafened.ToString()), true);
                e.AddField("Server Muted:", Code(usr.IsDeafened.ToString()), true);
                e.AddField("Streaming:", Code(usr.IsStreaming.ToString()), true);
                if (usr.JoinedAt.HasValue) e.AddField("Joined Server At:", Code(usr.JoinedAt.Value.DateTime.ToString()), true);

                add = "None";
                if (usr.Nickname != null) add = usr.Nickname;

                e.AddField("Nickname:", Code(add), true);

                add = "Not Boosting";
                if (usr.PremiumSince.HasValue) add = usr.PremiumSince.Value.DateTime.ToString();

                e.AddField("Server Boosting Since:", Code(add), true);

                add = "Not in Voice Channel";
                if (usr.VoiceChannel != null) add = usr.VoiceChannel.Name;

                e.AddField("Connected Voice Channel:", Code(add), true);

                add = "";

                foreach (SocketRole role in new List<SocketRole>(usr.Roles).FindAll(x => !x.IsEveryone && !x.IsManaged)) add += role.Mention + ", ";

                e.AddField("Roles (Ignoring Mandatory Roles):", add.Remove(add.Length - 2));
            }

            await ReplyAsync("", false, e.Build());
        }
        [Command("userinfo")]
        [Summary("Info about a user (found by their user ID)")]
        public async Task Userinfo([Summary("The user ID of the user")] ulong userID) => await Userinfo(await Context.Client.Rest.GetUserAsync(userID));
    }
}
