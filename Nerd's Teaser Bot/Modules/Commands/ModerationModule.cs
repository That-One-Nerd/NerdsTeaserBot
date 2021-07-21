using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules;
using NerdsTeaserBot.Modules.Extensions;
using NerdsTeaserBot.Modules.Models;
using Nerd_STF;
using Nerd_STF.Lists;
using System;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Name("Moderation")]
    [Summary("Commands about moderating and watching over users")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user from the server. The user can always join back with an invite")]
        public async Task Kick([Summary("The user to kick")] SocketGuildUser user, [Remainder][Summary("The reason to kick the user for. The user will be DMed this reason")] string reason = "No Reason Specified")
        {
            bool dat = await MainKickSystem(user, reason);

            if (dat) return;

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = "You have been kicked by " + Code(Context.User.Username + "#" + Context.User.Discriminator) + " for reason: " + Code(reason),
                Timestamp = DateTime.Now,
                Title = "You have been kicked from " + Context.Guild.Name,
            };

            await user.DMUserAsync("", false, e.Build());
        }

        [Command("kick ghost")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user from the server (without DMing them). The user can always join back with an invite")]
        public async Task KickGhost([Summary("The user to kick")] SocketGuildUser user, [Remainder][Summary("The reason to kick the user for")] string reason = "No Reason Specified") => await MainKickSystem(user, reason);

        [Command("message")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Send a message a user via a DM (This is not the way to respond to a ModMail message)")]
        public async Task Message([Summary("The user to DM")] IUser user, [Remainder][Summary("The message to DM to the user")] string message)
        {
            EmbedAuthorBuilder a = new()
            {
                IconUrl = Context.User.GetAvatarUrl(),
                Name = "Message from " + Context.User.Username + "#" + Context.User.Discriminator,
            };

            await MainMessageSystem(user, a, message);
        }

        [Command("message anon")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Send a message a user via a DM (This is not the way to respond to a ModMail message). Going anonymous hides your username from the recipient")]
        public async Task MessageAnon([Summary("The user to DM")] IUser user, [Remainder][Summary("The message to DM to the user")] string message)
        {
            EmbedAuthorBuilder a = new()
            {
                IconUrl = Context.Guild.IconUrl,
                Name = "Message from ",
            };

            if (((SocketGuildUser)Context.User).Roles.Count == 1) a.Name += "a user";
            else
            {
                SocketRole highest = null;
                foreach (SocketRole r in ((SocketGuildUser)Context.User).Roles)
                {
                    if (highest == null) highest = r;
                    else if (highest.Position < r.Position) highest = r;
                }

                int count = new List<SocketGuildUser>(Context.Guild.Users).FindAll(x => new List<SocketRole>(x.Roles).Contains(highest)).Length;

                if (count == 1) a.Name += "a " + highest.Name;
                else
                {
                    a.Name += "the " + highest.Name;
                    if (!highest.Name.EndsWith("s")) a.Name += "s";
                }
            }

            a.Name += " in " + Context.Guild.Name;

            await MainMessageSystem(user, a, message);
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageGuild & GuildPermission.ManageRoles)]
        [Summary("Mutes a user for a specified amount of time")]
        public async Task Mute([Summary("The user to mute")] IUser user, [Summary("The time to mute for. Use '---' for no limit")] string time, [Remainder][Summary("The reason to mute the user for")] string reason = "No Reason Specified")
        {
            DateTime release = DateTime.Now;

            string timeS = "";
            if (time == "---")
            {
                release = DateTime.MaxValue;
                timeS = "indefinitely";
            }
            else if (!int.TryParse(time.Remove(time.Length - 1), out _))
            {
                LogModule.LogMessage(LogSeverity.Error, "Time was not in the correct format" +
                    "\n(must include a unit of time at the end of the string)");
                return;
            }
            else
            {
                (int, char) dat = new();
                dat.Item1 = int.Parse(time[0..(time.Length - 1)]);
                dat.Item2 = time[^1];

                switch (dat.Item2)
                {
                    case 's':
                        release = release.AddSeconds(dat.Item1);
                        timeS = dat.Item1 + " Seconds";
                        break;

                    case 'm':
                        release = release.AddMinutes(dat.Item1);
                        timeS = dat.Item1 + " Minutes";
                        break;

                    case 'h':
                        release = release.AddHours(dat.Item1);
                        timeS = dat.Item1 + " Hours";
                        break;

                    case 'd':
                        release = release.AddDays(dat.Item1);
                        timeS = dat.Item1 + " Days";
                        break;

                    case 'w':
                        release = release.AddDays(dat.Item1 * 7);
                        timeS = dat.Item1 + " Weeks";
                        break;

                    case 'y':
                        release = release.AddDays(dat.Item1 * 365.24238);
                        timeS = dat.Item1 + " Years";
                        break;

                    default:
                        LogModule.LogMessage(LogSeverity.Error, "Time was not in the correct format" +
                            "\n(unit of time was incorrect)");
                        return;
                }
            }

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = user.Mention + " has been muted ",
                Timestamp = DateTime.Now,
                Title = "User has been muted"
            };

            if (timeS.ToLower().Trim() != "indefinitely") e.Description += "for ";
            e.Description += Code(timeS) + " for reason: " + Code(reason);

            Mute m = new()
            {
                moderator = Context.User.Id,
                reason = reason,
                release = release,
                start = DateTime.Now,
                unmute = null,
            };
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);
            if (usr == default)
            {
                Data.users.Data.Add(new User()
                {
                    currentMute = m,
                    userID = user.Id,
                    warns = new(),
                });
            }
            else usr.currentMute = m;

            Embed b1 = e.Build();

            e.Description = "You have been muted for ";

            if (timeS.ToLower().Trim() != "indefinitely") e.Description += "for ";
            e.Description += Code(timeS) + " by " + Context.User.Mention + " for reason: " + Code(reason);

            e.Title = "You have been muted in " + Context.Guild.Name;

            e.WithFooter("Your mute expires at: " + release.ToString());

            Embed b2 = e.Build();

            await user.DMUserAsync("", false, b2);

            await ReplyAsync("", false, b1);
        }
        // these two commands have practically the same code. remember to change both when editing code
        [Command("mute ghost")]
        [RequireUserPermission(GuildPermission.ManageGuild & GuildPermission.ManageRoles)]
        [Summary("Mutes a user for a specified amount of time (without DMing them)")]
        public async Task MuteGhost([Summary("The user to mute")] IUser user, [Summary("The time to mute for. Use '---' for no limit")] string time, [Remainder][Summary("The reason to mute the user for")] string reason = "No Reason Specified")
        {
            DateTime release = DateTime.Now;

            string timeS = "";
            if (time == "---")
            {
                release = DateTime.MaxValue;
                timeS = "indefinitely";
            }
            else if (!int.TryParse(time.Remove(time.Length - 1), out _))
            {
                LogModule.LogMessage(LogSeverity.Error, "Time was not in the correct format" +
                    "\n(must include a unit of time at the end of the string)");
                return;
            }
            else
            {
                (int, char) dat = new();
                dat.Item1 = int.Parse(time[0..(time.Length - 1)]);
                dat.Item2 = time[^1];

                switch (dat.Item2)
                {
                    case 's':
                        release = release.AddSeconds(dat.Item1);
                        timeS = dat.Item1 + " Seconds";
                        break;

                    case 'm':
                        release = release.AddMinutes(dat.Item1);
                        timeS = dat.Item1 + " Minutes";
                        break;

                    case 'h':
                        release = release.AddHours(dat.Item1);
                        timeS = dat.Item1 + " Hours";
                        break;

                    case 'd':
                        release = release.AddDays(dat.Item1);
                        timeS = dat.Item1 + " Days";
                        break;

                    case 'w':
                        release = release.AddDays(dat.Item1 * 7);
                        timeS = dat.Item1 + " Weeks";
                        break;

                    case 'y':
                        release = release.AddDays(dat.Item1 * 365.24238);
                        timeS = dat.Item1 + " Years";
                        break;

                    default:
                        LogModule.LogMessage(LogSeverity.Error, "Time was not in the correct format" +
                            "\n(unit of time was incorrect)");
                        return;
                }
            }

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = user.Mention + " has been muted ",
                Timestamp = DateTime.Now,
                Title = "User has been muted"
            };

            if (timeS.ToLower().Trim() != "indefinitely") e.Description += "for ";
            e.Description += Code(timeS) + " for reason: " + Code(reason);

            Mute m = new()
            {
                moderator = Context.User.Id,
                reason = reason,
                release = release,
                start = DateTime.Now,
                unmute = null,
            };
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);
            if (usr == default)
            {
                Data.users.Data.Add(new User()
                {
                    currentMute = m,
                    userID = user.Id,
                    warns = new(),
                });
            }
            else usr.currentMute = m;

            await ReplyAsync("", false, e.Build());
        }

        [Command("mute info")]
        [Summary("Shows mute data on the user executing the command")]
        public async Task MuteInfo() => await MuteInfo(Context.User);

        [Command("mute info")]
        [Summary("Shows mute data on a specific person")]
        public async Task MuteInfo([Summary("The user to show info on")] IUser user)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == default)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            Mute mute = usr.currentMute;

            if (mute == null)
            {
                mute = new();
                usr.currentMute = mute;
            }

            EmbedBuilder e = new()
            {
                Timestamp = DateTime.Now,
                Title = "User is ",
            };

            e.AddField("Mute Info", Italics("Muted At:") + " " + Code(mute.start.ToString()) +
                "\n" + Italics("Will Unmute At:") + " " + Code(mute.release.ToString()) +
                "\n" + Italics("Muted By:") + " <@" + mute.moderator + ">" +
                "\n" + Italics("Mute Reason:") + " " + Code(mute.reason));
            
            if (mute.IsMuted)
            {
                e.Color = Color.Red;
                e.Description = "Mute info is below:";
            }
            else
            {
                Mute.Unmute unmute = mute.unmute;

                e.Color = Color.Green;
                e.Description = "Mute and unmute info is below:";
                e.Title += "not ";

                e.AddField("Unmute Info", Italics("Unmuted At:") + " " + Code(unmute.time.ToString()) +
                    "\n" + Italics("Unmuted By:") + " <@" + unmute.moderator + ">" +
                    "\n" + Italics("Unmute Reason:") + " " + Code(unmute.reason));
            }
            e.Title += "currently muted";

            await ReplyAsync("", false, e.Build());
        }
        
        [Command("warn")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Warns a user")]
        public async Task Warn([Summary("The user to warn")] IUser user, [Remainder][Summary("The reason to warn the user with")] string reason = "No Reason Specified")
        {
            (EmbedBuilder, int, bool) dat = await MainWarnSystem(user, reason);

            if (dat.Item3) return;

            dat.Item1.Description = "You have been warned by " + Context.User.Mention + " for reason: " + Code(reason) +
                "\n" + Italics("This is your " + Code(Nerd_STF.Misc.PlaceMaker(dat.Item2)) + " warning.");
            dat.Item1.Title = "You have been warned in " + Context.Guild.Name + "!";

            await user.DMUserAsync("", false, dat.Item1.Build());
        }

        [Command("warn change")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Changes the warn reason of a specific warn")]
        public async Task WarnChange([Summary("The user to change the warn of")] IUser user, [Summary("The hash of the warn to change")] string hash, [Remainder] [Summary("The new reason to set the warn to")] string newReason)
        {
            if (Data.users.Data is null) Data.users.Data = new();

            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            if (usr.warns is null)
            {
                LogModule.LogMessage(LogSeverity.Error, "User has no tracked warns, and as such cannot find a specific one");
                return;
            }

            Warn warn = usr.warns.FindOrDefault(x => x.hash == hash);

            if (warn == null)
            {
                LogModule.LogMessage(LogSeverity.Error, "No warn exists on this user with the specified hash");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully changed " + user.Mention + "'s warn with reason " + Code(warn.reason) + " to have a new reason of " + Code(newReason),
                Timestamp = DateTime.Now,
                Title = "Warn reason changed"
            };

            warn.reason = newReason;

            await ReplyAsync("", false, e.Build());
        }

        [Command("warn clear")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Clears a user of all warns currently tracked")]
        public async Task WarnClear([Summary("The user to clear the warns of")] IUser user)
        {
            (int, bool) dat = await MainWarnClearSystem(user);

            if (dat.Item2) return;

            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = "Your " + Code(dat.Item1.ToString()) + " warn",
                Timestamp = DateTime.Now,
                Title = "All warns cleared in " + Context.Guild.Name,
            };

            if (dat.Item1 != 1) e.Description += "s";

            e.Description += " have now been reset. Great job!";

            await user.DMUserAsync("", false, e.Build());
        }

        [Command("warn clear ghost")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Clears a user of all warns currently tracked (without DMing them)")]
        public async Task WarnClearGhost([Summary("The user to clear the warns of")] IUser user) => await MainWarnClearSystem(user);

        [Command("warn delete")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Deletes a warn with a given hash from a user's warn list")]
        public async Task WarnDelete([Summary("The user to delete the warn of")] IUser user, [Summary("The hash of the warn to delete")] string hash)
        {
            (User, Warn, bool) dat = await MainWarnDeleteSystem(user, hash);
            if (dat.Item3) return;

            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = "One of your warnings has been deleted by " + Context.User.Mention +
                    "\n" + Italics("More info about your deleted warning is below.") +
                    "\n" +
                    "\n" + Italics("Reason:") + " " + Code(dat.Item2.reason) +
                    "\n" + Italics("Moderator:") + " <@" + dat.Item2.moderator + ">" +
                    "\n" + Italics("Time:") + " " + Code(dat.Item2.time.ToString()) +
                    "\n" + Italics("Hash:") + " " + Code(dat.Item2.hash),
                Timestamp = DateTime.Now,
                Title = "Warn Deleted in " + Context.Guild.Name,
            };
            e.WithFooter("Use 'n;warn list' to see your total tracked warns!");

            await user.DMUserAsync("", false, e.Build());
        }

        [Command("warn delete ghost")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Deletes a warn with a given hash from a user's warn list (without DMing them)")]
        public async Task WarnDeleteGhost([Summary("The user to delete the warn of")] IUser user, [Summary("The hash of the warn to delete")] string hash) => await MainWarnDeleteSystem(user, hash);

        [Command("warn ghost")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Warns a user (without DMing them)")]
        public async Task WarnGhost([Summary("The user to warn")] IUser user, [Remainder][Summary("The reason to warn the user with")] string reason = "No Reason Specified") => await MainWarnSystem(user, reason + " [GHOST]");
        
        [Command("warn info")]
        [Summary("Shows info about a user's warn")]
        public async Task WarnInfo([Summary("The user to find the warn of")] IUser user, [Summary("The hash of the warn to show")] string hash)
        {
            if (Data.users.Data is null) Data.users.Data = new();

            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            if (usr.warns is null) usr.warns = new();

            Warn warn = usr.warns.FindOrDefault(x => x.hash == hash);

            if (warn == null)
            {
                LogModule.LogMessage(LogSeverity.Error, "No warn exists on this user with the specified hash");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + "'s warning info is below" +
                    "\n" +
                    "\n" + Italics("Reason:") + " " + Code(warn.reason) +
                    "\n" + Italics("Moderator:") + " <@" + warn.moderator + ">" +
                    "\n" + Italics("Time:") + " " + Code(warn.time.ToString()) +
                    "\n" + Italics("Hash:") + " " + Code(warn.hash),
                Timestamp = DateTime.Now,
                Title = "Warning Info",
            };
            e.WithFooter("Use 'n;warn list' to see the total tracked warns of this user!");

            await ReplyAsync("", false, e.Build());
        }
        
        [Command("warn list")]
        [Summary("Lists all current warns of the user executing the command")]
        public async Task WarnList() => await WarnList(Context.User);
        [Command("warn list")]
        [Summary("Lists all current warns of a user")]
        public async Task WarnList([Summary("The warned user")] IUser user)
        {
            if (Data.users.Data is null) Data.users.Data = new();

            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            if (usr.warns is null) usr.warns = new();

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + " has been warned " + Code(usr.warns.Length.ToString()) + " time",
                Timestamp = DateTime.Now,
                Title = user.Username + "#" + user.Discriminator + "'s warns",
            };

            if (usr.warns.Length != 1) e.Description += "s";

            foreach (Warn w in usr.warns) e.AddField(w.reason, Italics("Moderator:") + " " + "<@" + w.moderator + ">" +
                "\n" + Italics("Time:") + " " + Code(w.time.ToString()) +
                "\n" + Italics("Hash:") + " " + Code(w.hash));

            await ReplyAsync("", false, e.Build());
        }

        [Command("unmute")]
        [RequireUserPermission(GuildPermission.ManageGuild & GuildPermission.ManageRoles)]
        [Summary("Unmutes a user")]
        public async Task Unmute([Summary("The user to unmute")] IUser user, [Remainder][Summary("The reason to unmute the user for")] string reason = "No Reason Specified")
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);
            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }
            Mute m = usr.currentMute;
            if (m == null)
            {
                m = new();
                usr.currentMute = m;
            }
            if (!m.IsMuted)
            {
                LogModule.LogMessage(LogSeverity.Error, "User is not currently muted. Use " + Code("n;mute") + " to mute them");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = user.Mention + " has been successfully unmuted for reason: " + Code(reason),
                Timestamp = DateTime.Now,
                Title = "User Unmuted",
            };
            e.WithFooter("Use 'n;mute info' for info on the user's current mute status");

            Mute.Unmute unmute = new()
            {
                moderator = Context.User.Id,
                reason = reason,
                time = DateTime.Now,
            };

            m.unmute = unmute;

            await user.DMUserAsync("", false, UnmuteDMEmbed((IGuildUser)user, reason, Context.User).Build());

            await ReplyAsync("", false, e.Build());
        }
        // these two commands have practically the same code. remember to change both when editing code
        [Command("unmute ghost")]
        [RequireUserPermission(GuildPermission.ManageGuild & GuildPermission.ManageRoles)]
        [Summary("Unmutes a user (without DMing them)")]
        public async Task UnmuteGhost([Summary("The user to unmute")] IUser user, [Remainder][Summary("The reason to unmute the user for")] string reason = "No Reason Specified")
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);
            if (usr == null)
            {
                usr = new()
                {
                    currentMute = new(),
                    userID = user.Id,
                };
                Data.users.Data.Add(usr);
            }
            Mute m = usr.currentMute;
            if (!m.IsMuted)
            {
                LogModule.LogMessage(LogSeverity.Error, "User is not currently muted. Use " + Code("n;mute") + " to mute them");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = user.Mention + " has been successfully unmuted for reason: " + Code(reason),
                Timestamp = DateTime.Now,
                Title = "User Unmuted",
            };
            e.WithFooter("Use 'n;mute info' for info on the user's current mute status");

            Mute.Unmute unmute = new()
            {
                moderator = Context.User.Id,
                reason = reason,
                time = DateTime.Now,
            };

            m.unmute = unmute;

            await ReplyAsync("", false, e.Build());
        }

        // end commands

        public async Task<bool> MainKickSystem(SocketGuildUser user, string reason)
        {
            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = user.Mention + " has been kicked from " + Code(Context.Guild.Name) + " for reason: " + Code(reason),
                Timestamp = DateTime.Now,
                Title = "User Kicked from Server",
            };

            await user.KickAsync("Moderator: " + LogItem() + " | Reason: " + reason);

            await ReplyAsync("", false, e.Build());

            return false;
        }
        public static async Task<bool> MainMessageSystem(IUser user, EmbedAuthorBuilder author, string message)
        {
            EmbedBuilder e = new()
            {
                Author = author,
                Color = Colors.DefaultColor,
                Description = message,
                Timestamp = DateTime.Now,
            };

            if((await user.DMUserAsync("", false, e.Build())).Item2)
            {
                e.Author = null;
                e.WithTitle("DM sent to " + user.Username + "#" + user.Discriminator);
            }

            return false;
        }
        public async Task<(int, bool)> MainWarnClearSystem(IUser user)
        {
            if (Data.users.Data is null) Data.users.Data = new();

            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add();
            }

            int originalAmount = 0;

            if (usr.warns is not null) originalAmount = usr.warns.Length;

            if (originalAmount == 0)
            {
                LogModule.LogMessage(LogSeverity.Error, "User has no currently tracked warns, and as such cannot clear them");
                usr.warns = new();
                return (-1, true);
            }

            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = user.Mention + "has gone from " + Code(originalAmount.ToString()) + " to " + Code("0") + " warns",
                Timestamp = DateTime.Now,
                Title = "User's warns have been cleared",
            };
            e.WithFooter("Use 'n;warn delete' to delete a specific warn by it's hash");

            usr.warns = new();

            await ReplyAsync("", false, e.Build());

            return (originalAmount, false);
        }
        public async Task<(User, Warn, bool)> MainWarnDeleteSystem(IUser user, string hash)
        {
            if (Data.users.Data is null)
            {
                LogModule.LogMessage(LogSeverity.Error, "No user data is found");
                return (null, null, true);
            }

            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            if (usr.warns is null) usr.warns = new();

            Warn warn = usr.warns.FindOrDefault(x => x.hash == hash);

            if (warn == null)
            {
                LogModule.LogMessage(LogSeverity.Error, "No warn exists on this user with the specified hash");
                return (null, null, true);
            }

            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = "Deleted " + user.Mention + "'s warn." +
                    "\n" + Italics("More info on that warn is below.") +
                    "\n" +
                    "\n" + Italics("Reason:") + " " + Code(warn.reason) +
                    "\n" + Italics("Moderator:") + " <@" + warn.moderator + ">" +
                    "\n" + Italics("Time:") + " " + Code(warn.time.ToString()) +
                    "\n" + Italics("Hash:") + " " + Code(warn.hash),
                Timestamp = DateTime.Now,
                Title = "Removed User Warn",
            };
            e.WithFooter("Use 'n;warn list' to see the total tracked warns of this user!");

            usr.warns.Remove(warn);

            await ReplyAsync("", false, e.Build());

            return (usr, warn, false);
        }
        public async Task<(EmbedBuilder, int, bool)> MainWarnSystem(IUser user, string reason)
        {
            Warn warn = new()
            {
                hash = Hashes.MD5(user.Mention + reason + DateTime.Now),
                moderator = Context.User.Id,
                reason = reason,
                time = DateTime.Now,
            };

            int amount;

            EmbedBuilder e = new()
            {
                Color = Color.Red,
                Description = user.Mention + " has been warned for reason: " + Code(reason),
                Timestamp = DateTime.Now,
                Title = "Warned User"
            };
            e.WithFooter("Warn Hash: " + warn.hash);
            if (reason.EndsWith(" [GHOST]")) e.Title = "Ghost " + e.Title;

            int index = Data.users.Data.FindIndex(x => x.userID == user.Id);
            if (index == -1)
            {
                Data.users.Data.Add(new()
                {
                    userID = user.Id,
                    warns = new(warn),
                });
                amount = 1;
            }
            else
            {
                Data.users.Data[index].warns.Add(warn);
                amount = Data.users.Data[index].warns.Length;
            }

            e.Description += "\n" + Italics("This is their " + Code(Nerd_STF.Misc.PlaceMaker(amount)) + " warning");

            await ReplyAsync("", false, e.Build());

            return (e, amount, false);
        }

        public static EmbedBuilder UnmuteDMEmbed(IGuildUser user, string reason, IUser moderator)
        {
            EmbedBuilder e = new()
            {
                Color = Color.Green,
                Description = moderator + " has unmuted you for reason: " + Code(reason),
                Timestamp = DateTime.Now,
                Title = "You have been unmuted in " + user.Guild.Name,
            };
            e.WithFooter("Use 'n;mute info' for info on your current mute status");

            return e;
        }
    }
}