using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Extensions;
using NerdsTeaserBot.Modules.Models;
using Nerd_STF.Extensions;
using Nerd_STF.File.Saving;
using Nerd_STF.Lists;
using System;
using System.IO;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Group("mm")]
    [Name("Modmail")]
    [Summary("Commands about Modmail and it's functions")]
    public class ModmailModule : ModuleBase<SocketCommandContext>
    {
        [Command("anon")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Sends an anonymous message to the recipient")]
        public async Task Anon()
        {
            SocketTextChannel ch = (SocketTextChannel)Context.Channel;

            if (!ch.CategoryId.HasValue)
            {
                LogModule.LogMessage(LogSeverity.Error, "Channel is not a ModMail ticket channel (Channel is not in any category)");
                return;
            }
            if (ch.CategoryId != Data.consts.Data.modmailCategory)
            {
                LogModule.LogMessage(LogSeverity.Error, "Channel is not a ModMail ticket channel (Channel category is not the specified ModMail category. Use " + Code("n;mm category set") + " to set it!)");
                return;
            }
            if (ch.Topic.Trim().ToLower().Contains("archive"))
            {
                LogModule.LogMessage(LogSeverity.Error, "ModMail ticket has been archived");
                return;
            }

            ulong userId = ulong.Parse(ch.Topic.Substring(9, 18));

            EmbedAuthorBuilder a = new()
            {
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Name = "From ModMail Recipients",
            };

            await SendModMailMessage(a, Context.Message, await Internals.client.GetUser(userId).GetOrCreateDMChannelAsync());
            if (new List<IMessage>(await ch.GetMessagesAsync().FlattenAsync()).FindAll(x => x != null).Length < 4) await SuccessModMailMessage(ch, true);
        }

        [Command("archive")]
        [RequireUserPermission(GuildPermission.ManageMessages & GuildPermission.ManageChannels)]
        [Summary("Pauses communication in a ModMail thread and saves all communication in a text file (this does not delete the ticket)")]
        public async Task Archive([Remainder][Summary("The reason you have to archive the ticket")] string reason = "No Reason Specified")
        {
            if (Context.Channel is INestedChannel nest)
            {
                SocketTextChannel ch = (SocketTextChannel)Context.Channel;

                if (nest.CategoryId != Data.consts.Data.modmailCategory)
                {
                    LogModule.LogMessage(LogSeverity.Error, "Channel is not a ModMail ticket channel (Channel category is not the specified ModMail category. Use " + Code("n;mm category set") + " to set it!)");
                    return;
                }

                if (ch.Topic.Trim().ToLower().Contains("archive"))
                {
                    LogModule.LogMessage(LogSeverity.Error, "ModMail ticket has already been archived");
                    return;
                }

                ulong userId = ulong.Parse(ch.Topic.Substring(9, 18));
                IUser user = Context.Client.GetUser(userId);
                User usr = Data.users.Data.FindOrDefault(x => x.userID == userId);

                int num = 0;

                if (usr != null) num = usr.tickets - 1;

                string folder = Data.appPath + "/data/files-" + Context.Guild.Id + "/tickets/";

                TextFile save = new(folder + userId + "-" + num + ".ticket");

                string add = "Nerd's Teaser Bot - Ticket Transcript\n" +
                             "=========================\n" +
                             "Ticket Info is below:\n" +
                             "\n" +
                             "Archiving Reason: " + reason + "\n" + 
                             "=========================\n" +
                             "User Info is below:\n" +
                             "\n" +
                             "Full Name: ";

                if (user == null) add += "-- Unknown --";
                else add += user.Username + "#" + user.Discriminator;
                add += "\n";

                add += "User ID: ";
                if (user == null) add += "-- Unknown --";
                else add += user.Id.ToString();
                add += "\n";

                add += "User Is Muted: ";
                if (usr == null || usr.currentMute == null) add += "False";
                else add += usr.currentMute.IsMuted.ToString();
                add += "\n";

                add += "Ticket Count: ";
                if (usr == null) add += "1 Ticket";
                else
                {
                    add += usr.tickets + " Ticket";
                    if (usr.tickets != 1) add += "s";
                }
                add += "\n";

                add += "Warn Count: ";
                if (usr == null || usr.warns is null) add += "0 Warns";
                else
                {
                    add += usr.warns.Length + " Warns";
                    if (usr.warns.Length != 1) add += "s";
                }
                add += "\n";

                add += "=========================\n" +
                       "-- Begin Transcript --\n" +
                       "\n";

                List<IUserMessage> messages = new List<IUserMessage>(await ch.GetMessagesAsync(1000).FlattenAsync()).FindAll(x => x != null);
                messages.Reverse();

                foreach (IUserMessage msg in messages)
                {
                    add += msg.Author.Username + "#" + msg.Author.Discriminator;
                    if (msg.Author.IsBot) add += " [BOT]";

                    add += " (" + msg.Timestamp.DateTime + "):\n";
                    if (msg.Content != null && msg.Content != "") add += msg.Content.Replace("\n", "\n    ") + "\n";
                    
                    List<Embed> embeds = new List<Embed>(msg.Embeds).FindAll(x => x != null);
                    for (int i = 0; i < embeds.Length; i++)
                    {
                        EmbedBuilder embed = embeds[i].ToEmbedBuilder();

                        add += "Embed " + (i + 1) + "/" + embeds.Length + ":\n";

                        if (embed.Author != null && embed.Author != new EmbedAuthorBuilder())
                        {
                            add += "    Author:\n" +
                                   "        Icon Url: " + (embed.Author.IconUrl ?? "-- None --") + "\n" +
                                   "        Name: " + (embed.Author.Name ?? "-- None --").Replace("\n", "\n            ") + "\n" +
                                   "        Url: " + (embed.Author.Url ?? "-- None --") + "\n";
                        }

                        if (embed.Description != null && embed.Description != "") add += "    Description: " + embed.Description + "\n";

                        if (embed.Fields != null && embed.Fields != new System.Collections.Generic.List<EmbedFieldBuilder>())
                        {
                            for (int j = 0; j < embed.Fields.Count; j++)
                            {
                                add += "    Field " + (j + 1) + "/" + embed.Fields.Count + ":\n" +
                                       "        Name: " + (embed.Fields[j].Name ?? "-- None --").Replace("\n", "\n            ") + "\n" +
                                       "        Value: " + (embed.Fields[j].Value ?? "-- None --").ToString().Replace("\n", "\n            ") + "\n";
                            }
                        }

                        if (embed.Footer != null && embed.Footer != new EmbedFooterBuilder())
                        {
                            add += "    Footer:\n" +
                                   "        Icon Url: " + (embed.Footer.IconUrl ?? "-- None --") + "\n" +
                                   "        Text: " + (embed.Footer.Text ?? "-- None --").Replace("\n", "\n            ") + "\n";
                        }

                        if (embed.ImageUrl != null && embed.ImageUrl != "") add += "    Image Url: " + embed.ImageUrl + "\n";
                        if (embed.ThumbnailUrl != null && embed.ThumbnailUrl != "") add += "    Thumbnail Url: " + embed.ThumbnailUrl + "\n";
                        if (embed.Timestamp.HasValue && embed.Timestamp.Value.DateTime != DateTime.MinValue) add += "    Timestamp: " + embed.Timestamp.Value.DateTime + "\n";
                        if (embed.Title != null && embed.Title != "") add += "    Title: " + embed.Title.Replace("\n", "\n        ") + "\n";
                        if (embed.Url != null && embed.Url != "") add += "    Url: " + embed.Url + "\n";
                    }

                    List<Attachment> attachments = new List<Attachment>(msg.Attachments).FindAll(x => x != null);
                    for (int i = 0; i < attachments.Length; i++)
                    {
                        Attachment att = attachments[i];

                        add += "Attachment " + (i + 1) + "/" + attachments.Length + ":\n" +
                               "    Filename: " + att.Filename + "\n" +
                               "    ID: " + att.Id + "\n" +
                               "    Size: " + att.Size + " Byte";
                        if (att.Size != 1) add += "s";
                        add += "\n" +
                               "    Url: " + att.Url;
                    }

                    add += "\n\n";
                }

                add += "-- End Transcript --\n" +
                       "=========================\n";

                save.Data = add;

                Directory.CreateDirectory(folder);
                save.Save();

                EmbedBuilder e = new()
                {
                    Color = Color.Red,
                    Description = "This ticket has been archived and closed for reason: " + Code(reason),
                    Footer = new() { Text = "Reply to create a new ticket" },
                    Timestamp = DateTime.Now,
                    Title = "Ticket Archived",
                };

                Modify(x =>
                {
                    x.Name = user.Username + user.Discriminator + "-archived";
                    x.Topic = "User: " + user.Mention + " (ARCHIVED)";
                });

                await user.DMUserAsync("", false, e.Build());

                e.Description += ". The ticket transcript is attached above";
                e.Footer = null;

                await Context.Channel.SendFileAsync(save.Path, "", false, e.Build());

                void Modify(Action<TextChannelProperties> func) => ch.ModifyAsync(func);
            }
            else
            {
                LogModule.LogMessage(LogSeverity.Error, "Channel is not a ModMail ticket channel (Channel is not in any category)");
                return;
            }
        }

        [Command("category get")]
        [Summary("The current modmail category for the bot")]
        public async Task CategoryGet()
        {
            SocketCategoryChannel c = null;
            if (Data.consts.Data.modmailCategory != default)
            {
                foreach (SocketGuild g in Context.Client.Guilds)
                {
                    SocketCategoryChannel c2 = new List<SocketCategoryChannel>(g.CategoryChannels).FindOrDefault(x => x.Id == Data.consts.Data.modmailCategory);
                    if (c2 != default)
                    {
                        c = c2;
                        break;
                    }
                }

                if (c == null)
                {
                    Data.consts.Data.modmailCategory = default;
                    LogModule.LogMessage(LogSeverity.Error, "The Modmail category found was no longer accessible by the bot, so it was reset");
                    return;
                }
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "No current modmail category found. Use " + Code("n;mm category set") + " to set it!",
                Timestamp = DateTime.Now,
                Title = "Current Modmail Category",
            };

            if (Data.consts.Data.modmailCategory != default && c != null) e.Description = "The current modmail category is " + Italics(c.Name) + " in the server " + Italics(c.Guild.Name + " (" + Code(c.Guild.Id.ToString()) + ")");

            await ReplyAsync("", false, e.Build());
        }

        [Command("category reset")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Resets the changelog channel of the bot back to none")]
        public async Task CategoryReset()
        {
            Data.consts.Data.modmailCategory = default;

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = "Successfully set the current modmail category to: " + Code("None"),
                Timestamp = DateTime.Now,
                Title = "Reset Modmail Category",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("category set")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Sets the given category to use for modmail tickets")]
        public async Task CategorySet([Summary("The category to use for modmail tickets")] SocketCategoryChannel cat)
        {
            if (!new List<SocketGuild>(Context.Client.Guilds).Any(x => new List<SocketCategoryChannel>(x.CategoryChannels).Contains(cat)))
            {
                LogModule.LogMessage(LogSeverity.Error, "Category is not found in any of the bot's known servers");
                return;
            }

            ulong old = Data.consts.Data.modmailCategory;
            
            Data.consts.Data.modmailCategory = cat.Id;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set modmail category to: " + Italics(cat.Name) + " in the server " + Italics(cat.Guild.Name + " (" + Code(cat.Guild.Id.ToString()) + ")"),
                Timestamp = DateTime.Now,
                Title = "Set modmail category",
            };

            SocketCategoryChannel oldC = null;
            foreach (SocketGuild g in Context.Client.Guilds)
            {
                SocketCategoryChannel tempO = new List<SocketCategoryChannel>(g.CategoryChannels).FindOrDefault(x => x.Id == old);
                if (tempO == default) continue;
                oldC = tempO;
                break;
            }

            if (oldC != null) e.Description += "\n(Previous: " + Italics(oldC.Name) + " in the server " + Italics(oldC.Guild.Name + " (" + Code(oldC.Guild.Id.ToString()) + ")") + ")";

            await ReplyAsync("", false, e.Build());
        }

        [Command("unarchive")]
        [RequireUserPermission(GuildPermission.ManageChannels & GuildPermission.ManageChannels)]
        [Summary("Resumes communication in an archived ModMail thread and saves all communication in a text file")]
        public async Task Unarchive([Remainder][Summary("The reason you have to unarchive the ticket")] string reason = "No Reason Specified")
        {
            if (Context.Channel is INestedChannel nest)
            {
                SocketTextChannel ch = (SocketTextChannel)Context.Channel;

                if (nest.CategoryId != Data.consts.Data.modmailCategory)
                {
                    LogModule.LogMessage(LogSeverity.Error, "Channel is not a ModMail ticket channel (Channel category is not the specified ModMail category. Use " + Code("n;mm category set") + " to set it!)");
                    return;
                }

                if (!ch.Topic.Trim().ToLower().Contains("archive"))
                {
                    LogModule.LogMessage(LogSeverity.Error, "ModMail ticket has not been archived");
                    return;
                }

                ulong userId = ulong.Parse(ch.Topic.Substring(9, 18));
                IUser user = Context.Client.GetUser(userId);

                SocketTextChannel other = new List<SocketTextChannel>(Context.Guild.TextChannels).FindAll(x => x.Category == ch.Category).FindOrDefault(x => x.Topic.Trim().StartsWith("User: " + user.Mention) && !x.Topic.Trim().ToLower().Contains("archive"));
                if (other != default)
                {
                    LogModule.LogMessage(LogSeverity.Error, "There is already an open ticket for this user, " + other.Mention);
                    return;
                }

                EmbedBuilder e = new()
                {
                    Color = Color.Green,
                    Description = "This ticket has been unarchived and reopened for reason: " + Code(reason),
                    Footer = new() { Text = "Answering ModMail tickets will now go through this ticket" },
                    Timestamp = DateTime.Now,
                    Title = "Ticket Unarchived",
                };

                Modify(x =>
                {
                    x.Name = user.Username + user.Discriminator;
                    x.Topic = "User: " + user.Mention;
                });

                await user.DMUserAsync("", false, e.Build());

                e.Footer = null;

                await ReplyAsync("", false, e.Build());

                void Modify(Action<TextChannelProperties> func) => ch.ModifyAsync(func);
            }
            else
            {
                LogModule.LogMessage(LogSeverity.Error, "Channel is not a ModMail ticket channel (Channel is not in any category)");
                return;
            }
        }

        // end commands

        public static async Task ModmailHandler(SocketMessage message)
        {
            if (message is SocketUserMessage msg)
            {
                if (msg.Author.Id == ID) return;
                if (msg.Content.Trim().StartsWith(";") || msg.Content.Trim().StartsWith("n;") || msg.Content.Trim().StartsWith("m;")) return;

                EmbedBuilder e;

                if (msg.Channel is SocketDMChannel)
                {
                    SocketCategoryChannel cat = null;
                    foreach (SocketGuild g in Internals.context.Client.Guilds)
                    {
                        SocketCategoryChannel tempCat = new List<SocketCategoryChannel>(g.CategoryChannels).FindOrDefault(x => x.Id == Data.consts.Data.modmailCategory);
                        if (tempCat == default) continue;
                        cat = tempCat;
                        break;
                    }

                    if (cat == null) return;

                    Data.TryLoadAll(cat.Guild.Id);

                    ITextChannel ch = null;
                    foreach (SocketGuildChannel tCh in cat.Channels) if (tCh is SocketTextChannel textCh && textCh.Topic == "User: " + msg.Author.Mention) ch = textCh;

                    e = new()
                    {
                        Color = Colors.DefaultColor,
                        Timestamp = DateTime.Now,
                    };

                    if (ch == null)
                    {
                        ch = await cat.Guild.CreateTextChannelAsync(msg.Author.Username + msg.Author.Discriminator, x => 
                        {
                            x.CategoryId = cat.Id;
                            x.Topic = "User: " + msg.Author.Mention;
                        });

                        e.Description = "Info on the thread creator is below";
                        e.Title = "New ModMail Thread Created";

                        e.AddField("User Full Name", Code(msg.Author.Username + "#" + msg.Author.Discriminator), true);
                        e.AddField("User Id", Code(msg.Author.Id.ToString()), true);
                        e.AddField("User Mention", msg.Author.Mention, true);

                        int count = 0;
                        User usr = Data.users.Data.FindOrDefault(x => x.userID == msg.Author.Id);
                        if (usr == default)
                        {
                            usr = new()
                            {
                                currentMute = null,
                                tickets = 1,
                                userID = msg.Author.Id,
                            };
                            count = 1;
                            Data.users.Data.Add(usr);
                        }
                        else
                        {
                            usr.tickets++;
                            count = usr.tickets;
                        }

                        bool muted;
                        if (usr.currentMute == null) muted = false;
                        else muted = usr.currentMute.IsMuted;

                        e.AddField("User is Muted", Code(muted.ToString()), true);

                        string s = "";
                        if (count != 1) s += "s";
                        e.AddField("Ticket Count", Code(count + " Ticket" + s), true);

                        if (usr.warns is null) count = 0;
                        else count = usr.warns.Length;

                        s = "";
                        if (count != 1) s += "s";
                        e.AddField("Warn Count", Code(count + " Warn" + s), true);

                        await ch.SendMessageAsync("", false, e.Build());
                        await ch.SyncPermissionsAsync();
                        await SuccessModMailMessage(await msg.Author.GetOrCreateDMChannelAsync(), true);
                    }

                    EmbedAuthorBuilder a = new()
                    {
                        IconUrl = msg.Author.GetAvatarUrl(),
                        Name = "From " + msg.Author.Username + "#" + msg.Author.Discriminator,
                    };

                    await SendModMailMessage(a, msg, ch);

                    Data.SaveAll(cat.Guild.Id);
                }
                else if (msg.Channel is SocketTextChannel ch)
                {
                    if (!ch.CategoryId.HasValue) return;
                    if (ch.CategoryId != Data.consts.Data.modmailCategory) return;
                    if (ch.Topic.Trim().ToLower().Contains("archive")) return;

                    ulong userId = ulong.Parse(ch.Topic.Substring(9, 18));

                    EmbedAuthorBuilder a = new()
                    {
                        IconUrl = msg.Author.GetAvatarUrl(),
                        Name = "From " + msg.Author.Username + msg.Author.Discriminator,
                    };

                    await SendModMailMessage(a, msg, await Internals.client.GetUser(userId).GetOrCreateDMChannelAsync());
                    if (new List<IMessage>(await ch.GetMessagesAsync().FlattenAsync()).FindAll(x => x != null).Length < 4) await SuccessModMailMessage(ch, true);
                }
            }
        }

        public static async Task SendModMailMessage(EmbedAuthorBuilder a, IMessage msg, IMessageChannel ch)
        {
            EmbedBuilder e = new()
            {
                Author = a,
                Color = Colors.DefaultColor,
                Description = msg.Content,
                Title = "ModMail Message Recieved",
            };

            List<Attachment> attachments = new(msg.Attachments);
            if (attachments.Length == 1) e.ImageUrl = attachments[0].Url;

            IUserMessage sent = await ch.SendMessageAsync("", false, e.Build());

            if (attachments.Length > 1)
            {
                for (int i = 0; i < attachments.Length; i++)
                {
                    e.Author = null;
                    e.Description = "This ModMail message has been send more than one attachment, and as such must be broken down into several embeds";
                    e.ImageUrl = attachments[i].Url;
                    e.Title = "ModMail Message Attachment " + (i + 1) + "/" + attachments.Length;

                    await sent.ReplyAsync("", false, e.Build());
                }
            }
        }
        public static async Task SuccessModMailMessage(IMessageChannel ch, bool appearsOnce = false)
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Your message has been sent to the ModMail recievers, and they will get back to you as soon as possible!",
                Timestamp = DateTime.Now,
                Title = "ModMail Message Sent",
            };
            if (appearsOnce) e.WithFooter("This message will appear only once per ticket");

            await ch.SendMessageAsync("", false, e.Build());
        }
    }
}
