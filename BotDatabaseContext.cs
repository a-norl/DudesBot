using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace DudesBot
{
    public partial class BotDatabaseContext : DbContext
    {
        public BotDatabaseContext()
        {
        }

        public BotDatabaseContext(DbContextOptions<BotDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ReminderObject> ReminderObjects { get; set; }
        public virtual DbSet<UserWarning> UserWarnings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=BotDatabase.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReminderObject>(entity =>
            {
                entity.HasKey(e => e.Reminder_ID);

                entity.ToTable("ReminderObject");

                entity.Property(e => e.Reminder_ID)
                    .HasColumnType("integer")
                    .HasColumnName("Reminder_ID");

                entity.Property(e => e.Channel_ID)
                    .HasColumnType("varchar")
                    .HasColumnName("Channel_ID");

                entity.Property(e => e.Guild_ID)
                    .HasColumnType("varchar")
                    .HasColumnName("Guild_ID");

                entity.Property(e => e.Message).HasColumnType("varchar");

                entity.Property(e => e.Time).HasColumnType("bigint");

                entity.Property(e => e.User_ID)
                    .HasColumnType("varchar")
                    .HasColumnName("User_ID");
            });

            modelBuilder.Entity<UserWarning>(entity =>
            {
                entity.ToTable("UserWarning");

                entity.Property(e => e.Id).HasColumnType("integer");

                entity.Property(e => e.UserId)
                    .HasColumnType("varchar")
                    .HasColumnName("UserID");

                entity.Property(e => e.WarnReason).HasColumnType("varchar");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
