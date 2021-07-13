using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using ImageMagick;

namespace DudesBot.Commands
{
    public class ImageCommands : BaseCommandModule
    {
        //TODO: inflate big and round command
        public HttpClient httpClient { private get; set; }

        private static readonly MagickImage BoxImage;
        private static readonly MagickImage GrabHandsImage;
        private static readonly MagickImage iFunnyWatermark;
        private static readonly MagickImage WojakImage;

        static ImageCommands()
        {
            BoxImage = new($"Resources{Path.DirectorySeparatorChar}box.png");
            GrabHandsImage = new($"Resources{Path.DirectorySeparatorChar}hands.png");
            iFunnyWatermark = new($"Resources{Path.DirectorySeparatorChar}ifunny-watermark.png");
            WojakImage = new($"Resources{Path.DirectorySeparatorChar}wojacktemplate.png");
        }

        [Command("imagetestmagick")]
        public async Task MagickImageTest(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage inputImage = await DownloadAttachment(context.Message);
            inputImage.Distort(DistortMethod.Barrel, 1.0, 0, 0);
            await SendImage(inputImage, context.Message, "output");
        }

        [Command("impact")]
        public async Task ImpactCommand(CommandContext context, string topText, string bottomText)
        {
            await context.TriggerTypingAsync();
            MagickImage inputImage = await DownloadAttachment(context.Message);
            MagickReadSettings textSettings = new()
            {
                Font = "Impact",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = inputImage.Height / 5,
                Width = inputImage.Width,
                //StrokeWidth = (inputImage.Height + inputImage.Width)/240,
                StrokeColor = MagickColors.Black,
                FillColor = MagickColors.White
            };
            MagickImage topCaption = new($"caption:{topText}", textSettings);
            MagickImage bottomCaption = new($"caption:{bottomText}", textSettings);
            inputImage.Composite(topCaption, 0, 0, CompositeOperator.Over);
            inputImage.Composite(bottomCaption, 0, inputImage.Height - (inputImage.Height / 5), CompositeOperator.Over);
            await SendImage(inputImage, context.Message, "output");
        }

        [Command("kill")]
        public async Task KillCommand(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            MagickImage backgroundImage = GrabHandsImage;
            victimAvatar.Resize(490, 490);
            backgroundImage.RotationalBlur(5.0);
            backgroundImage.Composite(victimAvatar, 420, 240, CompositeOperator.Over);
            await SendImage(backgroundImage, context.Message, $"{UtilityMethods.GetName(victimMember)} has been killed");
        }

        [Command("gifkill")]
        public async Task GIFKillCommand(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            victimAvatar.Resize(490, 490);

            MagickImageCollection killGIF = new();
            for (float i = 0; i <= 5.0; i += 1.0f)
            {
                IMagickImage<byte> backgroundImage = GrabHandsImage.Clone();
                backgroundImage.GifDisposeMethod = GifDisposeMethod.Background;
                backgroundImage.RotationalBlur(i);
                backgroundImage.BackgroundColor = MagickColors.Transparent;
                if(i % 2 == 0){backgroundImage.Rotate(i);}else{backgroundImage.Rotate(-i);}
                backgroundImage.Composite(victimAvatar.Clone(), 420, 240, CompositeOperator.Over);
                killGIF.Add(backgroundImage);
            }

            await SendImage(killGIF, context.Message, $"{UtilityMethods.GetName(victimMember)} has been killed");
        }

        [Command("inflate")]
        public async Task InflateBigAndRound(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            MagickImageCollection BigAndRoundGif = new();
            victimAvatar.Resize(250, 250);
            for (float i = 0; i <= 0.8; i += 0.05f)
            {
                IMagickImage<byte> tempImage = victimAvatar.Clone();
                tempImage.Distort(DistortMethod.Barrel, i, 0.0, 0.0);
                tempImage.AnimationDelay = 10;
                BigAndRoundGif.Add(tempImage);
            }
            BigAndRoundGif.Last().AnimationDelay = 100;
            await SendImage(BigAndRoundGif, context.Message, $"{UtilityMethods.GetName(victimMember)} has been inflated big and round");
        }

        [Command("deflate")]
        public async Task DeflateSmallAndRound(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            MagickImageCollection BigAndRoundGif = new();
            victimAvatar.Resize(250, 250);
            for (float i = 0; i >= -0.8; i -= 0.05f)
            {
                IMagickImage<byte> tempImage = victimAvatar.Clone();
                tempImage.Distort(DistortMethod.Barrel, i, 0.0, 0.0);
                tempImage.AnimationDelay = 10;
                BigAndRoundGif.Add(tempImage);
            }
            BigAndRoundGif.Last().AnimationDelay = 100;
            await SendImage(BigAndRoundGif, context.Message, $"{UtilityMethods.GetName(victimMember)} has been deflated small and (not?) round");
        }

        [Command("wojak"), Aliases("wojack")]
        public async Task WojakAvatar(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            IMagickImage<byte> wojakMask = WojakImage.Clone();
            MagickImage imageCanvas = new(MagickColors.White, WojakImage.Width, WojakImage.Height);
            victimAvatar.Resize(350, 350);
            imageCanvas.Composite(victimAvatar, 120, 60, CompositeOperator.Over);
            imageCanvas.Composite(wojakMask, 0, 0, CompositeOperator.Over);
            await SendImage(imageCanvas, context.Message, $"{UtilityMethods.GetName(victimMember)} has been drawn as a crying wojak, they have lost the argument");
        }

        private async Task<MagickImage> DownloadAttachment(DiscordMessage targetMessage)
        {
            IReadOnlyList<DiscordAttachment> attachmentList = targetMessage.Attachments;
            if (attachmentList.Count == 0) { return null; }
            DiscordAttachment attachment = attachmentList[0];
            try
            {
                byte[] attachmentByteArray = await httpClient.GetByteArrayAsync(attachment.Url);
                MagickImage downloadedImage = new(attachmentByteArray);
                return downloadedImage;
            }
            catch
            {
                return null;
            }
        }

        private async Task<MagickImage> DownloadAvatar(DiscordUser targetUser)
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
                    byte[] avatarByteArray = await getAvatarTask;
                    return new MagickImage(avatarByteArray);
                }
            }
        }

        private async Task SendImage(MagickImage attachment, DiscordMessage commandMessage, string message) //Send Png
        {
            attachment.Format = MagickFormat.Png;
            string filePath = $"Output{Path.DirectorySeparatorChar}output_{DateTime.Now.ToFileTime()}.png";
            attachment.Write(filePath);

            FileStream outputFile = File.OpenRead(filePath);

            DiscordMessageBuilder replyMessage = new DiscordMessageBuilder()
                .WithContent(message)
                .WithFile(outputFile);

            await commandMessage.RespondAsync(replyMessage);
            outputFile.Dispose();
            File.Delete(filePath);
        }

        private async Task SendImage(MagickImageCollection attachment, DiscordMessage commandMessage, string message) //Send gif
        {
            attachment[0].AnimationIterations = 0;
            string filePath = $"Output{Path.DirectorySeparatorChar}output_{DateTime.Now.ToFileTime()}.gif";
            attachment.Write(filePath);

            FileStream outputFile = File.OpenRead(filePath);

            DiscordMessageBuilder replyMessage = new DiscordMessageBuilder()
                .WithContent(message)
                .WithFile(outputFile);

            await commandMessage.RespondAsync(replyMessage);
            outputFile.Dispose();
            File.Delete(filePath);
        }
    }
}