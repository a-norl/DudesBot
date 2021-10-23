#pragma warning disable CS4014
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;

namespace DudesBot.Commands
{
    public class JuryCommand : BaseCommandModule
    {
        public static bool inSession = false;
        public static Dictionary<DiscordMember, DateTimeOffset> activeUserDict = new();
        public HttpClient httpClient { private get; set; }

        private static MagickImage BarsImage = new($"Resources{Path.DirectorySeparatorChar}jail_bars.png");
        private static MagickImage JailImage = new($"Resources{Path.DirectorySeparatorChar}jail_background.png");

        public static string Crime = "";

        [Command("jury"), Hidden, RequireOwner]
        public async Task Jury(CommandContext context, DiscordMember defendant, [RemainingText] string crime)
        {
            if (inSession)
            {
                await context.Message.RespondAsync("The court is already in session!");
                return;
            }
            inSession = true;
            Crime = crime;
            DiscordMember accuser = context.Member;
            DiscordMessageBuilder msgBld = new DiscordMessageBuilder();
            DiscordButtonComponent NotGuiltyButton = new DiscordButtonComponent(ButtonStyle.Success, "JURY_Innocent" + defendant.Id, "**NOT GUILTY**");
            DiscordButtonComponent GuiltyButton = new DiscordButtonComponent(ButtonStyle.Danger, "JURY_Guilty" + defendant.Id, "**GUILTY**");
            msgBld.AddComponents(new DiscordComponent[] { NotGuiltyButton, GuiltyButton });
            msgBld.WithContent(Formatter.Mention(defendant) + "! You stand accused of **" + crime + "**! \n How do you plead?");
            await context.Channel.SendMessageAsync(msgBld);
        }

        public static async Task ActiveUserCounter(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Message.Author.IsBot) { return; }
            if (activeUserDict.ContainsKey((DiscordMember)eventArgs.Author))
            {
                activeUserDict[(DiscordMember)eventArgs.Author] = eventArgs.Message.Timestamp;
            }
            else
            {
                activeUserDict.Add((DiscordMember)eventArgs.Author, eventArgs.Message.Timestamp);
            }
        }

        public static async Task JuryButtonsHandler(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            var buttonID = eventArgs.Interaction.Data.CustomId;
            if (!buttonID.Contains("JURY"))
            {
                return;
            }
            if (buttonID.Contains("Innocent") && buttonID.Contains(eventArgs.User.Id.ToString()))
            {
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent($"{Formatter.Mention(eventArgs.User)} has plead not guilty"));
                return;
            }
            if (buttonID.Contains("Guilty") && buttonID.Contains(eventArgs.User.Id.ToString()))
            {
                Task.Run(async () =>
               {
                   await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, null);
                   var jailStream = await JailStatic(client, (DiscordMember)eventArgs.User, Crime);
                   await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"{Formatter.Mention(eventArgs.User)} has plead guilty")
                        .AddFile("jail.gif", jailStream));
                   await jailStream.DisposeAsync();
               });
                Crime = "";
                return;
            }
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.Pong);
        }

        public static async Task<Stream> JailStatic(DiscordClient client, DiscordMember prisoner, string crime)
        {
            HttpClient httpClient = client.GetCommandsNext().Services.GetService<HttpClient>();
            MemoryStream jailStream = new();
            MagickImageCollection jailAnimation = await JailUserBase(prisoner, crime, httpClient);

            await jailAnimation.WriteAsync(jailStream);
            jailStream.Position = 0;
            return jailStream;
        }

        [Command("jail")]
        public async Task JailUser(CommandContext context, DiscordMember prisoner, [RemainingText] string crime)
        {
            await context.TriggerTypingAsync();
            await ImageCommands.SendImage(await JailUserBase(prisoner, crime, httpClient), context.Message, "");
        }


        public static async Task<MagickImageCollection> JailUserBase(DiscordMember prisoner, string crime, HttpClient httpClient)
        {
            MagickImageCollection animation = new();
            MagickImage userAvatar = await DownloadAvatar(prisoner, httpClient);
            userAvatar.Resize(350, 350);
            int animLength = 30;
            MagickReadSettings textSettings = new()
            {
                Font = $"Resources{Path.DirectorySeparatorChar}impact.ttf",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = 100,
                Width = 600,
                FillColor = MagickColors.Red,
                StrokeColor = MagickColors.Black,
                StrokeWidth = 3
            };

            MagickImage textDrawn = new($"caption:{crime}", textSettings);

            MagickImage background = new(JailImage);
            background.Composite(userAvatar, 125, 125, CompositeOperator.Over);

            for (int i = 0; i <= animLength; i++)
            {
                MagickImage tempBackground = new(background);
                tempBackground.Composite(BarsImage, 0, (int)(-600 + (i / (animLength * 1.0) * 600)), CompositeOperator.Over);
                MagickImage newText = new(textDrawn);

                newText.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, i / (animLength * 1.0));
                tempBackground.Composite(newText, 0, 475, CompositeOperator.Over);
                tempBackground.AnimationDelay = 1;
                tempBackground.AnimationTicksPerSecond = 50;
                tempBackground.GifDisposeMethod = GifDisposeMethod.Background;
                animation.Add(tempBackground);
            }
            animation.First().AnimationDelay = 35;
            animation.Last().AnimationDelay = 100;
            animation.Optimize();
            return animation;
        }

        private static async Task<MagickImage> DownloadAvatar(DiscordUser targetUser, HttpClient httpClient)
        {
            if (targetUser as DiscordMember is null)
            {
                byte[] avatarByteArray = await httpClient.GetByteArrayAsync(targetUser.AvatarUrl);
                return new MagickImage(avatarByteArray);
            }
            else
            {
                DiscordMember targetMember = targetUser as DiscordMember;
                var getAvatarTask = httpClient.GetByteArrayAsync(targetMember.AvatarUrl);
                var getGuildAvatarTask = httpClient.GetByteArrayAsync(targetMember.GuildAvatarUrl);
                try
                {
                    byte[] guildAvatarByteArray = await getGuildAvatarTask;
                    return new MagickImage(guildAvatarByteArray);
                }
                catch
                {
                    try
                    {
                        byte[] avatarByteArray = await getAvatarTask;
                        return new MagickImage(avatarByteArray);
                    }
                    catch
                    {
                        return new MagickImage(MagickColors.White, 400, 400);
                    }
                }
            }
        }
    }
}