using Discord.Commands;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules.Models;
using System;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.TypeReaders
{
    public class WarnTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            foreach (User u in Data.users.Data)
            {
                Warn w = u.warns.FindOrDefault(x => x.hash == input);
                if (w is not null) return Task.FromResult(TypeReaderResult.FromSuccess(w));
            }
            return Task.FromResult(TypeReaderResult.FromSuccess(null));
        }
    }
}
