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
using DudesBot.Services;
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

            DiscordClient discordClient = new(new DiscordConfiguration()
            {
                Token = botSettings.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
            });

            ServiceProvider services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PokeApiClient>()
                .AddSingleton<ReminderBackgroundService>()
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
            commands.RegisterCommands<Commands.ImageCommands>();
            commands.RegisterCommands<Commands.ReminderCommands>();
            commands.CommandErrored += CommandErrorHandler;
            commands.CommandExecuted += CommandExecutedHandler;

            discordClient.MessageUpdated += PinnedMessageHandler;

            await discordClient.ConnectAsync();
            Console.WriteLine("Connected to Discord");
            try
            {
                services.GetService<ReminderBackgroundService>().AttachClient(discordClient);
                await services.GetService<ReminderBackgroundService>().Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(-1);
        }

        static async Task CommandErrorHandler(CommandsNextExtension _, CommandErrorEventArgs eventArgs)
        {
            try //See if command errors due to a cooldown
            {
                ChecksFailedException checksFailedException = eventArgs.Exception as ChecksFailedException;
                IReadOnlyList<CheckBaseAttribute> failedcheckList = checksFailedException.FailedChecks;
                foreach (var check in failedcheckList)
                {
                    if(check is CooldownAttribute)
                    {
                        await eventArgs.Context.Message.RespondAsync("That command is on cooldown");
                    }
                }
            }
            catch (InvalidCastException) //For every other type of exception
            {
                Console.WriteLine(eventArgs.Exception);
                await eventArgs.Context.Message.RespondAsync(new DiscordEmbedBuilder().WithTitle("Exception Thrown").WithDescription($"The command threw an exception with the message `{eventArgs.Exception.Message}`"));
            }
        }

        static async Task CommandExecutedHandler(CommandsNextExtension extension, CommandExecutionEventArgs eventArgs)
        {
            DiscordMember commandUser = eventArgs.Context.Member;
            Console.WriteLine($"Received {eventArgs.Command} from {commandUser.Username}:{commandUser.Discriminator}");
        }

        static async Task PinnedMessageHandler(DiscordClient client, MessageUpdateEventArgs eventArgs)
        {
            if (eventArgs.MessageBefore is null || eventArgs.Message is null) { return; }
            if (eventArgs.MessageBefore.Pinned == false && eventArgs.Message.Pinned == true)
            {
                await Services.PinArchiveService.Archiver(eventArgs, botSettings.PinChannel);
            }
        }
    }
}
