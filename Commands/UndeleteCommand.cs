using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DudesBot.Commands
{
    public class UndeleteCommand : BaseCommandModule
    {
        public static DiscordMessage LatestDelete {private get; set;}

        [Command("undelete")]
        public async Task MessageUndeleter(CommandContext context)
        {
            if(LatestDelete is not null)
            {
                var message = await Services.PinArchiveService.PinnedMessageEmbedder(LatestDelete);
                await context.Channel.SendMessageAsync(message);
                Console.WriteLine($"[{DateTime.Now}] Restored message \"{LatestDelete.Content}\" from {LatestDelete.Author.Username}:{LatestDelete.Author.Discriminator}");
                LatestDelete = null;
            }
            else
            {
                await context.RespondAsync("there's nothing to bring back..");
            }
        }
    }
}