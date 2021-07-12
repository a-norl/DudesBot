#pragma warning disable CS1998
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeApiNet;

namespace DudesBot
{
    class BotMain
    {
        public static BotSettingsObject botSettings;

        static void Main()
        {
            Console.WriteLine("Program Started");
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            string jsonString = await File.ReadAllTextAsync($"Resources{Path.DirectorySeparatorChar}BotConfig.json");
            botSettings = JsonSerializer.Deserialize<BotSettingsObject>(jsonString);

            DiscordClient discordClient = new DiscordClient(new DiscordConfiguration()
            {
                Token = botSettings.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
            });

            ServiceProvider services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PokeApiClient>()
                .AddDbContextFactory<DudesDBContext>(options => options.UseSqlite($"Filename={botSettings.DBPath}"))
                .BuildServiceProvider();

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = botSettings.CommandPrefix,
                IgnoreExtraArguments = true,
                EnableMentionPrefix = true,
                Services = services
            });
            commands.RegisterCommands<Commands.DebugCommands>(); //Turn off when in actual use

            commands.RegisterCommands<Commands.MiscCommands>();
            commands.RegisterCommands<Commands.PokemonCommands>();
            commands.RegisterCommands<Commands.WarnCommands>();
            commands.CommandErrored += CommandErrorHandler;
            commands.CommandExecuted += CommandExecutedHandler;

            discordClient.MessageUpdated += PinnedMessageHandler;

            await discordClient.ConnectAsync();
            Console.WriteLine("Connected to Discord");
            await Task.Delay(-1);
        }

        static async Task CommandErrorHandler(CommandsNextExtension extension, CommandErrorEventArgs eventArgs)
        {
            try //See if command errors due to a cooldown
            {
                ChecksFailedException checksFailedException = (ChecksFailedException)eventArgs.Exception;
                IReadOnlyList<CheckBaseAttribute> failedcheckList = checksFailedException.FailedChecks;
                IEnumerable<CheckBaseAttribute> cooldownCheckList = failedcheckList
                    .Where(check => check.ToString().Contains("CooldownAttribute"));
                foreach (CheckBaseAttribute check in cooldownCheckList)
                {
                    await eventArgs.Context.Message.RespondAsync("That command is on cooldown");
                }
            }
            catch (InvalidCastException) //For every other type of exception
            {
                string exceptionString = eventArgs.Exception.GetBaseException().ToString();
                Console.WriteLine(exceptionString.ToString());
                await eventArgs.Context.Message.RespondAsync(new DiscordEmbedBuilder().WithTitle("Exception Thrown").WithDescription(exceptionString));
            }
        }

        static async Task CommandExecutedHandler(CommandsNextExtension extension, CommandExecutionEventArgs eventArgs)
        {
            DiscordMember commandUser = eventArgs.Context.Member;
            Console.WriteLine($"Received {eventArgs.Command} from {commandUser.Username}:{commandUser.Discriminator}");
        }

        static async Task PinnedMessageHandler(DiscordClient client, MessageUpdateEventArgs eventArgs)
        {
            if (eventArgs.MessageBefore.Pinned == false && eventArgs.Message.Pinned == true)
            {
                await Services.PinArchiveService.Archiver(eventArgs, botSettings.PinChannel);
            }
        }
    }
}
