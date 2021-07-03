using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace TestBankGuaranteeAPI.DatabaseModels
{
    public partial class BGDatabaseContext : DbContext
    {
        public BGDatabaseContext()
        {
        }

        public BGDatabaseContext(DbContextOptions<BGDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<TelegramUserData> TelegramUserData { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Number).HasMaxLength(30);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<TelegramUserData>(entity =>
            {
                entity.HasKey(e => e.TelegramId)
                    .HasName("PK__tmp_ms_x__0EAF6374F4B7C50F");

                entity.Property(e => e.TelegramId).ValueGeneratedNever();

                entity.Property(e => e.BeginDate).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.GuaranteeType).HasMaxLength(50);

                entity.Property(e => e.Link).HasMaxLength(1000);

                entity.Property(e => e.NotificationNumber).HasMaxLength(50);

                entity.Property(e => e.Stage).HasMaxLength(50);

                entity.Property(e => e.Sum).HasColumnType("money");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.CompanyName).HasMaxLength(500);

                entity.Property(e => e.Inn)
                    .HasMaxLength(20)
                    .HasColumnName("INN");

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
