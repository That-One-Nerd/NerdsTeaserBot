using Discord;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Commands;
using NerdsTeaserBot.Modules.Extensions;
using NerdsTeaserBot.Modules.Models;
using Nerd_STF.Lists;
using System;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules
{
    public class BackgroundModule
    {
        public static async Task MuteHandler()
        {
            while (true)
            {
                IRole role;
                foreach (SocketGuild g in Internals.client.Guilds)
                {
                    role = await GetOrCreateMuteRole(g);
                    await RunInGuild(g);
                }

                async Task RunInGuild(SocketGuild guild)
                {
                    if (guild == null) return;

                    if (!new List<SocketRole>(guild.Roles).Contains(x => x.Id == role.Id)) role = await GetOrCreateMuteRole(guild);

                    List<User> users = Data.users.Data.FindAll(x => x.currentMute != null).FindAll(x => x.currentMute.start != default);

                    foreach (SocketGuildUser user in guild.Users)
                    {
                        User userM = users.FindOrDefault(x => x.userID == user.Id);
                        if (userM == default) continue;

                        List<IRole> userRoles = new(user.Roles);
                        if (userM.currentMute.IsMuted) { if (!userRoles.Contains(role)) AddRoleAsync(role); }
                        else
                        {
                            if (userM.currentMute.unmute == null) await Unmute();
                            else if (userM.currentMute.unmute.moderator == default) await Unmute();

                            if (userRoles.Contains(role)) RemoveRoleAsync(role);

                            async Task Unmute()
                            {
                                userM.currentMute.unmute = new()
                                {
                                    moderator = Internals.client.CurrentUser.Id,
                                    reason = "Mute Timer Expired",
                                    time = DateTime.Now,
                                };

                                await user.DMUserAsync("", false, ModerationModule.UnmuteDMEmbed(user, "Mute Timer Expired", Internals.client.CurrentUser).Build());
                            }
                        }

                        void AddRoleAsync(IRole role) => user.AddRoleAsync(role);
                        void RemoveRoleAsync(IRole role) => user.RemoveRoleAsync(role);
                    }
                }

                static async Task<IRole> GetOrCreateMuteRole(SocketGuild guild)
                {
                    List<IRole> roles = new(guild.Roles);

                    IRole r = roles.FindOrDefault(x => x.Name.ToLower().Trim().Contains("mute"));

                    if (r == default)
                    {
                        r = await guild.CreateRoleAsync("Muted", null, Color.DarkRed, false, false, null);

                        OverwritePermissions o = new(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, speak: PermValue.Deny, stream: PermValue.Deny);
                        foreach (IGuildChannel c in guild.Channels) await c.AddPermissionOverwriteAsync(r, o);
                    }

                    return r;
                }

                await Task.Delay(2500);
            }
        }
        public static async Task StatusHandler()
        {
            (ActivityType, string)[] Statuses = new (ActivityType, string)[]
            {
                (ActivityType.Watching, " for n;help"),
                (ActivityType.Watching, " over Nerd's Teasers"),
                (ActivityType.Listening, " commands"),
                (ActivityType.Watching, " for DMs"),
            };

            while (true)
            {
                await Internals.client.SetGameAsync(Statuses[Data.consts.Data.statusNum].Item2, null, Statuses[Data.consts.Data.statusNum].Item1);
                await Task.Delay(30000);
                Data.consts.Data.statusNum++;
                if (Data.consts.Data.statusNum >= Statuses.Length) Data.consts.Data.statusNum = 0;
                Data.consts.Save();
            }
        }
    }
}
