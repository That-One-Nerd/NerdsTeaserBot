using Discord;
using Discord.WebSocket;
using Nerd_STF.Lists;
using System;
using static Discord.Format;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class Poll
    {
        public string hash;
        public List<User> voters = new();

        public void Update(SocketUserMessage msg)
        {
            EmbedBuilder e = new List<Embed>(msg.Embeds)[0].ToEmbedBuilder();

            string[] options = e.Description.Split("\n");

            for (int i = 0; i < options.Length; i++)
            {
                int p = -1;
                for (int j = 0; j < options[i].Length; j++) if (options[i][j] == '`') p = j;

                options[i] = options[i].Remove(p);

                int num = voters.FindAll(x => x.option == i + 1).Length;

                options[i] += "` - " + Bold(num + " Votes");
            }

            e.Description = "";
            foreach (string s in options) e.Description += s + "\n";

            msg.ModifyAsync(x => x.Embed = e.Build());
        }

        [Serializable]
        public class User
        {
            public ulong id;
            public byte option;
        }
    }
}
