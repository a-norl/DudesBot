using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using VideoLibrary;

namespace DudesBot.Commands
{
    public class ImageMusicCommand : BaseCommandModule
    {
        public VideoClient videoDownloader {private get; set;}
        private YouTube youTubeCrawler = YouTube.Default;

        [Command("addmusic"), Hidden]
        public async Task ImageMusicAdder(CommandContext context, [RemainingText] string youtubeURL)
        {
            var video = await youTubeCrawler.GetVideoAsync(youtubeURL);
            if(video.Info.LengthSeconds > 300)
            {
                await context.RespondAsync("video too long. die.");
                return;
            }

            var videoStream = await videoDownloader.StreamAsync(video);
            await context.RespondAsync(new DiscordMessageBuilder().WithFile("output.mp4", videoStream).WithContent($"{video.Title}: {video.Info.LengthSeconds}"));
        }
    }
}