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
        public static Dictionary<DiscordMember, DateTimeOffset> activeUserDict;

        [Command("jury")]
        public async Task Jury(CommandContext context)
        {
            await context.RespondAsync(":)");
        }

        public static async Task ActiveUserCounter(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
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