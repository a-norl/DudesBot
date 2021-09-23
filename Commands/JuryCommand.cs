using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using ImageMagick;

namespace DudesBot.Commands
{
    public class JuryCommand : BaseCommandModule
    {
        public bool inSession = false;
        public static Dictionary<DiscordMember, DateTimeOffset> activeUserDict = new();

        [Command("jury")]
        public async Task Jury(CommandContext context, DiscordMember defendant, [RemainingText] string crime)
        {
            if (inSession)
            {
                context.Message.RespondAsync("The court is already in session!");
                return;
            }
            inSession = true;
            DiscordMember accuser = context.Member;
            DiscordMessageBuilder msgBld = new DiscordMessageBuilder();
            DiscordButtonComponent NotGuiltyButton = new DiscordButtonComponent(ButtonStyle.Success, "NotGuilty" + defendant.Id, "**NOT GUILTY**");
            DiscordButtonComponent GuiltyButton = new DiscordButtonComponent(ButtonStyle.Danger, "Guilty" + defendant.Id, "**GUILTY**");
            msgBld.AddComponents(new DiscordComponent[] { NotGuiltyButton, GuiltyButton });
            msgBld.WithContent(Formatter.Mention(defendant) + "! You stand accused of **" + crime + "**! \n How do you plead?");
            await context.Channel.SendMessageAsync(msgBld);
        }

        public static async Task ActiveUserCounter(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if(eventArgs.Message.Author.IsBot) {return;}
            if (activeUserDict.ContainsKey((DiscordMember)eventArgs.Author))
            {
                activeUserDict[(DiscordMember)eventArgs.Author] = eventArgs.Message.Timestamp;
            }
            else
            {
                activeUserDict.Add((DiscordMember)eventArgs.Author, eventArgs.Message.Timestamp);
            }
        }
    }
}