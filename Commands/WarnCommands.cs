using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DudesBot.Commands
{
    public class WarnCommands : BaseCommandModule
    {
        public IDbContextFactory<DudesDBContext> ContextFactory { private get; set; }

        [Command("warn"), Description("warns a user"), Cooldown(1, 30, CooldownBucketType.User)]
        public async Task CreateWarning(CommandContext context, [Description("User to warn")] DiscordMember victim, [RemainingText, Description("Reason for warning")] params string[] reason)
        {
            var BotDBContext = ContextFactory.CreateDbContext();
            string warnReason = "";
            foreach (string word in reason)
            {
                warnReason += word + " ";
            }
            if (warnReason == "") { return; }
            UserWarning newWarning = new()
            {
                UserId = victim.Id.ToString(),
                WarnReason = warnReason
            };
            var insertWarningTask = BotDBContext.UserWarning.AddAsync(newWarning);

            await context.RespondAsync(new DiscordMessageBuilder()
                .WithContent($"Warned <@{victim.Id}> with reason: {warnReason}")
                .WithAllowedMentions(Mentions.None));
            await insertWarningTask;
            BotDBContext.SaveChanges();
        }

        [Command("warns"), Aliases("warnings"), Description("Views the warnings of a user")]
        public async Task ViewWarning(CommandContext context, [Description("User to view warnings of")] DiscordMember victim)
        {
            var BotDBContext = ContextFactory.CreateDbContext();
            string userIDString = victim.Id.ToString();
            var result = BotDBContext.UserWarning.Where(warn => warn.UserId == userIDString);

            string warningsBody = "```\n";
            foreach (UserWarning warning in result)
            {
                warningsBody += $"Reason: {warning.WarnReason} \n";
            }
            warningsBody += "```";
            if (warningsBody == "```\n```")
            {
                await context.RespondAsync("User has no warnings");
                return;
            }
            DiscordEmbedBuilder warningEmbed = new();
            warningEmbed.WithTitle($"Warnings for {victim.Username}:{victim.Discriminator}");
            warningEmbed.WithDescription(warningsBody);
            await context.RespondAsync(warningEmbed);
        }
    }
}