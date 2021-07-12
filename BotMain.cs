using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DudesBot
{
    class BotMain
    {
        public static BotSettingsObject botSettings;

        static void Main(string[] args)
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
                .AddDbContextFactory<DudesDBContext>(options => options.UseSqlite($"Filename={botSettings.DBPath}"))
                .BuildServiceProvider();

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = botSettings.CommandPrefix,
                IgnoreExtraArguments = true,
                EnableMentionPrefix = true,
                Services = services
            });

            commands.RegisterCommands<Commands.MiscCommands>();
            commands.CommandErrored += CommandError;

            await discordClient.ConnectAsync();
            Console.WriteLine("Connected to Discord");
            await Task.Delay(-1);
        }

        static async Task CommandError(CommandsNextExtension extension, CommandErrorEventArgs eventArgs)
        {
            try
            {
                ChecksFailedException checksFailedException = (ChecksFailedException) eventArgs.Exception;
                IReadOnlyList<CheckBaseAttribute> failedcheckList = checksFailedException.FailedChecks;
                IEnumerable<CheckBaseAttribute> cooldownCheckList = failedcheckList
                    .Where(check => check.ToString().Contains("CooldownAttribute"));
                foreach(CheckBaseAttribute check in cooldownCheckList)
                {
                    await eventArgs.Context.Message.RespondAsync("That command is on cooldown");
                }
                
            }
            catch
            {
                string exception = eventArgs.Exception.GetBaseException().ToString();
                Console.WriteLine(exception.ToString());
                await eventArgs.Context.Message.RespondAsync(new DiscordEmbedBuilder().WithTitle("Exception Thrown").WithDescription(exception));
            }


        }
    }
}
