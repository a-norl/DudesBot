using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus;

namespace DudesBot.Commands
{
    public class ImageContextMenu : ApplicationCommandModule
    {
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Quote")]
        public async Task QuoteContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            var messageLink = context.TargetMessage.JumpLink;
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!quote {messageLink}", context.Channel, context.Member);
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "GIF Kill")]
        public async Task GIFKillUserContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!gifkill <@{context.TargetUser.Id}>", context.Channel, context.Member);
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Squish")]
        public async Task SquishUserContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!squish <@{context.TargetUser.Id}>", context.Channel, context.Member);
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Jar")]
        public async Task JarUserContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!jar <@{context.TargetUser.Id}>", context.Channel, context.Member);
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Wojak")]
        public async Task WojakUserContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!wojak <@{context.TargetUser.Id}>", context.Channel, context.Member);
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Inflate")]
        public async Task InflateUserContext(ContextMenuContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
            await ComponentHandlers.FakeCommandHandler(context.Client, $"!inflate <@{context.TargetUser.Id}>", context.Channel, context.Member);
        }

        // [ContextMenu(ApplicationCommandType.UserContextMenu, "Deflate")]
        // public async Task DeflateUserContext(ContextMenuContext context)
        // {
        //     await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("generating"));
        //     await ComponentHandlers.FakeCommandHandler(context.Client, $"!deflate <@{context.TargetUser.Id}>", context.Channel, context.Member);
        // }
    }
}