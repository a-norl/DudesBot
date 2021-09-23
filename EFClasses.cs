using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace DudesBot
{
    public class DudesDBContext : DbContext
    {
        public DbSet<UserWarning> UserWarning { get; set; }
        public DbSet<ReminderObject> ReminderObject { get; set; }
        public DbSet<CustomCommandObject> CustomCommandObjects { get; set; }

        public DudesDBContext(DbContextOptions options) : base(options)
        {
        }
    }

    public class UserWarning
    {

        public int Id { get; set; }
        public string UserId { get; set; }
        public string WarnReason { get; set; }
    }

    public class ReminderObject
    {
        [Key]
        public int Reminder_ID { get; init; }
        public string User_ID { get; init; }
        public string Guild_ID { get; init; }
        public string Channel_ID { get; init; }
        public DateTime Time { get; init; }
        public string Message { get; init; }
    }

    public class CustomCommandObject
    {
        [Key]
        public int CommandId { get; init; }
        public string CommandPhrase { get; init; }
        public string Response { get; init; }
        public ulong authorId { get; init; }
    }
}