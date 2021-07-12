using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DudesBot.Commands
{
    public class MiscCommands : BaseCommandModule
    {

        public Random GetRandom { private get; set; }
        public IDbContextFactory<DudesDBContext> ContextFactory { private get; set; }

        private static readonly Dictionary<int, string> pollNumberDict = new Dictionary<int, string>{
            {1, ":one:"},
            {2, ":two:"},
            {3, ":three:"},
            {4, ":four:"},
            {5, ":five:"},
            {6, ":six:"},
            {7, ":seven:"},
            {8, ":eight:"},
            {9, ":nine:"},
            {10, ":keycap_ten:"},
        };

        [Command("coinflip")]
        public async Task CoinFlip(CommandContext context)
        {
            int coinRand = GetRandom.Next(1, 202);
            if (coinRand <= 100)
            {
                await context.RespondAsync("Heads.");
            }
            else if (coinRand <= 200 && coinRand > 100)
            {
                await context.RespondAsync("Tails.");
            }
            else if (coinRand == 201)
            {
                await context.RespondAsync("It Landed exactly on it's edge.");
            }
        }

        [Command("roll"), Description("Roll a dice")]
        public async Task DiceRoll(CommandContext context, [Description("Dice roll argument in 1d20 format")] String rollArg)
        {
            int diceAmount = 1;
            int diceSides = 20;
            Regex diceFormat = new Regex(@"[0-9]+d[0-9]+", RegexOptions.IgnoreCase);
            Regex halfDiceFormat = new Regex(@"d[0-9]+", RegexOptions.IgnoreCase);
            int rollTotal = 0;
            List<int> rollHist = new List<int>();
            string formattedRollList;

            if (diceFormat.IsMatch(rollArg))
            {
                var split = new Regex(@"d", RegexOptions.IgnoreCase).Split(rollArg);
                diceAmount = int.Parse(split[0]);
                diceSides = int.Parse(split[1]);
            }
            else if (halfDiceFormat.IsMatch(rollArg))
            {
                var split = new Regex(@"d", RegexOptions.IgnoreCase).Split(rollArg);
                diceSides = int.Parse(split[1]);
            }
            else
            {
                rollArg = "1d20 because fuck you";
            }


            for (int i = 1; i <= diceAmount; i++)
            {

                int diceRoll = GetRandom.Next(1, diceSides + 1);
                rollHist.Add(diceRoll);
                rollTotal += diceRoll;
            }

            if (diceAmount > 10)
            {
                formattedRollList = string.Join(" , ", rollHist.GetRange(0, 10));
                formattedRollList += " , ...";
            }
            else
            {
                formattedRollList = string.Join(", ", rollHist);
            }

            await context.RespondAsync($"**{rollTotal}**. Rolled From {rollArg} :game_die: [{formattedRollList}]");
        }

        [Command("poll"), Description("Use like `!poll <Topic:Text - Description of the poll> <Option1:Text> <Option2:Text> [Option3:Text] [Option4:Text] [Option5:Text] [Option6:Text] [Option7:Text] [Option8:Text] [Option9:Text] [Option10:Text]`")]
        public async Task Poll(CommandContext ctx, params string[] args)
        {
            if (args.Length > 11)
            {
                await ctx.RespondAsync("too many options dipshit");
                return;
            }
            List<String> argsList = args.ToList();

            var pollEmbed = new DiscordEmbedBuilder()
                                    .WithAuthor(name: ctx.Member.Nickname, iconUrl: ctx.Member.AvatarUrl)
                                    .WithTitle(argsList[0]);

            argsList.RemoveAt(0);

            for (int i = 1; i <= argsList.Count; i++)
            {
                string field = $"{pollNumberDict[i]} {argsList[i - 1]}";
                pollEmbed = pollEmbed.AddField("​", field, false);
            }

            await ctx.Message.DeleteAsync();
            DiscordMessage msg = await new DiscordMessageBuilder()
                        .WithEmbed(pollEmbed.Build())
                        .SendAsync(ctx.Channel);


            for (int i = 1; i <= argsList.Count; i++)
            {
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, pollNumberDict[i]));
            }

        }

    }
}