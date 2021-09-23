using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace DudesBot.Commands
{
    public class DisallowUserAttribute : CheckBaseAttribute
    {
        public ulong DisallowedUserID {get; private set;}

        public DisallowUserAttribute(ulong id)
        {
            DisallowedUserID = id;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
        {
            return Task.FromResult(!(DisallowedUserID == context.User.Id));
        }
    }

    public class RequireAttachmentAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
        {
            bool hasAttachment = false;
            if(context.Message.Attachments.Count > 0)
            {
                hasAttachment = true;
            }
            return Task.FromResult(hasAttachment);
        }
    }
}