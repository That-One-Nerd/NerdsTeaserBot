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
    [Group("level")]
    [Name("Level")]
    [Summary("Commands about leveling up and levelup message formatting")]
    public class LevelModule : ModuleBase<SocketCommandContext>
    {
        public static List<User> LevelLeaderboard
        {
            get
            {
                System.Collections.Generic.List<User> systemL = new(Data.users.Data.FindAll(x => x.level != null).FindAll(x => x.level.TotalXP > 0).ToArray());
                systemL.Sort((x, y) => x.level.TotalXP - y.level.TotalXP);
                systemL.Reverse();

                return new(systemL);
            }
        }

        [Command("give")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Gives a certain amount of levels or xp to a user")]
        public async Task Give([Summary("The user to give levels/xp to")] SocketGuildUser user, [Summary("The amount of levels/xp to give")] int value, [Summary("The type to give (levels/xp)")] ModifyType type = ModifyType.Levels)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            Level lvl = usr.level;

            if (lvl == null)
            {
                lvl = new();
                usr.level = lvl;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + " has has their " + Code(type.ToString()) + " added to, from ",
                Timestamp = DateTime.Now,
                Title = type + " Added",
            };

            switch (type)
            {
                case ModifyType.Level or ModifyType.Levels:
                    e.Description += Code("Level " + lvl.level) + " to " + Code("Level " + (lvl.level + value));
                    lvl.level += value;
                    break;

                case ModifyType.XP:
                    e.Description += Code(lvl.xp + " XP") + " to " + Code(lvl.xp + value + " XP");
                    lvl.xp += value;

                    while (lvl.xp >= lvl.MaxXP)
                    {
                        lvl.xp -= lvl.MaxXP;
                        lvl.level++;
                    }
                    break;
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("leaderboard")]
        [Summary("Shows the level stats of anyone who has talked in this server")]
        public async Task Leaderboard()
        {
            List<User> users = LevelLeaderboard;

            int countedM = 0, totalM = 0, totalX = 0;
            string leaderboard = "";
            for (int i = 0; i < users.Length; i++)
            {
                Level lvl = users[i].level;

                countedM += lvl.countedMsgs;
                totalM += lvl.msgs;
                totalX += lvl.TotalXP;

                leaderboard += Code(Nerd_STF.Misc.PlaceMaker(i + 1)) + ": <@" + users[i].userID + ">: " + Code("Level " + lvl.level) + " and " + Code(lvl.xp + "/" + lvl.MaxXP + " XP") + " *(" + lvl.msgs + " Message";
                if (lvl.msgs != 1) leaderboard += "s";
                leaderboard += ", " + lvl.countedMsgs + " Counted)*\n";
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "*" + totalM + " Total Message",
                Timestamp = DateTime.Now,
                Title = "Level Leaderboard for " + Context.Guild.Name,
            };

            if (totalM != 1) e.Description += "s";
            e.Description += " (" + countedM + " Counted)*\n" + Italics(totalX + " Total XP") + "\n\n" + leaderboard;

            await ReplyAsync("", false, e.Build());
        }

        [Command("mention get")]
        [Summary("Shows if the bot will mention a user when they level up")]
        public async Task MentionGet()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The bot will mention users when they level up: " + Code(Data.misc.Data.levelMention.ToString()),
                Timestamp = DateTime.Now,
                Title = "Levelup Mention",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("mention reset")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets the ability to mention when leveling up back to the default value")]
        public async Task MentionReset()
        {
            Data.misc.Data.levelMention = false;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the ability to mention a user when leveling up to: " + Code("False"),
                Timestamp = DateTime.Now,
                Title = "Reset Levelup Mention",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("mention set")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets the ability to mention when leveling up")]
        public async Task MentionSet([Summary("The true/false statement of the ability")] bool mention)
        {
            bool old = Data.misc.Data.levelMention;
            Data.misc.Data.levelMention = mention;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the ability to mention a user when leveling up to: " + Code(mention.ToString()),
                Timestamp = DateTime.Now,
                Title = "Set Levelup Mention",
            };
            e.Description += "\n(Previous: " + Code(old.ToString() + ")");

            await ReplyAsync("", false, e.Build());
        }

        [Command("rank")]
        [Summary("Shows level info on the user executing the command")]
        public async Task Rank() => await Rank(Context.User);

        [Command("rank")]
        [Summary("Shows level info on a specific user")]
        public async Task Rank(SocketUser user)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            Level lvl = usr.level;

            if (lvl == null)
            {
                lvl = new();
                usr.level = lvl;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + " " + Code(lvl.TotalXP > 0 ? "IS" : "is NOT") + " on the Leaderboard\n\n" +
                              "Level: " + Code(lvl.level.ToString()) + "\n" +
                              "XP: " + Code(lvl.xp.ToString()) + "\n" +
                              "(Total XP): " + Code(lvl.TotalXP.ToString()) + "\n" +
                              "XP to Level Up: " + Code((lvl.MaxXP - lvl.xp).ToString() + " (" + lvl.MaxXP + " Total)"),
                Timestamp = DateTime.Now,
                Title = user.Username + "'s Level Stats",
            };

            if (lvl.TotalXP > 0) e.Description += "\n" + "Leaderboard Rank: " + Code(Nerd_STF.Misc.PlaceMaker(LevelLeaderboard.FindIndex(x => x.userID == user.Id) + 1));

            await Context.Channel.SendFileAsync(Data.appPath + "/data/constant/level-imgs/" + Math.Clamp(lvl.level, -1, 101) + ".png", "", false, e.Build());
        }

        [Command("remove")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Removes a certain amount of levels or xp from a user")]
        public async Task Remove([Summary("The user to remove levels/xp from")] SocketGuildUser user, [Summary("The amount of levels/xp to remove")] int value, [Summary("The type to remove (levels/xp)")] ModifyType type = ModifyType.Levels)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            Level lvl = usr.level;

            if (lvl == null)
            {
                lvl = new();
                usr.level = lvl;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + " has has their " + Code(type.ToString()) + " removed, from ",
                Timestamp = DateTime.Now,
                Title = type + " Removed",
            };

            switch (type)
            {
                case ModifyType.Level or ModifyType.Levels:
                    if (lvl.level < value)
                    {
                        LogModule.LogMessage(LogSeverity.Error, "User does not have enough levels to remove");
                        return;
                    }

                    double percent = lvl.xp / lvl.MaxXP;

                    e.Description += Code("Level " + lvl.level) + " to " + Code("Level " + (lvl.level - value));
                    lvl.level -= value;
                    lvl.xp = (int)Math.Round(percent * lvl.MaxXP);
                    break;

                case ModifyType.XP:
                    if (lvl.TotalXP < value)
                    {
                        LogModule.LogMessage(LogSeverity.Error, "User does not have enough xp to remove");
                        return;
                    }

                    e.Description += Code(lvl.xp + " XP") + " to " + Code(lvl.xp - value + " XP");

                    while (value >= lvl.MaxXP)
                    {
                        value -= lvl.MaxXP;
                        lvl.xp--;
                    }

                    lvl.xp -= value;
                    break;
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("reset")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Resets all levels or xp for a user")]
        public async Task Reset([Summary("The user to reset levels/xp of")] SocketGuildUser user, [Summary("The type to reset (levels/xp)")] ModifyType type = ModifyType.Levels)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            Level lvl = usr.level;

            if (lvl == null)
            {
                lvl = new();
                usr.level = lvl;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + " has has their " + Code(type.ToString()) + " reset to " + Code("0"),
                Timestamp = DateTime.Now,
                Title = type + " Reset",
            };

            switch (type)
            {
                case ModifyType.Level or ModifyType.Levels:
                    double percent = lvl.xp / lvl.MaxXP;

                    lvl.level = 0;
                    lvl.xp = (int)Math.Round(percent * 200);
                    break;

                case ModifyType.XP:
                    lvl.xp = 0;
                    break;
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("set")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets a user's level or xp")]
        public async Task Set([Summary("The user to set levels/xp of")] SocketGuildUser user, [Summary("The amount of levels/xp to set")] int value, [Summary("The type to set (levels/xp)")] ModifyType type = ModifyType.Levels)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == user.Id);

            if (usr == null)
            {
                usr = new() { userID = user.Id };
                Data.users.Data.Add(usr);
            }

            Level lvl = usr.level;

            if (lvl == null)
            {
                lvl = new();
                usr.level = lvl;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = user.Mention + " has has their " + Code(type.ToString()) + " set from ",
                Timestamp = DateTime.Now,
                Title = type + " Changed",
            };

            switch (type)
            {
                case ModifyType.Level or ModifyType.Levels:
                    e.Description += Code("Level " + lvl.level) + " to " + Code("Level " + value);

                    double xp = lvl.xp;
                    if (value < lvl.level) xp = lvl.xp / lvl.MaxXP;

                    lvl.level = value;

                    if (value < lvl.level) lvl.xp = (int)Math.Round(xp * lvl.MaxXP);
                    break;

                case ModifyType.XP:
                    e.Description += Code(lvl.xp + " XP") + " to " + Code(value + " XP");
                    lvl.xp += value;

                    while (lvl.xp >= lvl.MaxXP)
                    {
                        lvl.xp -= lvl.MaxXP;
                        lvl.level++;
                    }
                    break;
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("speed get")]
        [Summary("Shows the current xp cooldown")]
        public async Task SpeedGet()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "The current cooldown between messages is `" + Data.misc.Data.levelCooldown + " Second",
                Timestamp = DateTime.Now,
                Title = "XP Speed",
            };
            if (Data.misc.Data.levelCooldown != 1) e.Description += "s";
            e.Description += "`";

            await ReplyAsync("", false, e.Build());
        }

        [Command("speed reset")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets the current xp cooldown to 60 seconds")]
        public async Task SpeedReset()
        {
            Data.misc.Data.levelCooldown = 60;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set the current xp cooldown to: " + Code("60 Seconds"),
                Timestamp = DateTime.Now,
                Title = "Reset xp cooldown",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("speed set")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets the current xp cooldown")]
        public async Task SpeedSet([Summary("The time in seconds of the xp cooldown")] int seconds)
        {
            int old = Data.misc.Data.levelCooldown;
            Data.misc.Data.levelCooldown = seconds;

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "Successfully set changelog channel to: `" + seconds + " Second",
                Timestamp = DateTime.Now,
                Title = "Set changelog channel",
            };
            if (seconds != 1) e.Description += "s";
            e.Description += "`\n(Previous: `" + old + " Second";

            if (old != 1) e.Description += "s";
            e.Description += "`)";

            await ReplyAsync("", false, e.Build());
        }
        
        // end commands
        
        public static async Task LevelHandler(SocketUserMessage msg)
        {
            User usr = Data.users.Data.FindOrDefault(x => x.userID == msg.Author.Id);
            Level level = null;
            if (usr == null)
            {
                level = new();

                usr = new()
                {
                    currentMute = new(),
                    level = level,
                    tickets = 0,
                    userID = msg.Author.Id,
                    warns = new(),
                };

                Data.users.Data.Add(usr);
            }
            else
            {
                if (usr.level == null) usr.level = new();
                level = usr.level;
            }

            level.msgs++;
            if (level.lastCountedMsg.AddSeconds(Data.misc.Data.levelCooldown) < msg.Timestamp.DateTime)
            {
                level.countedMsgs++;
                level.lastCountedMsg = msg.Timestamp.DateTime;
                level.xp += new Random().Next(15, 26);

                if (level.xp >= level.MaxXP)
                {
                    level.xp -= level.MaxXP;
                    usr.level.level++;

                    EmbedBuilder e = new()
                    {
                        Color = Colors.DefaultColor,
                        Description = msg.Author.Mention + ", you have leveled up to " + Code("Level " + usr.level.level) + ". You need " + Code(level.MaxXP + " XP") + " to get to the next level",
                        Timestamp = DateTime.Now,
                        Title = "Leveled Up!",
                    };
                    string send = "";
                    if (Data.misc.Data.levelMention) send = msg.Author.Mention;

                    await msg.Channel.SendFileAsync(Data.appPath + "/data/constant/level-imgs/" + Math.Clamp(level.level, -1, 101) + ".png", send, false, e.Build());
                }
            };
        }

        // end of methods

        public enum ModifyType
        {
            Level,
            Levels,
            XP,
        }
    }
}