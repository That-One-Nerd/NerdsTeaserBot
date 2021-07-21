using Nerd_STF.Lists;
using System;

namespace NerdsTeaserBot.Modules.Models
{
    [Serializable]
    public class MiscData
    {
        public List<ulong> announceChannels = new();
        public ulong changelogChannel;
        public int levelCooldown = 60;
        public bool levelMention;
        public ulong logChannel;
        public List<string> memeSubs = new();
        public ulong messagingC;
        public List<Poll> polls = new();
        public string prefix = "n;";
        public List<ulong> publishChannels = new();
        public List<ulong> roles = new();
        public List<Tag> tags = new();
    }
}
