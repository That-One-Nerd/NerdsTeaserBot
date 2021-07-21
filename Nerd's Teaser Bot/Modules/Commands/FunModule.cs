using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Models;
using Nerd_STF;
using Nerd_STF.Lists;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Discord.Format;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot.Modules.Commands
{
    [Name("Fun")]
    [Summary("Commands that can be used for fun")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        internal static JArray jokes = null;

        [Command("cate")]
        [Summary("Finds an image of a cate on the internet and relays it back")]
        public async Task Cate()
        {
            HttpClient client = new();

            JArray array = JArray.Parse(await client.GetStringAsync("https://api.thecatapi.com/v1/images/search?size=full"));

            JObject cate = (JObject)array[0];

            EmbedBuilder e = new()
            {
                Footer = new() { Text = "Cate found using 'https://thecatapi.com'" },
                Timestamp = DateTime.Now,
                Title = "Cate Detected",
                ImageUrl = cate["url"].ToString(),
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("doge")]
        [Summary("Finds an image of a doge on the internet and relays it back")]
        public async Task Doge()
        {
            HttpClient client = new();

            JArray array = JArray.Parse(await client.GetStringAsync("https://api.thedogapi.com/v1/images/search?size=full"));

            JObject doge = (JObject)array[0];

            EmbedBuilder e = new()
            {
                Footer = new() { Text = "Doge found using 'https://thedogapi.com'" },
                Timestamp = DateTime.Now,
                Title = "Doge Detected",
                ImageUrl = doge["url"].ToString(),
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("joke")]
        [Summary("Gives a random joke to you. Punchline and everything")]
        public async Task Joke()
        {
            if (jokes == null) jokes = JArray.Parse(await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/15Dkatz/official_joke_api/master/jokes/index.json"));

            JObject joke = (JObject)jokes[new Random().Next(0, jokes.Count)];

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = Italics(joke["setup"].ToString()) + "\n" + Spoiler(joke["punchline"].ToString()),
                Footer = new() { Text = "Joke found using 'https://github.com/15Dkatz/official_joke_api'" },
                Timestamp = DateTime.Now,
                Title = "Definitely Funny Joke Incoming",
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("pocketsand")]
        [Summary("Pocketsands a user")]
        public async Task Pocketsand([Summary("The user to pocketsand")] IUser user)
        {
            EmbedBuilder e = new()
            {
                Color = Color.Orange,
                Description = Context.User.Mention + " pocketsands " + user.Mention + "! Shi-Shi Sha!",
                ImageUrl = "https://cdn.discordapp.com/attachments/786036378613710928/851972369941528586/pocketsand.gif",
                Timestamp = DateTime.Now,
                Title = "Pocketsanded!",
            };

            await ReplyAsync("Hey " + user.Mention + ", you just got...", false, e.Build());
        }

        [Command("poll")]
        [Summary("Creates a poll with the specified name and options")]
        public async Task Poll([Remainder][Summary("The options for the poll. Quote each option with ''. The first option is the title, the next are selections")] string options)
        {
            string[] letterEmotes = new[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯", "🇰", "🇱", "🇲", "🇳", "🇴", "🇵", "🇶", "🇷", "🇸", "🇹", "🇺", "🇻", "🇼", "🇽", "🇾", "🇿" };
            const string letters = "abcdefghijklmnopqrstuvwxyz";

            if (!options.StartsWith("'") || !options.EndsWith("'"))
            {
                LogModule.LogMessage(LogSeverity.Error, "Options must start and end with a '");
                return;
            }

            string[] splits = options.Trim()[1..(options.Length - 1)].Split("' '");

            if (splits.Length == 1)
            {
                await Poll("'" + splits[0] + "' 'Yes' 'No'");
                return;
            }

            if (splits.Length > 21)
            {
                LogModule.LogMessage(LogSeverity.Error, "There cannot be more than 21 options (including the title)");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = ":bar_chart: " + splits[0],
            };

            for (int i = 1; i < splits.Length; i++) e.Description += ":regional_indicator_" + letters[i - 1] + ": - " + Code(splits[i]) + "\n";

            IUserMessage msg = await ReplyAsync("", false, e.Build());

            for (int i = 1; i < splits.Length; i++) await msg.AddReactionAsync(new Emoji(letterEmotes[i - 1]));
        }

        [Command("pollbutton")]
        [Summary("Creates a poll with the specified name and options, and uses buttons to track interactions [NOTE: This is not perfectly stable]")]
        public async Task Pollbutton([Remainder][Summary("The options for the poll. Quote each option with ''. The first option is the title, the next are selections")] string options)
        {
            string[] letterEmotes = new[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯", "🇰", "🇱", "🇲", "🇳", "🇴", "🇵", "🇶", "🇷", "🇸", "🇹", "🇺", "🇻", "🇼", "🇽", "🇾", "🇿" };
            const string letters = "abcdefghijklmnopqrstuvwxyz";

            if (!options.StartsWith("'") || !options.EndsWith("'"))
            {
                LogModule.LogMessage(LogSeverity.Error, "Options must start and end with a '");
                return;
            }

            string[] splits = options.Trim()[1..(options.Length - 1)].Split("' '");

            if (splits.Length == 1)
            {
                await Pollbutton("'" + splits[0] + "' 'Yes' 'No'");
                return;
            }

            if (splits.Length > 26)
            {
                LogModule.LogMessage(LogSeverity.Error, "There cannot be more than 26 options (including the title)");
                return;
            }

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = "",
                Timestamp = DateTime.Now,
                Title = ":bar_chart: " + splits[0],
            };

            for (int i = 1; i < splits.Length; i++) e.Description += ":regional_indicator_" + letters[i - 1] + ": - " + Code(splits[i]) + "\n";

            int buttons = splits.Length - 1;

            ComponentBuilder c = new() { ActionRows = new(), };

            string hash = Hashes.SHA256(splits[0] + DateTime.Now);

            for (int i = 1; i < splits.Length; i++)
            {
                ActionRowBuilder a = new();

                int j = 0;

                for (_ = 0; i < splits.Length; i++)
                {
                    if (j >= 5)
                    {
                        i--;
                        break;
                    }

                    ButtonBuilder b = new()
                    {
                        CustomId = "poll " + i + " " + hash,
                        Emote = new Emoji(letterEmotes[i - 1]),
                        Label = splits[i],
                        Style = ButtonStyle.Primary,
                    };

                    a.WithComponent(b.Build());

                    j++;
                }

                c.ActionRows.Add(a);
            }

            IUserMessage msg = await ReplyAsync("", false, e.Build(), component: c.Build());
            Data.misc.Data.polls.Add(new Poll() { hash = hash, voters = new(), });
        }

        [Command("quote")]
        [Summary("Quotes a user on a message")]
        public async Task Quote([Summary("The user to quote")] IUser user, [Remainder][Summary("The message you want to quote")] string quote)
        {
            ComponentBuilder c = new();
            ButtonBuilder b = new()
            {
                CustomId = "delete " + user.Username + "#" + user.Discriminator,
                Label = "Delete",
                Style = ButtonStyle.Danger,
            };
            c.WithButton(b);

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = Format.Quote(Italics('"' + quote + '"')) + "\n\n" +
                    Bold("- " + user.Mention),
            };
            e.WithFooter("The person this is targeted towards can choose to delete this message.");

            await ReplyAsync("", false, e.Build(), component: c.Build());
        }

        [Command("randomfact")]
        [Summary("Shows a random fact found on the internet")]
        public async Task Randomfact()
        {
            HttpClient client = new();

            JObject obj = JObject.Parse(await client.GetStringAsync("https://uselessfacts.jsph.pl/random.json?language=en"));

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = Code(obj["text"].ToString().Replace("`", "'")),
                Timestamp = DateTime.Now,
                Title = "Random Fact by " + obj["source"].ToString(),
                Url = obj["source_url"].ToString()
            };
            e.WithFooter("Fact found using 'https://uselessfacts.jsph.pl'");

            await ReplyAsync("", false, e.Build());
        }

        [Command("say")]
        [Summary("Causes the bot to say what you want it to (Deletes the original message)")]
        public async Task Say([Remainder][Summary("The message for the bot to say")] string message)
        {
            await Context.Message.DeleteAsync();

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = message,
            };

            await ReplyAsync("", false, e.Build());
        }

        [Command("sayfor")]
        [Summary("Causes the bot to say what you want it to, but as someone else (Deletes the original message)")]
        public async Task Sayfor([Summary("The user the bot pretends to be")] IUser user, [Remainder][Summary("The message for the bot to say")] string message)
        {
            await Context.Message.DeleteAsync();

            ComponentBuilder c = new();
            ButtonBuilder b = new()
            {
                CustomId = "delete " + user.Username + "#" + user.Discriminator,
                Label = "Delete",
                Style = ButtonStyle.Danger,
            };
            c.WithButton(b);

            EmbedBuilder e = new()
            {
                Color = Colors.DefaultColor,
                Description = message,
                Title = "Message from " + user.Username + "#" + user.Discriminator,
            };
            e.WithFooter("The person this is targeted towards can choose to delete this message.");

            await ReplyAsync("", false, e.Build(), component: c.Build());
        }
    
        [Command("what")]
        [Summary("Number 7: Student watches porn, and gets naked.")]
        public async Task What([Remainder][Summary("the")] string msg = "Don't insult Delaware it's fire")
        {
            string response;

            response = msg.Trim().ToLower() switch
            {
                "ball" or "nut" or "balls" or "nuts" =>
                    "nuts.\n" +
                    "balls, even.",

                "cock" or "dick" =>
                    "dick.\n" +
                    "cock, even.",

                "don't insult delaware it's fire" =>
                    "when you dont change the message",

                "ok" =>
                    "please become funny",

                "penis" =>
                    "𝓹𝓮𝓷𝓲𝓼",

                "testicals" =>
                    "clearly somebody doesnt know how to fucking spell testicles. just use 'balls,' bitch",

                "testicles" =>
                    "𝓽𝓮𝓼𝓽𝓲𝓬𝓵𝓮𝓼",

                _ => "mmm im so hungry",
            };

            EmbedBuilder e = new()
            {
                Color = Colors.RandColor,
                Description = response,
                Timestamp = DateTime.Now,
            };

            await ReplyAsync("", false, e.Build());
        }   

        [Command("8ball")]
        [Summary("Gives a question, and the bot answers it (pretty badly)")]
        public async Task Ball8([Remainder][Summary("The question to ask the bot")] string question)
        {
            (string, float)[] answers = new[]
            {
                ("Definitely", 0.9f),
                ("Don't Ask Me", -1),
                ("I Don't Know", -1),
                ("I Don't Really Know", -1),
                ("Maybe", 0.5f),
                ("Maybe?", 0.5f),
                ("Never Happening", 0),
                ("No", 0.2f),
                ("Nope", 0.1f),
                ("Not At All", 0),
                ("Not Sure", -1),
                ("Yes", 0.8f),
                ("100%", 1f),
            };

            EmbedBuilder e = new()
            {
                Color = Color.DarkGrey,
                Description = "",
                Footer = new() { Text = "Question: " + question },
                Timestamp = DateTime.Now,
            };
            for (int i = 0; i < new Random().Next(1, 4); i++) e.Description += "u";
            for (int i = 0; i < new Random().Next(1, 8); i++) e.Description += "h";
            for (int i = 0; i < new Random().Next(1, 6); i++) e.Description += ".";

            IUserMessage msg = await ReplyAsync("", false, e.Build());

            await Task.Delay(new Random().Next(1500, 3000));

            (string, float) response = answers[new Random().Next(0, answers.Length)];

            e.Color = Colors.DefaultColor;
            e.Description = Bold(response.Item1) + "\n\n";
            e.Title = "8Ball Question";

            if (response.Item2 >= 0)
            {
                e.Description += Code((response.Item2 * 100) + "%") + "\n";

                for (int i = 0; i < (int)(response.Item2 * 10); i++) e.Description += ":green_square:";
                for (int i = (int)(response.Item2 * 10); i < 10; i++) e.Description += ":white_large_square:";
            }
            else
            {
                e.Description += Code("?? %") + "\n";
                for (int i = 0; i < 10; i++) e.Description += ":grey_question:";
            }
            
            await msg.ModifyAsync(x => x.Embed = e.Build());
        }
    }
}