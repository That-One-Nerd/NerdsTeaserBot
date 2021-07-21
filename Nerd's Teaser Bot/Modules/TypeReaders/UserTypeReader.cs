using Discord.Commands;
using NerdsTeaserBot.Misc;
using System;
using System.Threading.Tasks;

namespace NerdsTeaserBot.Modules.TypeReaders
{
    public class UserTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) => Task.FromResult(TypeReaderResult.FromSuccess(Data.users.Data.FindOrDefault(x => x.userID.ToString() == input || "<@" + x.userID + ">" == input || "<@!" + x.userID + ">" == input)));
    }
}
