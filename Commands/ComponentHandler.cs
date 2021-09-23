#pragma warning disable CS1998
#pragma warning disable CS4014
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DudesBot.Commands
{
    public static class ComponentHandlers
    {
        public static async Task TestSelectHandler(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            if(eventArgs.Values[0].ToLower().Contains("mousetrap"))
            {
                Task.Run(() => {ImageCommands.MouseTrapImage(client, eventArgs, false);});
                return;
            }
            if(eventArgs.Values[0].ToLower().Contains("jar"))
            {
                FakeCommandHandler(client, $"!jar <@{eventArgs.User.Id}>", eventArgs.Channel);
                return;
            }
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(new DiscordMessageBuilder()
            {
                Content = $"You selected {eventArgs.Values[0]}"
            })
            .AsEphemeral(true));
        }

        public static async Task FakeCommandHandler(DiscordClient client, string commandStringRaw, DiscordChannel channel, DiscordUser executor = null)
        {
            var cnext = client.GetCommandsNext();
            var prefix = commandStringRaw.Substring(0, 1);
            var commandString = commandStringRaw.Substring(1);

            var command = cnext.FindCommand(commandString, out var args);

            if(executor is null)
            {
                executor = client.CurrentUser;
            }

            var context = cnext.CreateFakeContext(executor, channel, commandStringRaw, prefix, command, args);
            Task.Run(async () => await cnext.ExecuteCommandAsync(context));

        }
    }
}