using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Extensions;
using Nerd_STF.Lists;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Group("role")]
    [Name("Role")]
    [Summary("Commands about role registering, unregistering, and creating/listing/removing")]
    public class RoleModule : ModuleBase<SocketCommandContext>
    {
        [Command("add")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Adds a role to the role list")]
        public async Task RoleAdd([Summary("The role to add")] SocketRole role)
        {
            List<SocketRole> roles = new List<SocketRole>(Context.Guild.Roles).FindAll(x => Data.misc.Data.roles.Contains(x.Id));
            if (roles.Any(x => x.Name.Trim().ToLower() == role.Name.Trim().ToLower()))
            {
                LogModule.LogMessage(LogSeverity.Error, "A role already is added with that name. Please change the name slightly");
                return;
            }

            /*if (Context.Guild.CurrentUser.Roles < role.Position)
            {
            
            }*/

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "A user can now use " + Code("n;role register " + role.Name.ToLower()) + " to be given the role " + role.Mention,
                Timestamp = DateTime.Now,
                Title = "Role added to register roles",
            };

            Data.misc.Data.roles.Add(role.Id);

            await ReplyAsync("", false, e.Build());
        }

        [Command("list")]
        [Summary("Lists all roles in the role list")]
        public async Task RoleList()
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = Data.misc.Data.roles.Length + " Roles in Role List",
            };

            List<IRole> roles = new List<IRole>(Context.Guild.Roles).FindAll(x => Data.misc.Data.roles.Contains(x.Id));

            foreach (IRole role in roles) e.Description += role.Mention + "\n";

            await ReplyAsync("", false, e.Build());
        }

        [Command("register")]
        [Summary("Gives yourself a role with the given name (DO NOT MENTION A ROLE)")]
        public async Task RoleRegister([Remainder][Summary("The role to add. Use '--all roles--' to give all roles (DO NOT MENTION A ROLE)")] string roleName)
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
            };
            List<SocketRole> registeredRoles = new List<SocketRole>(Context.Guild.Roles).FindAll(x => Data.misc.Data.roles.Contains(x.Id));
            string name = roleName.Trim().ToLower();

            SocketGuildUser gUser = (SocketGuildUser)Context.User;

            if (name == "--all roles--" || name == "-- all roles --")
            {
                e.Description = Context.User.Mention + " has been given the following roles:\n";
                foreach (SocketRole r in registeredRoles) e.Description += r.Mention + ", ";
                e.Description = e.Description.Remove(e.Description.Length - 2);

                e.Title = registeredRoles.Length + " Roles given to " + Context.User.Username + "#" + Context.User.Discriminator;

                await gUser.AddRolesAsync(registeredRoles);
            }
            else
            {
                SocketRole role = registeredRoles.FindOrDefault(x => x.Name.Trim().ToLower() == name);

                if (role == default)
                {
                    LogModule.LogMessage(LogSeverity.Error, "No register role found with that name (" + Code(name) + ")");
                    return;
                }

                if (new List<SocketRole>(gUser.Roles).Contains(x => x.Id == role.Id))
                {
                    LogModule.LogMessage(LogSeverity.Error, "User has already registered that role");
                    return;
                }

                e.Description = Context.User.Mention + " has been given the role " + role.Mention;
                e.Title = "Role registered to user";

                await gUser.AddRoleAsync(role);
            }

            await ReplyAsync("", false, e.Build());
        }

        [Command("remove")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Removes a role from the role list")]
        public async Task RoleRemove([Summary("The role to remove")] IRole role)
        {
            if (!Data.misc.Data.roles.Contains(role.Id))
            {
                LogModule.LogMessage(LogSeverity.Error, "That role is not found in the list of register roles");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "A user can no longer now use " + Code("n;role register " + role.Name.ToLower()) + " to be given the role " + role.Mention,
                Timestamp = DateTime.Now,
                Title = "Role removed from register roles",
            };

            Data.misc.Data.roles.Remove(role.Id);

            await ReplyAsync("", false, e.Build());
        }

        [Command("unregister")]
        [Summary("Removes a role with the given name (DO NOT MENTION A ROLE)")]
        public async Task RoleUnregister([Remainder][Summary("The role to remove. Use '--all roles--' to remove all roles (DO NOT MENTION A ROLE)")] string roleName)
        {
            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Timestamp = DateTime.Now,
            };
            List<SocketRole> registeredRoles = new List<SocketRole>(Context.Guild.Roles).FindAll(x => Data.misc.Data.roles.Contains(x.Id));
            string name = roleName.Trim().ToLower();

            SocketGuildUser gUser = (SocketGuildUser)Context.User;

            if (name == "--all roles--" || name == "-- all roles --")
            {
                e.Description = Context.User.Mention + " has removed the following roles:\n";
                foreach (SocketRole r in registeredRoles) e.Description += r.Mention + ", ";
                e.Description = e.Description.Remove(e.Description.Length - 2);

                e.Title = registeredRoles.Length + " Roles removed from " + Context.User.Username + "#" + Context.User.Discriminator;

                await gUser.RemoveRolesAsync(registeredRoles);
            }
            else
            {
                SocketRole role = registeredRoles.FindOrDefault(x => x.Name.Trim().ToLower() == name);

                if (role == default)
                {
                    LogModule.LogMessage(LogSeverity.Error, "No register role (to remove) found with that name " + Code("(" + name + ")"));
                    return;
                }

                if (!new List<SocketRole>(gUser.Roles).Contains(x => x.Id == role.Id))
                {
                    LogModule.LogMessage(LogSeverity.Error, "User has not registered that role");
                    return;
                }

                e.Description = Context.User.Mention + " has lost the role " + role.Mention;
                e.Title = "Role unregistered to user";

                await gUser.RemoveRoleAsync(role);
            }

            await ReplyAsync("", false, e.Build());
        }
    }
}