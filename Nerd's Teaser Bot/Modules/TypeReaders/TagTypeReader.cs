using Discord.Commands;
using NerdsTeaserBot.Misc;
using System;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.TypeReaders
{
    public class TagTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) => Task.FromResult(TypeReaderResult.FromSuccess(Data.misc.Data.tags.FindOrDefault(x => x.name == input.Trim().ToLower())));
    }
}
