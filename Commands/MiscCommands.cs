using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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
            Regex diceFormat = new(@"[0-9]+d[0-9]+", RegexOptions.IgnoreCase);
            Regex halfDiceFormat = new(@"d[0-9]+", RegexOptions.IgnoreCase);
            int rollTotal = 0;
            List<int> rollHist = new();
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

        [Command("recursion")]
        public async Task RecursionDM(CommandContext context)
        {
            var DMChannel = await context.Member.CreateDmChannelAsync();
            for (int i = 0; i < 20; i++)
            {
                await DMChannel.SendMessageAsync("recursion");
                Thread.Sleep(i * 200);
            }

        }

        [Command("uninstall")]
        public async Task UninstallRat(CommandContext context, DiscordMember member)
        {
            var message = await context.RespondAsync($"uninstalling {UtilityMethods.GetName(member)}");
            Thread.Sleep(1000);

            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n▁▁▁▁▁▁▁▁▁▁▁▁▁ 0%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n█▁▁▁▁▁▁▁▁▁ 10%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n██▁▁▁▁▁▁▁▁ 20%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n███▁▁▁▁▁▁▁ 30%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n████▁▁▁▁▁▁ 40%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n██████▁▁▁▁▁▁ 50%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n██████▁▁▁▁ 60%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n███████▁▁▁ 70%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n████████▁▁ 80%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n█████████▁ 90%");
            Thread.Sleep(500);
            await message.ModifyAsync($"uninstalling {UtilityMethods.GetName(member)}\n██████████ 100%");
            Thread.Sleep(500);
            await message.ModifyAsync($"{UtilityMethods.GetName(member)} has been uninstalled");
        }

        [Command("install")]
        public async Task installRat(CommandContext context, DiscordMember member)
        {
            var message = await context.RespondAsync($"installing {UtilityMethods.GetName(member)}");
            Thread.Sleep(1000);

            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n▁▁▁▁▁▁▁▁▁▁▁▁▁ 0%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n█▁▁▁▁▁▁▁▁▁ 10%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n██▁▁▁▁▁▁▁▁ 20%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n███▁▁▁▁▁▁▁ 30%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n████▁▁▁▁▁▁ 40%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n██████▁▁▁▁▁▁ 50%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n██████▁▁▁▁ 60%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n███████▁▁▁ 70%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n████████▁▁ 80%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n█████████▁ 90%");
            Thread.Sleep(500);
            await message.ModifyAsync($"installing {UtilityMethods.GetName(member)}\n██████████ 100%");
            Thread.Sleep(500);
            await message.ModifyAsync($"{UtilityMethods.GetName(member)} has been installed");
        }

        [Command("nickname"), RequireOwner]
        public async Task ChangeBotNickname(CommandContext context, string newName)
        {
            await context.Guild.CurrentMember.ModifyAsync(self=>
            {
                self.Nickname = newName;
            });
            await context.RespondAsync($"Nickname changed to {newName}");
        }

        [Command("leave"), RequireOwner]
        public async Task LeaveSearver(CommandContext context)
        {
            await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":wave:"));
            await context.Guild.LeaveAsync();
        }

        [Command("select"), Hidden]
        public async Task SelectMenuTest(CommandContext context, params string[] options)
        {
            var optionList = new  List<DiscordSelectComponentOption>();
            foreach(var optionString in options)
            {
                optionList.Add(new DiscordSelectComponentOption(optionString, optionString, optionString + " description"));
            }
            var selectMenu = new DiscordSelectComponent("test_select", "select", optionList);
            await context.RespondAsync(new DiscordMessageBuilder().AddComponents(selectMenu).WithContent("select menu test"));
        }

        [Command("eval"),RequireOwner]
        public async Task EvaluationCommand(CommandContext context, [RemainingText] string toEval)
        {
            var responseMsg = await context.RespondAsync(new DiscordEmbedBuilder()
                                        .WithTitle("Evaluating...")
                                        .WithColor(DiscordColor.Yellow));
            
            var globals = new EvaluationEnvironment(context);
            var scriptOptions = ScriptOptions.Default
                .WithImports("System", "System.Collections.Generic", "System.Diagnostics", "System.Linq", "System.Net.Http", "System.Net.Http.Headers", "System.Reflection", "System.Text", 
                             "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Entities", "DSharpPlus.EventArgs", "DSharpPlus.Exceptions", "ImageMagick", "DudesBot.Commands", "DudesBot")
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location)));

            var compilationStopwatch = Stopwatch.StartNew();
            var cScript = CSharpScript.Create(toEval, scriptOptions, typeof(EvaluationEnvironment));
            var cScriptCompiled = cScript.Compile();
            compilationStopwatch.Stop();

            if(cScriptCompiled.Any(error => error.Severity == DiagnosticSeverity.Error))
            {
                string errorList = "```\n";
                foreach(var error in cScriptCompiled)
                {
                    errorList += $"{error}\n";
                }
                errorList += "```";
                var errorEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Compilation failed")
                    .WithDescription($"Compilation failed after {compilationStopwatch.ElapsedMilliseconds} ms")
                    .AddField("Errors", errorList)
                    .WithColor(DiscordColor.Red);

                await responseMsg.ModifyAsync(embed: errorEmbed.Build());
                return;
            }

            Exception resultException = null;
            ScriptState<object> scriptState = null;
            var executionStopwatch = Stopwatch.StartNew();
            try
            {
                scriptState = await cScript.RunAsync(globals);
                resultException = scriptState.Exception;
            }
            catch(Exception e)
            {
                resultException = e;
            }

            if(resultException is not null)
            {
                var errorEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Execution Failed")
                    .WithColor(DiscordColor.Red)
                    .WithDescription($"Execution failed after {executionStopwatch.ElapsedMilliseconds} ms with `{resultException.GetType()}: {resultException.Message}`.");
                await responseMsg.ModifyAsync(embed: errorEmbed.Build());
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Evaluation successful")
                .WithColor(DiscordColor.Green);

            embed.AddField("Result", (scriptState.ReturnValue != null && scriptState.ReturnValue.ToString() != "") ? scriptState.ReturnValue.ToString() : "No value return", false)
                .AddField("Compilation Time", $"{compilationStopwatch.ElapsedMilliseconds} ms", true)
                .AddField("Execution Time", $"{executionStopwatch.ElapsedMilliseconds} ms", true);

            if(scriptState.ReturnValue is not null)
            {
                embed.AddField("Return Type", scriptState.ReturnValue.GetType().ToString(), true);
            }

            await responseMsg.ModifyAsync(embed: embed.Build());

        }

    }

    public sealed class EvaluationEnvironment
    {
        public CommandContext Context { get; }

        public DiscordMessage Message => this.Context.Message;
        public DiscordChannel Channel => this.Context.Channel;
        public DiscordGuild Guild => this.Context.Guild;
        public DiscordUser User => this.Context.User;
        public DiscordMember Member => this.Context.Member;
        public DiscordClient Client => this.Context.Client;
        public HttpClient Http => this.Context.Services.GetService<HttpClient>();

        public EvaluationEnvironment(CommandContext ctx)
        {
            this.Context = ctx;
        }
    }
}