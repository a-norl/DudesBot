using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;

namespace DudesBot.Commands
{
    public class ImageCommands : BaseCommandModule
    {
        private const int CooldownTime = 5;
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
        private static readonly MagickImage JarImage;
        private static readonly string ThisFilm = "This Image is Dedicated to the Brave Mujahideen Fighters of Afghanistan";

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
            JarImage = new($"Resources{Path.DirectorySeparatorChar}jar.png");
        }

        [Command("imagetestmagick"), Description("a test command, don't expect much from this")]
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

        [Command("imagetest4"), Description("a test command, don't expect much from this")]
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

        [Command("impact"), Cooldown(1, CooldownTime, CooldownBucketType.User), Description("Adds impact text to the top and bottom of the image")]
        public async Task ImpactCommand(CommandContext context, [Description("The top text of the image")] string topText, [Description("The bottom text of the image")] string bottomText)
        {
            await context.TriggerTypingAsync();
            MagickImage inputImage = await DownloadAttachment(context.Message);
            MagickReadSettings textSettings = new()
            {
                Font = $"Resources{Path.DirectorySeparatorChar}impact.ttf",
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

        [Command("kill"), Cooldown(1, CooldownTime, CooldownBucketType.User), Description("commit murder")]
        public async Task KillCommand(CommandContext context, [Description("the damned")] DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            var backgroundImage = GrabHandsImage.Clone();
            victimAvatar.Resize(490, 490);
            backgroundImage.RotationalBlur(5.0);
            backgroundImage.Composite(victimAvatar, 420, 240, CompositeOperator.Over);
            await SendImage((MagickImage)backgroundImage, context.Message, $"{UtilityMethods.GetName(victimMember)} has been killed");
        }

        [Command("gifkill"), Cooldown(1, CooldownTime, CooldownBucketType.User), Description("commit murder (but animated)")]
        public async Task GIFKillCommand(CommandContext context, [Description("the damned")] DiscordMember victimMember)
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

        [Command("inflate"), Priority(1), Cooldown(1, CooldownTime, CooldownBucketType.User), Description("inflates a user big and round")] //Inflate User
        public async Task InflateBigAndRound(CommandContext context, [Description("to be inflated....")] DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            var BigAndRoundGif = ImageInflator(victimAvatar);
            await SendImage(BigAndRoundGif, context.Message, $"{UtilityMethods.GetName(victimMember)} has been inflated big and round");
        }

        [Command("inflate"), Priority(0), Cooldown(1, CooldownTime, CooldownBucketType.User),] //Inflate Attachment
        public async Task InflateAttachment(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage inflateTarget = await DownloadAttachment(context.Message);

            var BigAndRoundGif = ImageInflator(inflateTarget);
            await SendImage(BigAndRoundGif, context.Message, $" ");
        }

        private MagickImageCollection ImageInflator(MagickImage inflateTarget)
        {
            MagickImageCollection BigAndRoundGif = new();
            inflateTarget.Resize(250, 250);
            for (float i = 0; i <= 0.8; i += 0.05f)
            {
                IMagickImage<byte> tempImage = inflateTarget.Clone();
                tempImage.Distort(DistortMethod.Barrel, i, 0.0, 0.0);
                tempImage.AnimationDelay = 10;
                tempImage.GifDisposeMethod = GifDisposeMethod.Background;
                BigAndRoundGif.Add(tempImage);
            }
            BigAndRoundGif.Last().AnimationDelay = 100;
            return BigAndRoundGif;
        }

        [Command("deflate"), Cooldown(1, CooldownTime, CooldownBucketType.User), Priority(1)]
        public async Task DeflateSmallAndRound(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            MagickImage victimAvatar = await DownloadAvatar(victimMember);
            var BigAndRoundGif = ImageDeflator(victimAvatar);
            await SendImage(BigAndRoundGif, context.Message, $"{UtilityMethods.GetName(victimMember)} has been deflated small and (not?) round");
        }

        [Command("deflate"), Cooldown(1, CooldownTime, CooldownBucketType.User), Priority(0)]
        public async Task DeflateAttachmentSmallAndRound(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage deflateTarget = await DownloadAttachment(context.Message);

            var BigAndRoundGif = ImageDeflator(deflateTarget);
            await SendImage(BigAndRoundGif, context.Message, $" ");
        }

        private MagickImageCollection ImageDeflator(MagickImage inflateTarget)
        {
            MagickImageCollection BigAndRoundGif = new();
            inflateTarget.Resize(250, 250);
            for (float i = 0; i <= 0.8; i += 0.05f)
            {
                IMagickImage<byte> tempImage = inflateTarget.Clone();
                tempImage.Distort(DistortMethod.BarrelInverse, i, 0.0, 0.0);
                tempImage.AnimationDelay = 10;
                tempImage.GifDisposeMethod = GifDisposeMethod.Background;
                BigAndRoundGif.Add(tempImage);
            }
            BigAndRoundGif.Last().AnimationDelay = 100;
            return BigAndRoundGif;
        }

        [Command("squish"), Cooldown(1, CooldownTime, CooldownBucketType.User), Priority(1)]
        public async Task SquishUser(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            var downloadAvatarTask = DownloadAvatar(victimMember);

            var killGIF = ImageSquisher(await downloadAvatarTask);
            await SendImage(killGIF, context.Message, "");
        }

        [Command("squish"), Cooldown(1, CooldownTime, CooldownBucketType.User), Priority(0)]
        public async Task SquishAttachment(CommandContext context)
        {
            await context.TriggerTypingAsync();

            var attachment = await DownloadAttachment(context.Message);

            var killGIF = ImageSquisher(attachment);
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

        [Command("cowboy"), Cooldown(1, CooldownTime, CooldownBucketType.User)]
        public async Task EmojiCowboy(CommandContext context, DiscordEmoji inputEmoji)
        {
            await context.TriggerTypingAsync();
            MagickImage emojiImage = await DownloadEmojiImage(inputEmoji);

            emojiImage.Resize(CowboyHat.Width, 0);
            MagickImage imageCanvas = new(MagickColors.Transparent, emojiImage.Width, emojiImage.Height + (CowboyHat.Height / 4));
            imageCanvas.Composite(emojiImage, 0, (CowboyHat.Height / 4), CompositeOperator.Over);
            imageCanvas.Composite(CowboyHat, 0, 0, CompositeOperator.Over);
            await SendImage(imageCanvas, context.Message, "");
        }

        [Command("wojak"), Aliases("wojack"), Cooldown(1, CooldownTime, CooldownBucketType.User)]
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

        [Command("jpeg"), Aliases("jpg"), Cooldown(1, CooldownTime, CooldownBucketType.User)]
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

        [Command("quote"), Priority(1)]
        public async Task QuoteCommand(CommandContext context, DiscordMember quotedMember, [RemainingText] string quote)
        {
            await context.TriggerTypingAsync();

            MagickImage quoteImage = await QuoteGenerator(quotedMember, quote);

            await SendImage(quoteImage, context.Message, " ");
        }

        [Command("quote"), Priority(2)]
        public async Task QuoteMessageCommand(CommandContext context, DiscordMessage inputMessage)
        {
            // await context.TriggerTypingAsync();

            var quotedMember = await context.Guild.GetMemberAsync(inputMessage.Author.Id);
            Task<MagickImage> quoteImageTask = QuoteGenerator(quotedMember, inputMessage.Content);
            MagickImage quoteImage;
            do
            {
                await context.TriggerTypingAsync();
            }
            while (!quoteImageTask.IsCompleted);
            quoteImage = await quoteImageTask;

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
            quote = Regex.Replace(quote, pattern, replacement);
            quote = Regex.Replace(quote, "<(.*?)>", "");

            var imageCanvas = new MagickImage(MagickColors.Transparent, AZQuoteImage.Width, AZQuoteImage.Height);
            MagickImage quotedAvatar = await getAvatarTask;
            quotedAvatar.AdaptiveResize(400, 400);
            imageCanvas.Composite(quotedAvatar, -50, 0, CompositeOperator.Over);
            imageCanvas.Composite(AZQuoteImage, 0, 0, CompositeOperator.Over);

            MagickReadSettings quoteNameSettings = new()
            {
                Font = $"Resources{Path.DirectorySeparatorChar}LHANDW.TTF",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = 35,
                Width = 420,
                FillColor = MagickColors.White
            };
            MagickReadSettings quoteTextSettings = new()
            {
                Font = $"Resources{Path.DirectorySeparatorChar}GARA.TTF",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = 245,
                Width = 485,
                FillColor = MagickColors.White,
            };
            MagickImage nameDrawn = new($"caption:{quotedName}", quoteNameSettings);
            imageCanvas.Composite(nameDrawn, 365, 300, CompositeOperator.Over);
            MagickImage quoteDrawn = new($"caption:{quote}", quoteTextSettings);
            imageCanvas.Composite(quoteDrawn, 335, 20, CompositeOperator.Over);

            return imageCanvas;
        }

        [Command("text"), Hidden]
        public async Task TextTestCommand(CommandContext context, [RemainingText] string input)
        {
            await context.TriggerTypingAsync();
            var imageCanvas = new MagickImage(MagickColors.Black, 500, 500);

            MagickReadSettings quoteTextSettings = new()
            {
                Font = $"Garamond",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                // Height = 500,
                // Width = 500,
                FillColor = MagickColors.White,
            };
            MagickImage quoteDrawn = new($"pango:{input}", quoteTextSettings);
            imageCanvas.Composite(quoteDrawn, 0, 0, CompositeOperator.Over);

            await SendImage(imageCanvas, context.Message, "");
        }

        [Command("brave"), Cooldown(1, CooldownTime, CooldownBucketType.User)]
        public async Task BraveMujahideenFighters(CommandContext context)
        {
            await context.TriggerTypingAsync();
            MagickImage imageInput = await DownloadAttachment(context.Message);
            MagickReadSettings textSettings = new()
            {
                Font = $"Resources{Path.DirectorySeparatorChar}Basaro.ttf",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                Height = imageInput.Height / 2,
                Width = imageInput.Width - (imageInput.Width / 4),
                FillColor = MagickColors.White,
            };
            MagickImage textDrawn = new($"caption:{ThisFilm}", textSettings);
            var shadow = textDrawn.Clone();
            shadow.Shadow(10, -10, 1, new Percentage(50));
            imageInput.Composite(shadow, imageInput.Width / 8, imageInput.Height / 4, CompositeOperator.Over);
            imageInput.Composite(textDrawn, imageInput.Width / 8, imageInput.Height / 4, CompositeOperator.Over);


            await SendImage(imageInput, context.Message, "");
        }

        [Command("jar"), Cooldown(1, CooldownTime, CooldownBucketType.User), Priority(0)]
        public async Task JarAttachment(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var inputImage = await DownloadAttachment(context.Message);
            var outputImage = PutInJar(inputImage);
            await SendImage(outputImage, context.Message, $"");
        }

        [Command("jar"), Cooldown(1, CooldownTime, CooldownBucketType.User), Priority(1), DisallowUser(281938559416664064)]
        public async Task JarUser(CommandContext context, DiscordMember victimMember)
        {
            await context.TriggerTypingAsync();
            var avatarImage = await DownloadAvatar(victimMember);
            var outputImage = PutInJar(avatarImage);
            await SendImage(outputImage, context.Message, $"{UtilityMethods.GetName(victimMember)} has been trapped in the jar");
        }

        private MagickImage PutInJar(MagickImage victimPhoto)
        {
            var imageCanvas = new MagickImage(MagickColors.Transparent, JarImage.Width, JarImage.Height);
            victimPhoto.Resize(450, 450);
            imageCanvas.Composite(victimPhoto, 100, 320, CompositeOperator.Over);
            imageCanvas.Composite(JarImage, 0, 0, CompositeOperator.Over);

            return imageCanvas;
        }

        [Command("mousetrap")]
        public async Task MousetrapCommand(CommandContext context)
        {
            var mousetrapEmote = DiscordEmoji.FromName(context.Client, ":mouse_trap:");
            var buttonMessage = new DiscordMessageBuilder()
            {
                Content = "press me :]"
            };
            buttonMessage.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "mousetrap_button", null, false, new DiscordComponentEmoji(mousetrapEmote)));
            var deleteTask = context.Message.DeleteAsync();
            await context.Channel.SendMessageAsync(buttonMessage);
            await deleteTask;
        }

        public static async Task MouseTrapImage(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs, bool deleteOriginal = true)
        {
            var interaction = eventArgs.Interaction;
            await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            if (deleteOriginal) { await interaction.DeleteOriginalResponseAsync(); }
            await eventArgs.Channel.TriggerTypingAsync();

            var httpClient = client.GetCommandsNext().Services.GetService<HttpClient>();
            var victimMember = (DiscordMember)interaction.User;
            MagickImage victimAvatar;

            var getAvatarTask = httpClient.GetByteArrayAsync(victimMember.AvatarUrl);
            var getGuildAvatarTask = httpClient.GetByteArrayAsync(victimMember.GuildAvatarUrl);
            try
            {
                byte[] guildAvatarByteArray = await getGuildAvatarTask;
                victimAvatar = new MagickImage(guildAvatarByteArray);
            }
            catch
            {
                byte[] avatarByteArray = await getAvatarTask;
                victimAvatar = new MagickImage(avatarByteArray);
            }
            victimAvatar.Resize(190, 190);
            var imageCanvas = new MagickImage(MagickColors.White, BoxImage.Width, BoxImage.Height);
            imageCanvas.Composite(victimAvatar, 225, 260, CompositeOperator.Over);
            imageCanvas.Composite(BoxImage, 0, 0, CompositeOperator.Over);

            await SendImage(imageCanvas, eventArgs.Message, $"{UtilityMethods.GetName(victimMember)} has been trapped");
        }

        private async Task<MagickImage> DownloadAttachment(DiscordMessage targetMessage)
        {
            IReadOnlyList<DiscordAttachment> attachmentList = targetMessage.Attachments;
            DiscordAttachment attachment;
            if (attachmentList.Count == 0)
            {
                if (targetMessage.ReferencedMessage is not null)
                {
                    if (targetMessage.ReferencedMessage.Attachments.Count != 0)
                    {
                        attachment = targetMessage.ReferencedMessage.Attachments[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                attachment = attachmentList[0];
            }

            try
            {
                byte[] attachmentByteArray = await httpClient.GetByteArrayAsync(attachment.Url);
                MagickImageCollection downloadedRaw = new(attachmentByteArray);
                MagickImage downloadedImage = (MagickImage)downloadedRaw.Last();
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

        public static async Task SendImage(MagickImage attachment, DiscordMessage commandMessage, string message, bool jpeg = false)
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

        public static async Task SendImage(MagickImageCollection attachment, DiscordMessage commandMessage, string message) //Send gif
        {
            attachment[0].AnimationIterations = 0;
            string filePath = $"Output{Path.DirectorySeparatorChar}output_{DateTime.Now.ToFileTime()}.gif";
            attachment.Write(filePath);

            FileStream outputFile = File.OpenRead(filePath);

            DiscordMessageBuilder replyMessage = new DiscordMessageBuilder()
                .WithContent($"{message}") //and with file size {Math.Round(outputFile.Length/1048576.0, 2)} mb
                .WithFile(outputFile);

            await commandMessage.RespondAsync(replyMessage);
            outputFile.Dispose();
            File.Delete(filePath);
        }
    }
}