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
            
        }

        public static async Task ActiveUserCounter(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            // if (activeUserDict.Keys.Contains((DiscordMember)eventArgs.Message.Author))
            // {
            //     activeUserDict[(DiscordMember)eventArgs.Message.Author] = eventArgs.
            // }
        }
    }
}