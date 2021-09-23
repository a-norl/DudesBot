#pragma warning disable CS1998
#pragma warning disable CS4014
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
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DudesBot.Commands;
using DudesBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeApiNet;
using VideoLibrary;

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
                .AddSingleton<VideoClient>()
                .AddSingleton<ReminderBackgroundService>()
                .AddSingleton(new MessageSentWorker(discordClient))
                .AddDbContextFactory<DudesDBContext>(options => options.UseSqlite($"Filename={botSettings.DBPath}"))
                .BuildServiceProvider();

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = botSettings.CommandPrefix,
                IgnoreExtraArguments = true,
                EnableMentionPrefix = true,
                Services = services
            });

            SlashCommandsExtension slash = discordClient.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = services
            });

            InteractivityExtension interactivity = discordClient.UseInteractivity(new InteractivityConfiguration()
            {
                PaginationButtons = new PaginationButtons()
                {
                    Left = new DiscordButtonComponent(ButtonStyle.Primary, "left", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⬅️"))),
                    Right = new DiscordButtonComponent(ButtonStyle.Primary, "right", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➡️"))),
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Primary, "skipleft", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏪"))),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Primary, "skipright", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏭️"))),
                    Stop = new DiscordButtonComponent(ButtonStyle.Primary, "stop", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏹️"))),
                },
                ButtonBehavior = ButtonPaginationBehavior.Disable,
                Timeout = TimeSpan.FromSeconds(120),
                AckPaginationButtons = true
            });

            //Registering Commands
            commands.RegisterCommands<DebugCommands>(); //Turn off when in actual use

            commands.RegisterCommands<MiscCommands>();
            commands.RegisterCommands<PokemonCommands>();
            commands.RegisterCommands<ImageCommands>();
            commands.RegisterCommands<CustomCommands>();
            commands.RegisterCommands<ImageMusicCommand>();
            if (botSettings.WarningCommand) { commands.RegisterCommands<WarnCommands>(); }
            if (botSettings.ReminderCommand) { commands.RegisterCommands<ReminderCommands>(); }
            if (botSettings.UndeleteCommand) { commands.RegisterCommands<UndeleteCommand>(); }

            commands.CommandErrored += CommandErrorHandler;
            commands.CommandExecuted += CommandExecutedHandler;

            // slash.RegisterCommands<ImageContextMenu>(846505700330962984); //Bot Testing server
            slash.RegisterCommands<ImageContextMenu>(824167452934012948); //Main server

            //Attaching Event Handlers
            discordClient.MessageUpdated += PinnedMessageHandler;
            discordClient.MessageCreated += MessageCreatedHandler;
            discordClient.MessageDeleted += MessageDeleteHandler;
            discordClient.ComponentInteractionCreated += ComponentInteractionCreateHandler;

            await discordClient.ConnectAsync();
            Console.WriteLine("Connected to Discord");

            if (botSettings.ReminderCommand)
            {
                services.GetService<ReminderBackgroundService>().AttachClient(discordClient);
                await services.GetService<ReminderBackgroundService>().Start();
            }
            services.GetService<HttpClient>().DefaultRequestHeaders.Add("User-Agent", "Discord Bot By anorl#7827");
            await Task.Delay(-1);
        }

        static async Task MessageCreatedHandler(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            client.GetCommandsNext().Services.GetService<MessageSentWorker>().Enqueue(eventArgs.Message); //Sends message to the MessageSendWorker
            if (botSettings.QuietChannels.Contains(eventArgs.Channel.Id) && eventArgs.Author.IsBot)
            {
                Task.Run(() => { eventArgs.Message.DeleteAsync(); });
            }
        }

        static async Task MessageDeleteHandler(DiscordClient client, MessageDeleteEventArgs eventArgs)
        {
            if (eventArgs.Message is null) { return; }
            if (eventArgs.Message.Author is null) { return; }
            if (eventArgs.Message.Author.IsBot) { return; }
            UndeleteCommand.LatestDelete = eventArgs.Message;
        }

        static async Task ComponentInteractionCreateHandler(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Id == "mousetrap_button") { ImageCommands.MouseTrapImage(client, eventArgs); }
            if (eventArgs.Id == "test_select") { ComponentHandlers.TestSelectHandler(client, eventArgs); }
        }

        static async Task CommandErrorHandler(CommandsNextExtension _, CommandErrorEventArgs eventArgs)
        {
            try //See if command errors due to a cooldown
            {
                ChecksFailedException checksFailedException = eventArgs.Exception as ChecksFailedException;
                IReadOnlyList<CheckBaseAttribute> failedcheckList = checksFailedException.FailedChecks;
                foreach (var check in failedcheckList)
                {
                    switch (check)
                    {
                        case CooldownAttribute:
                            await eventArgs.Context.Message.RespondAsync("That command is on cooldown");
                            break;
                        case RequireAttachmentAttribute:
                            await eventArgs.Context.Message.RespondAsync("Command requires an attachment");
                            break;
                        case DisallowUserAttribute:
                            await eventArgs.Context.Message.RespondAsync("you specifically cannot use this command.");
                            break;
                    }
                }
            }
            catch (NullReferenceException) //For every other type of exception
            {
                Console.WriteLine(eventArgs.Exception);
                if (eventArgs.Exception is CommandNotFoundException) { return; }
                await eventArgs.Context.Message.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Exception Thrown")
                    .WithDescription($"The command threw an exception with the message\n```{eventArgs.Exception.Message}\n```")
                    .WithColor(new DiscordColor("36393F")));
            }
        }

        static async Task CommandExecutedHandler(CommandsNextExtension extension, CommandExecutionEventArgs eventArgs)
        {
            DiscordMember commandUser = eventArgs.Context.Member;
            Console.WriteLine($"[{DateTime.Now}] Executed {eventArgs.Command} from {commandUser.Username}:{commandUser.Discriminator} in {eventArgs.Context.Channel.Name}:{eventArgs.Context.Guild.Name}");
        }

        static async Task PinnedMessageHandler(DiscordClient client, MessageUpdateEventArgs eventArgs)
        {
            // if (eventArgs.MessageBefore is null || eventArgs.Message is null) { return; }
            if (eventArgs.Message.Pinned == true)
            {
                Task.Run(async () => { await PinArchiveService.Archiver(eventArgs, botSettings.PinChannel); });
            }
        }
    }
}
