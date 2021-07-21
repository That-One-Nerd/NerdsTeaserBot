using Discord;
using Discord.Commands;
using NerdsTeaserBot.Misc;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Discord.Format;

namespace NerdsTeaserBot.Modules.Commands
{
    [Name("Online")]
    [Summary("Commands that involve the internet")]
    public class OnlineModule : ModuleBase<SocketCommandContext>
    {
        [Command("meme")]
        [Summary("Returns a meme from a random meme subreddit")]
        public async Task Meme([Name("minimum upvotes")] [Summary("The minimum amount of upvotes")] float minUps = 1500)
        {
            if (Data.misc.Data.memeSubs.Length == 0)
            {
                LogModule.LogMessage(LogSeverity.Error, "No Meme Subreddits are given. Use " + Code("n;memesub add") + " to add a subreddit to the list");
                return;
            }

            await Reddit(Data.misc.Data.memeSubs[new Random().Next(0, Data.misc.Data.memeSubs.Length)], minUps);
        }

        [Command("memesub add")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Adds a new meme subreddit to the list, if it is avaliable")]
        public async Task MemesubAdd([Summary("The name of the subreddit")] string name)
        {
            if (name.StartsWith("r/")) name = name[2..];

            if (Data.misc.Data.memeSubs.Contains(name))
            {
                LogModule.LogMessage(LogSeverity.Error, "Subreddit already exists in list");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Color.LightGrey,
                Description = "Checking if the subreddit r/" + name + "exists.\n" +
                    Italics("The Reddit API is slow, and loading an object can take some time. Please be patient."),
                Timestamp = DateTime.Now,
                Title = "Checking Subreddit Authenticity",
            };

            IUserMessage msg = await ReplyAsync("", false, e.Build());

            string str = await new HttpClient().GetStringAsync("https://reddit.com/r/" + name + "/random.json?limit=1");
            JObject obj = HalfParse(str);
            if ((int)obj["dist"] < 1)
            {
                LogModule.LogMessage(LogSeverity.Error, "Subreddit does not exist, or is private", "", msg);
                return;
            }

            Data.misc.Data.memeSubs.Add(name);

            e = new()
            {
                Color = Color.Orange,
                Description = "The subreddit, " + Url("r/" + name, "https://reddit.com/r/" + name) + " has been added to the meme subreddit list.",
                Timestamp = DateTime.Now,
                Title = "Subreddit Added",
            };
            e.WithFooter("Use 'n;memesub list' for a list of all considered meme subreddits");

            await msg.ModifyAsync(x => x.Embed = e.Build());

            static JObject HalfParse(string str)
            {
                if (str.StartsWith("[")) return (JObject)JArray.Parse(str)[0]["data"];
                else return (JObject)JObject.Parse(str)["data"];
            }
        }

        [Command("memesub list")]
        [Summary("Lists all current meme subreddits")]
        public async Task MemesubList()
        {
            EmbedBuilder e = new()
            {
                Color = Color.Orange,
                Description = "",
                Timestamp = DateTime.Now,
                Title = "Showing " + Data.misc.Data.memeSubs.Length + " Accepted Meme Subreddits",
            };

            foreach (string s in Data.misc.Data.memeSubs) e.Description += Code("r/" + s) + "\n";

            await ReplyAsync("", false, e.Build());
        }

        [Command("memesub remove")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Removed a meme subreddit from the subreddit list, if it exists in the list")]
        public async Task MemesubRemove([Summary("The name of the subreddit")] string name)
        {
            if (name.StartsWith("r/")) name = name[2..];

            bool predicate(string x) => x.ToLower() == name.ToLower();
            if (!Data.misc.Data.memeSubs.Contains(predicate))
            {
                LogModule.LogMessage(LogSeverity.Error, "Subreddit doesn't exist in the list");
                return;
            }

            Data.misc.Data.memeSubs.Remove(predicate);

            EmbedBuilder e = new()
            {
                Color = Color.Orange,
                Description = "Subreddit " + Code("r/" + name) + " has been removed from the meme subreddit list.",
                Timestamp = DateTime.Now,
                Title = "Subreddit Removed"
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("reddit")]
        [Summary("Returns a reddit post from a subreddit. Defaults to r/all")]
        public async Task Reddit([Summary("The subreddit to pull the post from")] string subreddit = "all", [Name("minimum upvotes")] [Summary("The minimum amount of upvotes")] float minUps = 1500)
        {
            if (subreddit.StartsWith("r/")) subreddit = subreddit[2..];

            HttpClient client = new();
            JObject obj;

            EmbedBuilder e = new()
            {
                Color = Color.LightGrey,
                Description = "Searching r/" + subreddit + " for a post with at least " + minUps + " upvote",
                Timestamp = DateTime.Now,
                Title = "Post Loading..."
            };
            if (minUps != 1) e.Description += "s";
            e.Description += "\n" + Italics("The Reddit API is slow, and loading an object can take some time. Please be patient.");

            IUserMessage msg = await ReplyAsync("", false, e.Build());

            bool selected = false;
            string str = await client.GetStringAsync("https://reddit.com/r/" + subreddit + "/top.json?t=all");
            obj = HalfParse(str);
            if ((int)obj["dist"] < 1)
            {
                LogModule.LogMessage(LogSeverity.Error, "Subreddit does not exist, or is private", "", msg);
                return;
            }
            obj = (JObject)obj["children"][0]["data"];
            if ((int)obj["ups"] < minUps)
            {
                LogModule.LogMessage(LogSeverity.Warning, "Top post of all time has less upvotes than the minimum required. Defaulting to that post.");
                minUps = (int)obj["ups"];
                selected = true;
            }

            static JObject HalfParse(string str)
            {
                if (str.StartsWith("[")) return (JObject)JArray.Parse(str)[0]["data"];
                else return (JObject)JObject.Parse(str)["data"];
            }
            
            while (!selected)
            {
                str = await client.GetStringAsync("https://reddit.com/r/" + subreddit + "/random.json?limit=1");
                obj = (JObject)HalfParse(str)["children"][0]["data"];

                if ((int)obj["ups"] >= minUps) selected = true;
            }

            if (obj == null)
            {
                LogModule.LogMessage(LogSeverity.Critical, "An unknown internal error has occurred. This should never happen.", "", msg);
                return;
            }

            EmbedAuthorBuilder a = new()
            {
                Name = obj["author"].ToString(),
                Url = "https://reddit.com/u/" + obj["author"],
            };
            e = new()
            {
                Author = a,
                Color = Color.Orange,
                Title = obj["title"].ToString(),
                Url = "https://reddit.com" + obj["permalink"],
            };

            e.WithFooter(obj["ups"] + " Upvote");
            if ((int)obj["ups"] != 1) e.Footer.Text += "s";
            e.Footer.Text += " | " + obj["num_comments"] + " Comment";
            if ((int)obj["num_comments"] != 1) e.Footer.Text += "s";

            if (obj["selftext"].ToString() != "") e.WithDescription(obj["selftext"].ToString());
            if (obj["url"].ToString() != obj["permalink"].ToString())
            {
                string url = obj["url"].ToString();
                if (url.EndsWith(".gifv")) url = url.Remove(url.Length - 1);
                e.WithImageUrl(url);
            }

            await msg.ModifyAsync(x => x.Embed = e.Build());
        }
    }
}