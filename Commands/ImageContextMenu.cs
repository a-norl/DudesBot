using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace DudesBot.Commands
{
    public class ImageContextMenu : ApplicationCommandModule
    {
        [ContextMenu(DSharpPlus.ApplicationCommandType.MessageContextMenu, "Quote")]
        public async Task QuoteContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            var messageLink = context.TargetMessage.JumpLink;
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!quote {messageLink}", context.Channel);
        }
    }
}