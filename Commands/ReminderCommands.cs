using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DudesBot.Services;
using Microsoft.EntityFrameworkCore;

namespace DudesBot.Commands
{
    public class ReminderCommands : BaseCommandModule
    {
        public IDbContextFactory<DudesDBContext> ContextFactory { private get; set; }
        public ReminderBackgroundService reminderService { private get; set; }

        [Command("reminder"), Aliases("remindme", "setreminder")]
        public async Task SetReminder(CommandContext context, TimeSpan timeOffset, [RemainingText] params string[] messageArray)
        {
            string message = "";
            DateTime dueTime = DateTime.UtcNow + timeOffset;
            foreach (string word in messageArray)
            {
                message += word + " ";
            }
            await reminderService.AddReminder(new ReminderObject()
            {
                Message = message,
                User_ID = context.User.Id.ToString(),
                Guild_ID = context.Guild.Id.ToString(),
                Channel_ID = context.Channel.Id.ToString(),
                Time = dueTime,
            });
            await context.RespondAsync($"Reminder set {Formatter.Timestamp(timeOffset, TimestampFormat.ShortDateTime)}: {message}");
        }

        [Command("reminders")]
        public async Task ListSelfReminders(CommandContext context)
        {
            var dbContext  = ContextFactory.CreateDbContext();
            string userIDString = context.User.Id.ToString();
            string reminderListString = "Your Reminders:\n";
            foreach(ReminderObject reminder in dbContext.ReminderObject)
            {
                if(reminder.User_ID == userIDString)
                {
                    reminderListString += $"<#{reminder.Channel_ID}> {Formatter.Timestamp(reminder.Time, TimestampFormat.RelativeTime)} ({Formatter.Timestamp(reminder.Time, TimestampFormat.ShortDateTime)}): {reminder.Message}\n";
                }
            }
            var interactivity = context.Client.GetInteractivity();
            var pages = interactivity.GeneratePagesInEmbed(reminderListString, DSharpPlus.Interactivity.Enums.SplitType.Line);
            await context.Channel.SendPaginatedMessageAsync(context.Member, pages);
        }

        [Command("remindersall"), Hidden, RequireOwner]
        public async Task ListAllReminders(CommandContext context)
        {
            var dbContext  = ContextFactory.CreateDbContext();
            string reminderListString = "Your Reminders:\n";
            foreach(ReminderObject reminder in dbContext.ReminderObject)
            {
                reminderListString += $"<#{reminder.Channel_ID}> {Formatter.Timestamp(reminder.Time, TimestampFormat.RelativeTime)} ({Formatter.Timestamp(reminder.Time, TimestampFormat.ShortDateTime)}): {reminder.Message}\n";
            }
            var interactivity = context.Client.GetInteractivity();
            var pages = interactivity.GeneratePagesInEmbed(reminderListString, DSharpPlus.Interactivity.Enums.SplitType.Line);
            await context.Channel.SendPaginatedMessageAsync(context.Member, pages);
        }

    }
}