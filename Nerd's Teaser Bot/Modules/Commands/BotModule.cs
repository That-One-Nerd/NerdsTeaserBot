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
    [Name("Bot")]
    [Summary("Commands about the bot and it's data")]
    public class BotModule : ModuleBase<SocketCommandContext>
    {
        [Command("commandinfo")]
        [Summary("Info about a specific command")]
        public async Task CommandInfo([Summary("The command to show info about")] [Remainder] string command)
        {
            CommandService comms = Internals.commands;

            List<CommandInfo> cmds = new List<CommandInfo>(comms.Commands).FindAll(x => !new List<Attribute>(x.Attributes).Contains(y => y.GetType() == typeof(HiddenFromListAttribute)));
            List<CommandInfo> cmdS = cmds.FindAll(x => x.Name.ToLower() == command.ToLower());

            bool ded = false;
            if (cmdS is null) ded = true;
            else if (cmdS.Length == 0) ded = true;

            if (ded)
            {
                LogModule.LogMessage(LogSeverity.Error, "There is no command found with that name.");
                return;
            }

            foreach (CommandInfo cmd in cmdS)
            {
                EmbedBuilder e = new()
                {
                    Color = Colors.DefaultColor,
                    Timestamp = DateTime.Now,
                    Title = "Info about n;" + cmd.Name,
                };

                string summary = "No Summary Provided";
                if (cmd.Summary != "" && cmd.Summary != null) summary = cmd.Summary;

                e.AddField("Summary", Code(summary), true);
                e.AddField("Module", Code(cmd.Module.Name), true);

                List<string> aliases = new List<string>(cmd.Aliases).FindAll(x => x.ToLower() != cmd.Name.ToLower());
                bool pass = false;
                if (aliases is null) pass = true;
                else if (aliases.Length == 0) pass = true;

                if (!pass)
                {
                    string alias = "";
                    foreach (string s in aliases) alias += Code(s) + ", ";
                    e.AddField("Aliases", alias.Remove(alias.Length - 2), true);
                }

                List<ParameterInfo> param = new List<ParameterInfo>(cmd.Parameters).FindAll(x => !new List<Attribute>(x.Attributes).Contains(y => y.GetType() == typeof(HiddenFromListAttribute)));

                e.AddField("Parameter Count:", Code(param.Length.ToString()));

                foreach (ParameterInfo par in param)
                {
                    string add = "";

                    string summaryP = "No Summary Provided";
                    if (par.Summary != "" && par.Summary != null) summaryP = par.Summary;

                    add += "_Summary: _" + Code(summaryP) + "\n";
                    add += "_Optional: _" + "`" + par.IsOptional;
                    if (par.IsOptional)
                    {
                        add += " (Default: ";
                        if (par.Type == typeof(string)) add += '"' + par.DefaultValue.ToString() + '"';
                        else add += par.DefaultValue;
                        add += ")";
                    }
                    add += "`\n";

                    add += "_Parameter Type: _" + Code(par.Type.Name);

                    e.AddField("Parameter: " + par.Name + "", add);
                }

                await ReplyAsync("", false, e.Build());
            }            
        }

        [Command("commands")]
        [Summary("List of command modules, or commands of a module")]
        public async Task Commands([Summary("The module name to show commands for. Leave empty to show all modules.")] string module = "")
        {
            CommandService comms = Internals.commands;
            List<ModuleInfo> modules = new List<ModuleInfo>(comms.Modules).FindAll(x => !new List<Attribute>(x.Attributes).Contains(y => y.GetType() == typeof(HiddenFromListAttribute)));
            List<CommandInfo> commands = new List<CommandInfo>(comms.Commands).FindAll(x => !new List<Attribute>(x.Attributes).Contains(y => y.GetType() == typeof(HiddenFromListAttribute)));

            if (commands.Contains(x => x.Name.ToLower() == module.ToLower())) LogModule.LogMessage(LogSeverity.Warning, "A command has been found with the name " + Bold(Code(module.ToLower())) + ". Did you mean to use the command " + Code("n;commandinfo " + module.ToLower()) + "?");

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = "",
            };

            if (module == "") ShowModules();
            else
            {
                ModuleInfo info = modules.FindOrDefault(x => x.Name.ToLower() == module.ToLower());
                if (info == null)
                {
                    LogModule.LogMessage(LogSeverity.Error, "No module is found with that name.");
                    return;
                }
                else ShowCommands(info);
            }

            void ShowCommands(ModuleInfo module)
            {
                List<CommandInfo> cmds = new List<CommandInfo>(module.Commands).FindAll(x => !new List<Attribute>(x.Attributes).Contains(y => y.GetType() == typeof(HiddenFromListAttribute)));

                e.Title = "Showing " + cmds.Length + " Command";
                if (modules.Length != 1) e.Title += "s";

                bool pass = false;
                if (cmds is null) pass = true;
                else if (cmds.Length == 0) pass = true;

                if (pass) e.Description = Italics("There are no commands in this module currently avaliable.");
                else
                {
                    foreach (CommandInfo cmd in cmds)
                    {
                        string summary = "No Summary Provided";
                        if (cmd.Summary != "" && cmd.Summary != null) summary = cmd.Summary;
                        e.Description += Bold(Code(cmd.Name)) + " - " + Bold(cmd.Summary) + "\n";
                    }

                    e.Description += "\n*" + cmds.Length + " Total Command";
                    if (cmds.Length != 1) e.Description += "s";
                    e.Description += "*";
                }
            }
            void ShowModules()
            {
                e.Title = "Showing " + modules.Length + " Command Module";
                if (modules.Length != 1) e.Title += "s";

                bool pass = false;
                if (modules is null) pass = true;
                else if (modules.Length == 0) pass = true;

                if (pass) e.Description = Italics("There are no command modules currently avaliable.");
                else
                {
                    int commandsL = 0;

                    foreach (ModuleInfo module in modules)
                    {
                        int length = new List<CommandInfo>(module.Commands).Length;

                        string summary = "No Summary Provided";
                        if (module.Summary != "" && module.Summary != null) summary = module.Summary;
                        e.Description += "**" + Code(module.Name) + " *(" + length + " cmd";
                        if (length != 1) e.Description += "s)";
                        e.Description += "*** - " + Bold(summary) + "\n";

                        commandsL += length;
                    }

                    e.Description += "\n*" + commandsL + " Total Command";
                    if (commandsL != 1) e.Description += "s";
                    e.Description += "*";
                }
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("credits")]
        [Summary("The list of people who deserve mentions")]
        public async Task Credits()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = Italics("There are currently " + Code(Static.Credits.Length.ToString()) + " honerable mentions."),
                Timestamp = DateTime.Now,
                Title = "Honerable Mentions for " + Username,
            };

            foreach (Credit c in Static.Credits)
            {
                SocketUser usr = Context.Client.GetUser(c.id);
                e.AddField(usr.Username + "#" + usr.Discriminator, Code(c.reason));
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("help")]
        [Summary("Basic info about the bot")]
        public async Task Help()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
                Title = "Basic Info on " + Username
            };

            e.AddField("What am I?", "I am a bot used to maintain the backend of a server, Nerd's Teasers.", true);
            e.AddField("What can I do?", "I can control automod, tags, moderation tools, teaser info, and more.", true);
            e.AddField("What is my prefix?", "My prefix is " + Code("n;") + ". Use " + Code("n;info") + " for more advanced info, or " + Code("n;commands") + " for a list of command modules.", true);

            await ReplyAsync("", false, e.Build());
        }

        [Command("info")]
        [Summary("In-depth info about the bot")]
        public async Task Info()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 512),
                Timestamp = DateTime.Now,
                Title = "In-Depth info about " + Username,
            };

            SocketSelfUser self = Internals.client.CurrentUser;
            e.AddField("Full Name", Code(FullName), true);
            e.AddField("Client ID", Code(ID.ToString()), true);
            e.AddField("Mention", "<@" + ID + ">", true);
            e.AddField("Created At", Code(self.CreatedAt.DateTime.ToString()), true);
            e.AddField("Program Language", Code("C-Sharp (C#)"), true);
            e.AddField("Program APIs", Code(".NET 5.0") + "\n" + Code("Discord.NET (Experimental) v2.3.8 (API v6)"), true);

            string version = "Cannot detect Bot Version (No Changelog Provided)";
            if (new List<SocketGuildChannel>(Context.Guild.Channels).Contains(x => x.Id == Data.misc.Data.changelogChannel))
            {
                IMessage msg = new List<IMessage>(await Internals.client.GetGuild(Context.Guild.Id).GetTextChannel(Data.misc.Data.changelogChannel).GetMessagesAsync(1).FlattenAsync())[0];
                int start = -1, end = msg.Content.Length;

                for (int i = 0; i < msg.Content.Length; i++)
                {
                    if (msg.Content[i] == 'v') start = i;
                    else if (start != -1 && (msg.Content[i] == ' ' || msg.Content[i] == '*'))
                    {
                        end = i;
                        break;
                    }
                }
                if (start == -1) start = 0;
                version = msg.Content[start..end];
            }
            e.AddField("Version: ", Code(version), true);

            List<SocketRole> roles = new(Context.Guild.GetUser(self.Id).Roles);
            roles.Remove(Context.Guild.EveryoneRole);

            string roleS = "";
            foreach (SocketRole role in roles) roleS += role.Mention + ", ";

            e.AddField("Assigned Roles: ", roleS.Remove(roleS.Length - 2));

            await ReplyAsync("", false, e.Build());
        }

        [Command("ping")]
        [Summary("Pings the bot, and returns helpful info about that ping")]
        public async Task Ping()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = Italics("You have just called for the ping time of " + Code(Username)),
                Timestamp = DateTime.Now,
                Title = ":ping_pong: Pong",
            };

            e.AddField("Estimated Server Latency", Code(Context.Client.Latency + " Milliseconds") +
                "\n" + Italics("Should be around " + Code("100 - 300")), true);
            e.AddField("Round-Trip Ping Time", Code((DateTime.UtcNow - Context.Message.CreatedAt.UtcDateTime).TotalMilliseconds.ToString("0") + " Milliseconds") +
                "\n" + Italics("Should be around " + Code("2000 - 4000")), true);
            e.AddField("Uptime", "This bot has been online since " + Code(Internals.upTime.Humanize(true, true)) +
                "\n" + Italics("(" + (DateTime.Now - Internals.upTime).Humanize(ignoreMilliseconds: true)) + ")", true);

            await ReplyAsync("", false, e.Build());
        }

        [Command("serverlist")]
        [RequireOwner]
        [Summary("Shows a list of all servers the bot is currently in")]
        public async Task Serverlist()
        {
            List<SocketGuild> guilds = new(Context.Client.Guilds);

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = "This Bot is in " + guilds.Length + " Servers",
            };

            foreach (SocketGuild guild in guilds)
            {
                if (!guild.HasAllMembers) await guild.DownloadUsersAsync();
                e.Description += Bold(Code(guild.Name)) + Italics(Code("(" + guild.Id + ")")) + " - Owned by " + guild.Owner.Mention + " and has " + Code(guild.MemberCount + " Member" + (guild.MemberCount == 1 ? "" : "s")) + "\n";
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("test")]
        [Summary("Does whatever is currently being tested")]
        public async Task Test()
        {
            await ReplyAsync("eat ass, chump");
        }

        // end commands

        public static async Task ServerJoinedDMHandler(SocketGuild guild)
        {
            if (!guild.HasAllMembers) await guild.DownloadUsersAsync();

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "<@" + ID + "> has joined " + Bold(Code(guild.Name)) + ". More info is below:\n\n" +
                              "Member Count: " + Code(guild.MemberCount + " Member" + (guild.MemberCount == 1 ? "" : "s")) + "\n" +
                              "Owner: " + guild.Owner.Mention + " " + Bold("(" + guild.Owner.Username + "#" + guild.Owner.Discriminator + ")") + "\n" +
                              "Server ID: " + Code(guild.Id.ToString()),
                Timestamp = DateTime.Now,
                Title = "Joined New Server",
            };

            await (await Internals.client.GetApplicationInfoAsync()).Owner.SendMessageAsync("", false, e.Build());
        }
    }
}