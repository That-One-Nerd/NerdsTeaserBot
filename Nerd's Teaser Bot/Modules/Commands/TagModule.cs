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
    [Group("tag")]
    [Name("Tag")]
    [Summary("Commands used to create, remove, and view all tags")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command("delete")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes a tag with the specified name")]
        public async Task Delete([Summary("The name of the tag to delete")] string name)
        {
            Tag tag = Data.misc.Data.tags.FindOrDefault(x => x.name == name.Trim().ToLower() || "$" + x.name == name.Trim().ToLower());

            if (tag == null)
            {
                LogModule.LogMessage(LogSeverity.Error, "Tag does not exist in the database of created tags");
                return;
            }
            else if (tag.owner == Context.User.Id)
            {
                LogModule.LogMessage(LogSeverity.Error, "You do not own this tag, and as such cannot delete it");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = "Using " + Code("$" + tag.name) + " will no longer trigger any sort of response from the bot",
                Timestamp = DateTime.Now,
                Title = "Tag Deleted",
            };

            Data.misc.Data.tags.Remove(tag);

            await ReplyAsync("", false, e.Build());
        }

        [Command("info")]
        [Summary("Shows info on a given tag")]
        public async Task Info([Summary("The name of the tag to show info of")] string name)
        {
            Tag tag = Data.misc.Data.tags.FindOrDefault(x => x.name == name.Trim().ToLower() || "$" + x.name == name.Trim().ToLower());

            if (tag == null)
            {
                LogModule.LogMessage(LogSeverity.Error, "Tag does not exist in the database of created tags");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
                Title = "Info on $" + tag.name,
            };
            e.AddField("Tag Owner", "<@" + tag.owner + ">", true);
            e.AddField("Tag Name", "$" + tag.name, true);
            e.AddField("Response", tag.response);

            await ReplyAsync("", false, e.Build());
        }

        [Command("list")]
        [Summary("Shows all currently existing tags and their owners")]
        public async Task List()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = Data.misc.Data.tags.Length + " Tags in List",
            };

            foreach (Tag t in Data.misc.Data.tags)
            {
                string res = t.response.Replace("\n", " ");
                if (res.Length > 50) res = res.Remove(47) + "…";
                e.Description += "<@" + t.owner + "> - " + Code("$" + t.name) + ": " + res + "\n";
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("set")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Edit a tag if it is found, otherwise creates one")]
        public async Task Set([Summary("The name of the tag to add/edit")] string name, [Remainder][Summary("The summary of the tag to make/change")] string response)
        {
            Tag tag = Data.misc.Data.tags.FindOrDefault(x => x.name.ToLower().Trim() == name.ToLower().Trim() || "$" + x.name == name.Trim().ToLower());

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = " will now trigger the bot to respond with: ",
                Timestamp = DateTime.Now,
                Title = "Tag "
            };

            if (tag == default)
            {
                tag = new()
                {
                    name = name,
                    owner = Context.User.Id,
                    response = response.Trim(),
                };
                Data.misc.Data.tags.Add(tag);
            }
            else
            {
                if (tag.owner != Context.User.Id)
                {
                    LogModule.LogMessage(LogSeverity.Error, "You do not own this tag, and as such cannot edit it");
                    return;
                }
                tag.response = response.Trim();
            }

            if (tag.response.Contains("\n")) e.Description += "\n" + tag.response;
            else e.Description += tag.response;
            e.Title += "Edited";
            e.Description = Code("$" + tag.name) + e.Description;

            await ReplyAsync("", false, e.Build());
        }

        [Command("transfer")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Transfers ownership of a tag from one user to another")]
        public async Task Transfer([Summary("The user to make the new owner")] IGuildUser user, [Summary("The name of the tag to transfer owners of")] string name)
        {
            Tag tag = Data.misc.Data.tags.FindOrDefault(x => x.name == name.Trim().ToLower() || x.name == "$" + name.Trim().ToLower());

            if (tag == null)
            {
                LogModule.LogMessage(LogSeverity.Error, "Tag does not exist in the database of created tags");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "<@" + tag.owner + "> has transfered the tag ownership of " + Code("$" + tag.name) + " now belongs to " + user.Mention,
                Timestamp = DateTime.Now,
                Title = "Tag Ownership Transferred",
            };

            tag.owner = user.Id;

            await ReplyAsync("", false, e.Build());
        }

        // end commands

        public static async Task TagHandler(SocketUserMessage msg)
        {
            List<string> words = new(msg.Content.Split(" "));

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
            };

            foreach (Tag t in Data.misc.Data.tags)
            {
                if (words.Contains(x => x.Trim().ToLower() == "$" + t.name))
                {
                    e.Description = t.response;
                    e.Title = "$" + t.name;

                    if (msg.ReferencedMessage == null) await msg.ReplyAsync("", false, e.Build());
                    else await msg.ReferencedMessage.ReplyAsync("", false, e.Build());
                }
            }
        }
    }
}
