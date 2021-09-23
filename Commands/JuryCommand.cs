using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using ImageMagick;
using Emzi0767;
using DSharpPlus.SlashCommands;

namespace DudesBot.Commands
{
    public class JuryCommand : BaseCommandModule
    {
        public static bool inSession = false;
        public static Dictionary<DiscordMember, DateTimeOffset> activeUserDict = new();

        [Command("jury")]
        public async Task Jury(CommandContext context, DiscordMember defendant, [RemainingText] string crime)
        {
            if (inSession)
            {
                await context.Message.RespondAsync("The court is already in session!");
                return;
            }
            inSession = true;
            DiscordMember accuser = context.Member;
            DiscordMessageBuilder msgBld = new DiscordMessageBuilder();
            DiscordButtonComponent NotGuiltyButton = new DiscordButtonComponent(ButtonStyle.Success, "JURY_Innocent" + defendant.Id, "**NOT GUILTY**");
            DiscordButtonComponent GuiltyButton = new DiscordButtonComponent(ButtonStyle.Danger, "JURY_Guilty" + defendant.Id, "**GUILTY**");
            msgBld.AddComponents(new DiscordComponent[] { NotGuiltyButton, GuiltyButton });
            msgBld.WithContent(Formatter.Mention(defendant) + "! You stand accused of **" + crime + "**! \n How do you plead?");
            await context.Channel.SendMessageAsync(msgBld);
        }

        public static async Task ActiveUserCounter(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Message.Author.IsBot) { return; }
            if (activeUserDict.ContainsKey((DiscordMember)eventArgs.Author))
            {
                activeUserDict[(DiscordMember)eventArgs.Author] = eventArgs.Message.Timestamp;
            }
            else
            {
                activeUserDict.Add((DiscordMember)eventArgs.Author, eventArgs.Message.Timestamp);
            }
        }

        public static async Task JuryButtonsHandler(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            var buttonID = eventArgs.Interaction.Data.CustomId;
            await eventArgs.Channel.SendMessageAsync(buttonID);
            if (!buttonID.Contains("JURY"))
            {
                return;
            }
            if (buttonID.Contains("Innocent") && buttonID.Contains(eventArgs.User.Id.ToString()))
            {
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent($"{Formatter.Mention(eventArgs.User)} has plead not guilty"));
                return;
            }
            if (buttonID.Contains("Guilty") && buttonID.Contains(eventArgs.User.Id.ToString()))
            {
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                        .WithContent($"{Formatter.Mention(eventArgs.User)} has plead guilty"));
                return;
            }

            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.Pong);
        }

        [Command("jail")]
        public async Task JailUser(DiscordMember prisoner)
        {

            // ImageCommands.SendImage();
        }
    }
}