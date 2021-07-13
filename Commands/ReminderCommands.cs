using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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
            await context.RespondAsync($"Reminder set for {dueTime.ToLongTimeString()} on {dueTime.ToLongDateString()} UTC: {message}");
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
                    reminderListString += $"<#{reminder.Channel_ID}> at {reminder.Time.ToLongTimeString()} on {reminder.Time.ToLongDateString()} UTC: {reminder.Message}\n";
                }
            }
            await context.RespondAsync(reminderListString);
        }

    }
}