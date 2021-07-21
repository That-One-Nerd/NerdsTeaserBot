using Discord;
using NerdsTeaserBot.Misc;
using System;

namespace NerdsTeaserBot
{
    public static class Const
    {
        public static string Discriminator { get => Internals.client.CurrentUser.Discriminator; }
        public static string FullName { get => Username + "#" + Discriminator; }
        public static ulong ID { get => Internals.client.CurrentUser.Id; }
        public static string Username { get => Internals.client.CurrentUser.Username; }

        public static class Channels
        {
            public const ulong BotChangelog = 832234747538833448;
            public const ulong BotTesting = 778654642967937039;
        }

        public static class Colors
        {
            public static Color DefaultColor
            {
                get
                {
                    if (Internals.client.CurrentUser.Id == 843876640001884230) return new Color(141, 236, 34);
                    else return new Color(42, 137, 236);
                }
            }
            public static Color RandColor
            {
                get
                {
                    Color[] cols = new[]
                    {
                        Color.Blue,
                        Color.DarkBlue,
                        Color.DarkerGrey,
                        Color.DarkGreen,
                        Color.DarkGrey,
                        Color.DarkMagenta,
                        Color.DarkOrange,
                        Color.DarkPurple,
                        Color.DarkRed,
                        Color.DarkTeal,
                        Color.Default,
                        Color.Gold,
                        Color.Green,
                        Color.LighterGrey,
                        Color.LightGrey,
                        Color.LightOrange,
                        Color.Magenta,
                        Color.Orange,
                        Color.Purple,
                        Color.Red,
                        Color.Teal,
                    };

                    return cols[new Random().Next(0, cols.Length)];
                }
            }
            public static Color[] SeverityColors
            {
                get
                {
                    return new[]
                    {
                        Color.DarkRed,
                        Color.Red,
                        Color.Gold,
                        Color.LighterGrey,
                        Color.DarkGrey,
                        Color.LightGrey,
                    };
                }
            }
        }

        public static class Guilds
        {
            public const ulong NerdsTeasers = 755153205717106720;
            public const ulong TestServer = 778362746144030781;
        }

        public static class Log
        {
            public static void Write(string msg = "")
            {
                msg = msg.Replace('\n', ' ');
                Data.log.Data += "[" + DateTime.Now + "]: " + msg + "\n";
                Console.WriteLine("Logged: " + msg);
                Data.log.Save();
            }
        }

        public static string LogItem() => LogItem(Internals.context.User);
        public static string LogItem(IUser user) { return user.Username + "#" + user.Discriminator + " (" + user.Id + ")"; }
    }
}