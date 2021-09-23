using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;
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
        public async Task JailUser(CommandContext context, DiscordMember prisoner)
        {
            MagickImageCollection animation = new();
            MagickImage bars = new($"Resources{Path.DirectorySeparatorChar}jail_bars.png");
            MagickImage jailImage = new MagickImage(MagickColors.Black, 600, 600);
            int animLength = 60;
            for (int i = 0; i < animLength; i++)
            {
                MagickImage newBackground = new MagickImage(jailImage);
                newBackground.Composite(jailImage, 0, -600 + ((i/animLength) * 600), CompositeOperator.Over);
                newBackground.AnimationDelay = 16;
                newBackground.GifDisposeMethod = GifDisposeMethod.Background;
                animation.Add(newBackground);
            }
            await ImageCommands.SendImage(animation, context.Message, "");
        }
    }
}