using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DudesBot.Services
{
    public class PinArchiveService : BaseCommandModule
    {
        private static readonly HttpClient httpClient = new();

        [Command("embed"), RequireOwner, Hidden]
        public async Task EmbedDebug(CommandContext ctx, DiscordMessage toEmbed)
        {
            DiscordMessageBuilder messageEmbed = await PinnedMessageEmbedder(toEmbed);
            await ctx.Channel.SendMessageAsync(messageEmbed);
        }

        public static async Task<DiscordMessageBuilder> PinnedMessageEmbedder(DiscordMessage message)
        {
            Task<DiscordMember> getMemberTask = message.Channel.Guild.GetMemberAsync(message.Author.Id);
            DiscordEmbedBuilder embedBuilder = new();
            DiscordMessageBuilder messageBuilder = new();
            IReadOnlyList<DiscordAttachment> attachments = message.Attachments;

            DiscordMember messageAuthor = await getMemberTask;
            string nickname = UtilityMethods.GetName(messageAuthor);
            string AvatarUrl = await UtilityMethods.GetProfilePictureURL(messageAuthor, httpClient);

            embedBuilder
                .WithAuthor(nickname, null, AvatarUrl);

            if (attachments.Count == 1)
            {
                embedBuilder.WithImageUrl(attachments[0].Url);
                embedBuilder
                    .WithDescription($"{message.Content} \n\n[Jump To Message]({message.JumpLink}) []({messageAuthor.Id}) ")
                    .WithTimestamp(message.Timestamp);
                messageBuilder.AddEmbed(embedBuilder);
            }
            else if (attachments.Count > 1)
            {
                embedBuilder.WithDescription($"{message.Content}");
                messageBuilder.AddEmbed(embedBuilder);
                foreach (DiscordAttachment multiAttachment in attachments)
                {
                    messageBuilder.AddEmbed(new DiscordEmbedBuilder().WithImageUrl(multiAttachment.Url));


                }
                messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"[Jump To Message]({message.JumpLink}) []({messageAuthor.Id}) ")
                    .WithTimestamp(message.Timestamp));
            }
            else
            {
                embedBuilder
                    .WithDescription($"{message.Content} \n\n[Jump To Message]({message.JumpLink}) []({messageAuthor.Id}) ")
                    .WithTimestamp(message.Timestamp);
                messageBuilder.AddEmbed(embedBuilder);
            }

            return messageBuilder;
        }

        public static async Task Archiver(MessageUpdateEventArgs eventArgs, ulong pinChannelId)
        {
            DiscordChannel PinChannel = eventArgs.Guild.GetChannel(pinChannelId);
            if (PinChannel is null) { return; }
            DiscordMessageBuilder archivedMessage = await PinnedMessageEmbedder(eventArgs.Message);

            var archiveTask = PinChannel.SendMessageAsync(archivedMessage);
            var pinList = await eventArgs.Message.Channel.GetPinnedMessagesAsync();
            if (pinList.Count > 47)
            {
                for (int i = 0; i < 4; i++)
                {
                    await pinList[pinList.Count - 1 - i].UnpinAsync();
                }

            }
            await archiveTask;
        }

    }
}