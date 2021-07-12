using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DudesBot
{
    public class DudesDBContext : DbContext
    {
        public DbSet<UserWarning> UserWarning {get; set;}
        public DbSet<ReminderObject> ReminderObject {get;set;}

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
        public int ReminderId { get; init; }
        public string UserId { get; init; }
        public string GuildId { get; init; }
        public string ChannelId { get; init; }
        public DateTime Time { get; init; }
        public string Message { get; init; }
    }
}