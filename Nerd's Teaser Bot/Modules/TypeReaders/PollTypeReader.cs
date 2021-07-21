using Discord.Commands;
using NerdsTeaserBot.Misc;
using System;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.TypeReaders
{
    public class PollTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) => Task.FromResult(TypeReaderResult.FromSuccess(Data.misc.Data.polls.FindOrDefault(x => x.hash == input.Trim())));
    }
}
