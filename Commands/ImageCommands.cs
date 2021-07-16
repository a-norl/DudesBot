using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
        private static readonly MagickImage AZQuoteImage;
        private static readonly MagickImage HandRight;
        private static readonly MagickImage HandLeft;
        private static readonly MagickImage CowboyHat;
        private static readonly MagickImage LayerHandLeft;
        private static readonly MagickImage LayerHandRight;
        private static readonly MagickImage LayerFingeeLeft;
        private static readonly MagickImage LayerFingeeRight;

        static ImageCommands()
        {
            BoxImage = new($"Resources{Path.DirectorySeparatorChar}box.png");
            GrabHandsImage = new($"Resources{Path.DirectorySeparatorChar}hands.png");
            iFunnyWatermark = new($"Resources{Path.DirectorySeparatorChar}ifunny-watermark.png");
            WojakImage = new($"Resources{Path.DirectorySeparatorChar}wojacktemplate.png");
            AZQuoteImage = new($"Resources{Path.DirectorySeparatorChar}quote_template.png");
            HandRight = new($"Resources{Path.DirectorySeparatorChar}hand_right.png");
            HandLeft = new($"Resources{Path.DirectorySeparatorChar}hand_left.png");
            HandRight.BackgroundColor = MagickColors.Transparent;
            HandLeft.BackgroundColor = MagickColors.Transparent;
            CowboyHat = new($"Resources{Path.DirectorySeparatorChar}cowboy_hat.png");
            LayerFingeeLeft = new($"Resources{Path.DirectorySeparatorChar}fingees_left_layer.png");
            LayerFingeeRight = new($"Resources{Path.DirectorySeparatorChar}fingees_right_layer.png");
            LayerHandLeft = new($"Resources{Path.DirectorySeparatorChar}hand_left_layer.png");
            LayerHandRight = new($"Resources{Path.DirectorySeparatorChar}hand_right_layer.png");
        }

        [Command("imagetestmagick")]
        public async Task MagickImageTest(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage inputImage = await DownloadAttachment(context.Message);
            inputImage.Resize(500, 300);
            MagickImageCollection sparkles = new();
            for (int i = 0; i <= 3; i++)
            {
                MagickImage sparkleImage = new(MagickColors.Transparent, inputImage.Width, inputImage.Height);
                sparkleImage.AddNoise(NoiseType.Random, Channels.All);
                sparkleImage.Emboss();
                var tempImage = inputImage.Clone();
                tempImage.Composite(sparkleImage, 0, 0, CompositeOperator.Over);
                tempImage.AnimationDelay = 10;
                sparkles.Add(tempImage);
            }

            await SendImage(sparkles, context.Message, "output");
        }

        [Command("imagetest4")]
        public async Task MagickImageTestTwo(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage inputImage = await DownloadAttachment(context.Message);
            var cloneFlip = inputImage.Clone();
            cloneFlip.Flip();
            cloneFlip.InverseLevelColors(MagickColors.Black, MagickColors.White);
            inputImage.Composite(cloneFlip, Channels.Red);
            cloneFlip.Flop();
            inputImage.Composite(cloneFlip, Channels.Blue);
            await SendImage(inputImage, context.Message, "ouput");
        }

        [Command("impact"), Cooldown(1, 10, CooldownBucketType.User)]
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
                StrokeColor = MagickColors.Black,
                FillColor = MagickColors.White
            };
            MagickImage topCaption = new($"caption:{topText}", textSettings);
            MagickImage bottomCaption = new($"caption:{bottomText}", textSettings);
            inputImage.Composite(topCaption, 0, 0, CompositeOperator.Over);
            inputImage.Composite(bottomCaption, 0, inputImage.Height - (inputImage.Height / 5), CompositeOperator.Over);
            await SendImage(inputImage, context.Message, "output");
        }

        [Command("kill"), Cooldown(1, 10, CooldownBucketType.User)]
        public async Task KillCommand(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            var backgroundImage = GrabHandsImage.Clone();
            victimAvatar.Resize(490, 490);
            backgroundImage.RotationalBlur(5.0);
            backgroundImage.Composite(victimAvatar, 420, 240, CompositeOperator.Over);
            await SendImage((MagickImage)backgroundImage, context.Message, $"{UtilityMethods.GetName(victimMember)} has been killed");
        }

        [Command("gifkill"), Cooldown(1, 10, CooldownBucketType.User)]
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
                if (i % 2 == 0) { backgroundImage.Rotate(i); } else { backgroundImage.Rotate(-i); }
                backgroundImage.Composite(victimAvatar.Clone(), 420, 240, CompositeOperator.Over);
                killGIF.Add(backgroundImage);
            }

            await SendImage(killGIF, context.Message, $"{UtilityMethods.GetName(victimMember)} has been killed");
        }

        [Command("inflate"), Priority(1), Cooldown(1, 10, CooldownBucketType.User)]
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

        [Command("inflate"), Priority(0), Cooldown(1, 10, CooldownBucketType.User),]
        public async Task InflateAttachment(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage inflateTarget = await DownloadAttachment(context.Message);
            MagickImageCollection BigAndRoundGif = new();
            inflateTarget.Resize(250, 250);
            for (float i = 0; i <= 0.8; i += 0.05f)
            {
                IMagickImage<byte> tempImage = inflateTarget.Clone();
                tempImage.Distort(DistortMethod.Barrel, i, 0.0, 0.0);
                tempImage.AnimationDelay = 10;
                BigAndRoundGif.Add(tempImage);
            }
            BigAndRoundGif.Last().AnimationDelay = 100;
            await SendImage(BigAndRoundGif, context.Message, $" ");
        }

        [Command("deflate"), Cooldown(1, 10, CooldownBucketType.User)]
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

        [Command("squish"), Cooldown(1, 10, CooldownBucketType.User), Priority(1)]
        public async Task SquishUser(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            var downloadAvatarTask = DownloadAvatar(victimMember);

            var killGIF = ImageSquisher(await downloadAvatarTask);
            await SendImage(killGIF, context.Message, "");
        }

        [Command("squish"), Cooldown(1, 10, CooldownBucketType.User), Priority(0)]
        public async Task SquishAttachment(CommandContext context)
        {
            await context.TriggerTypingAsync();

            var killGIF = ImageSquisher(await DownloadAttachment(context.Message));
            await SendImage(killGIF, context.Message, "");
        }

        private MagickImageCollection ImageSquisher(MagickImage victimImage)
        {
            MagickImage imageCanvas = new(MagickColors.Transparent, GrabHandsImage.Width, GrabHandsImage.Height);
            victimImage.Resize(490, 0);

            MagickImageCollection killGIF = new();
            for (int i = 0; i <= 20; i++)
            {
                var tempCanvas = imageCanvas.Clone();
                var tempAvatar = victimImage.Clone();
                tempCanvas.GifDisposeMethod = GifDisposeMethod.Background;
                tempCanvas.Composite(LayerHandLeft, 0 + (i * 10), 0, CompositeOperator.Over);
                tempCanvas.Composite(LayerHandRight, HandLeft.Width - (i * 10), 0, CompositeOperator.Over);
                var geometry = new MagickGeometry(tempAvatar.Width - (i * 10), tempAvatar.Height + (i * 10))
                {
                    IgnoreAspectRatio = true
                };
                tempAvatar.Resize(geometry);
                tempCanvas.Composite(tempAvatar, 420 + (i * 5), 240 - (i * 10), CompositeOperator.Over);
                tempCanvas.Composite(LayerFingeeLeft, 0 + (i * 10), 0, CompositeOperator.Over);
                tempCanvas.Composite(LayerFingeeRight, HandLeft.Width - (i * 10), 0, CompositeOperator.Over);
                tempCanvas.AnimationDelay = 7;
                tempCanvas.Resize(tempCanvas.Width / 2, tempCanvas.Height / 2);
                killGIF.Add(tempCanvas);
            }
            killGIF.Last().AnimationDelay = 50;
            return killGIF;
        }

        [Command("cowboy"), Cooldown(1, 10, CooldownBucketType.User), Hidden, RequireOwner]
        public async Task EmojiCowboy(CommandContext context, DiscordEmoji inputEmoji)
        {
            await context.TriggerTypingAsync();
            MagickImage emojiImage = await DownloadEmojiImage(inputEmoji);

            emojiImage.Resize(CowboyHat.Width, 0);
            MagickImage imageCanvas = new(MagickColors.Transparent, emojiImage.Width, emojiImage.Height + (CowboyHat.Height / 2));
            imageCanvas.Composite(emojiImage, 0, CowboyHat.Height - (CowboyHat.Height / 2), CompositeOperator.Over);
            imageCanvas.Composite(CowboyHat, 0, 0, CompositeOperator.Over);
            await SendImage(imageCanvas, context.Message, "");
        }

        [Command("wojak"), Aliases("wojack"), Cooldown(1, 10, CooldownBucketType.User)]
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

        [Command("jpeg"), Aliases("jpg"), Cooldown(1, 10, CooldownBucketType.User)]
        public async Task JpegCommand(CommandContext context, int compressionLevel = 10)
        {
            await context.TriggerTypingAsync();
            MagickImage inputImage = await DownloadAttachment(context.Message);
            int OGHeight = inputImage.Height;
            int OGWidth = inputImage.Width;
            inputImage.Resize(OGWidth / 10, OGHeight / 10);
            inputImage.Quality = compressionLevel;
            inputImage.Resize(OGWidth, OGHeight);
            await SendImage(inputImage, context.Message, "output", true);
        }

        [Command("text"), Hidden]
        public async Task TextTest(CommandContext context, string text, string font = "Comic-Sans-MS")
        {
            MagickReadSettings textSettings = new()
            {
                Font = font,
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = 200,
                Width = 500,
                StrokeColor = MagickColors.Black,
                FillColor = MagickColors.White
            };
            MagickImage textDrawn = new($"caption:{text}", textSettings);
            await SendImage(textDrawn, context.Message, "output");
        }

        [Command("quote"), Priority(1)]
        public async Task QuoteCommand(CommandContext context, DiscordMember quotedMember, [RemainingText] params string[] quoteRaw)
        {
            await context.TriggerTypingAsync();

            string quote = "";
            foreach (var word in quoteRaw)
            {
                quote += $"{word} ";
            }

            MagickImage quoteImage = await QuoteGenerator(quotedMember, quote);

            await SendImage(quoteImage, context.Message, " ");
        }

        [Command("quote"), Priority(2)]
        public async Task QuoteMessageCommand(CommandContext context, DiscordMessage inputMessage)
        {
            await context.TriggerTypingAsync();

            var quotedMember = await context.Guild.GetMemberAsync(inputMessage.Author.Id);
            MagickImage quoteImage = await QuoteGenerator(quotedMember, inputMessage.Content);

            await SendImage(quoteImage, context.Message, " ");
        }

        [Command("quote"), Priority(0)]
        public async Task QuoteReplyCommand(CommandContext context)
        {
            DiscordMessage quotedMessage = context.Message.ReferencedMessage;
            if (quotedMessage is null) { return; }
            await context.TriggerTypingAsync();
            var quotedMember = await context.Guild.GetMemberAsync(quotedMessage.Author.Id);
            MagickImage quoteImage = await QuoteGenerator(quotedMember, quotedMessage.Content);

            await SendImage(quoteImage, context.Message, " ");
        }



        private async Task<MagickImage> QuoteGenerator(DiscordMember quotedMember, string quote)
        {
            var getAvatarTask = DownloadAvatar(quotedMember);

            string quotedName = UtilityMethods.GetName(quotedMember);
            quotedName = $"~ {quotedName} ~";

            Regex nameCheck = new(@"[^\u0020-\u007F\u00A0-\u00FF\u0100-\u017F\u0180-\u024F\s]+");
            if (nameCheck.IsMatch(quotedName))
            {
                quotedName = $"~ {quotedMember.Username} ~";
            }

            string pattern = @"<:(.*?):\d+>";
            string replacement = "$1";
            quote = Regex.Replace(quote, pattern, replacement); ;

            quote = Regex.Replace(quote, "/(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?/gm", "");
            quote = Regex.Replace(quote, "<(.*?)>", "");

            var imageCanvas = new MagickImage(MagickColors.Transparent, AZQuoteImage.Width, AZQuoteImage.Height);
            MagickImage quotedAvatar = await getAvatarTask;
            quotedAvatar.AdaptiveResize(400, 400);
            imageCanvas.Composite(quotedAvatar, -50, 0, CompositeOperator.Over);
            imageCanvas.Composite(AZQuoteImage, 0, 0, CompositeOperator.Over);

            MagickReadSettings quoteNameSettings = new()
            {
                Font = "Lucida-Handwriting-Italic",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = 35,
                Width = 420,
                FillColor = MagickColors.White
            };
            MagickReadSettings quoteTextSettings = new()
            {
                Font = "placeholder",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = 245,
                Width = 485,
                FillColor = MagickColors.White,
            };
            MagickImage nameDrawn = new($"caption:{quotedName}", quoteNameSettings);
            imageCanvas.Composite(nameDrawn, 365, 260, CompositeOperator.Over);
            MagickImage quoteDrawn = new($"caption:{quote}", quoteTextSettings);
            imageCanvas.Composite(quoteDrawn, 335, 20, CompositeOperator.Over);

            return imageCanvas;
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

        private async Task<MagickImage> DownloadEmojiImage(DiscordEmoji inputEmoji)
        {
            try
            {
                byte[] emojiByteArray = await httpClient.GetByteArrayAsync(inputEmoji.Url);
                return new MagickImage(emojiByteArray);
            }
            catch (InvalidOperationException)
            {
                //check the json file for unicode emoji urls
                return null;
            }

        }

        private async Task SendImage(MagickImage attachment, DiscordMessage commandMessage, string message, bool jpeg = false)
        {
            string filePath;
            if (jpeg)
            {
                attachment.Format = MagickFormat.Jpeg;
                filePath = $"Output{Path.DirectorySeparatorChar}output_{DateTime.Now.ToFileTime()}.jpeg";
            }
            else
            {
                attachment.Format = MagickFormat.Png;
                filePath = $"Output{Path.DirectorySeparatorChar}output_{DateTime.Now.ToFileTime()}.png";
            }

            attachment.Write(filePath);

            FileStream outputFile = File.OpenRead(filePath);

            DiscordMessageBuilder replyMessage = new DiscordMessageBuilder()
                .WithContent(message)
                .WithFile(outputFile);

            await commandMessage.RespondAsync(replyMessage);
            await outputFile.DisposeAsync();
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