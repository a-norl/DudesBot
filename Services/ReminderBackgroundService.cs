#pragma warning disable CS1998
#pragma warning disable CS4014
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DudesBot.Services
{
    public class ReminderBackgroundService
    {
        private DiscordClient discordClient;
        private List<Timer> ReminderList;

        public ReminderBackgroundService()
        {
            ReminderList = new();
        }

        public void AttachClient(DiscordClient client)
        {
            discordClient = client;
        }

        public async Task Start()
        {
            var DbContext = new BotDatabaseContext();
            try
            {
                foreach (var reminder in DbContext.ReminderObjects)
                {
                    try
                    {
                        ReminderList.Add(new Timer(new TimerCallback((_) => { SendReminder(reminder); }), null, reminder.Time - DateTime.UtcNow, Timeout.InfiniteTimeSpan));
                    }
                    catch
                    {
                        SendReminder(reminder);
                    }
                }
            }
            catch
            {
                DbContext.Database.ExecuteSqlRaw("DROP TABLE ReminderObjects");
                DbContext.SaveChanges();
                Console.WriteLine("cleared table");
            }


        }

        public async Task AddReminder(ReminderObject reminder)
        {
            var DbContext = new BotDatabaseContext();
            ReminderList.Add(new Timer(new TimerCallback((_) => { SendReminder(reminder); }), null, reminder.Time - DateTime.UtcNow, Timeout.InfiniteTimeSpan));
            await DbContext.ReminderObjects.AddAsync(reminder);
            await DbContext.SaveChangesAsync();
            Console.WriteLine("Reminder Saved");
        }

        private async Task SendReminder(ReminderObject reminder)
        {
            var DbContext = new BotDatabaseContext();
            string reminderMessage = $"<@{reminder.User_ID}> Reminder: {reminder.Message}";
            DiscordGuild guild = await discordClient.GetGuildAsync(ulong.Parse(reminder.Guild_ID));
            DiscordChannel channel = guild.GetChannel(ulong.Parse(reminder.Channel_ID));
            await channel.SendMessageAsync(reminderMessage);
            DbContext.ReminderObjects.Remove(reminder);
            await DbContext.SaveChangesAsync();
        }
    }
}