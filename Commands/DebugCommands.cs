using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DudesBot.Commands
{
    public class DebugCommands : BaseCommandModule
    {
        public IDbContextFactory<DudesDBContext> ContextFactory { private get; set; }
        public HttpClient httpClient { private get; set; }

        [Command("dbdebug"), Hidden]
        public async Task DBTest(CommandContext context)
        {
            var dbContext = ContextFactory.CreateDbContext();
            var warn = dbContext.UserWarning
                .OrderBy(warn => warn.Id)
                .Last();

            await context.RespondAsync(warn.WarnReason);
        }

        [Command("cooldowntest"), Cooldown(1, 30, CooldownBucketType.User), Hidden]
        public async Task CooldownTest(CommandContext context)
        {
            await context.RespondAsync("response");
        }

        [Command("getavatar")]
        public async Task GetAvatar(CommandContext context, DiscordMember queriedUser)
        {
            await context.RespondAsync(await UtilityMethods.GetProfilePictureURL(queriedUser, httpClient));
        }

        [Command("getname"), Hidden, Priority(1)]
        public async Task GetNameDebug(CommandContext context, DiscordMember queriedUser)
        {
            await context.RespondAsync(UtilityMethods.GetName(queriedUser));
        }

        [Command("getname"), Hidden, Priority(0)]
        public async Task GetNameUserDebug(CommandContext context, DiscordUser queriedUser)
        {
            await context.RespondAsync(UtilityMethods.GetName(queriedUser));
        }

        [Command("embed"), Hidden]
        public async Task EmbedCommand(CommandContext context, DiscordMessage message)
        {
            await context.Channel.SendMessageAsync(await Services.PinArchiveService.PinnedMessageEmbedder(message));
        }

        [Command("fakeexecution"), Hidden]
        public async Task FakeCommandHandlerTester(CommandContext context, DiscordMember executor, [RemainingText] string commandString)
        {
            await ComponentHandlers.FakeCommandHandler(context.Client, commandString, context.Channel, executor);
        }

        [Command("buttonInteractivity"), Hidden]
        public async Task ButtonInteractivityTest(CommandContext context)
        {
            var responseMessageBuilder = new DiscordMessageBuilder()
                .WithContent("button")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "inter_test", "press_me"));

            var responseMessage = await context.RespondAsync(responseMessageBuilder);

            var interactivityResponse = await responseMessage.WaitForButtonAsync(context.User);
            if (!interactivityResponse.TimedOut)
            {
                await interactivityResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("button"));
            }
        }
    }
}