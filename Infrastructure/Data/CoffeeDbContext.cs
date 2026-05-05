using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Data
{
    public class CoffeeDbContext : DbContext
    {
        public CoffeeDbContext(DbContextOptions<CoffeeDbContext> options) : base(options)
        {
        }
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ItemImages> ItemImages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<WorkingSchedule> WorkingSchedules { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User (System users only - Admin/Staff)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.PasswordSalt)
                    .IsRequired();

                entity.Property(u => u.PasswordVersion)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(u => u.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(u => u.CreatedDate)
                    .IsRequired();
            });

            // 2. Customer (Temporary customer data for orders)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email)
                    .HasMaxLength(200);

                entity.Property(c => c.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(c => c.CreatedDate)
                    .IsRequired();

                entity.Property(c => c.IsDataCleared)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasMany(c => c.Orders)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasIndex(o => o.OrderCode).IsUnique();

                entity.Property(o => o.OrderCode)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsFixedLength()
                    .HasColumnType("char(5)");

                entity.Property(o => o.CustomerId)
                    .IsRequired();

                entity.Property(o => o.FulfillmentScope)
                    .IsRequired();

                entity.Property(o => o.OutHouseFulfillmentType);

                entity.Property(o => o.TotalPrice)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(o => o.TotalItemsAmount)
                    .IsRequired();

                entity.Property(o => o.Version)
                    .IsRequired()
                    .HasDefaultValue(0)
                    .IsConcurrencyToken();

                entity.Property(o => o.CreatedDate)
                    .IsRequired();

                entity.Property(o => o.IsCompleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Order Items
            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.UnitPrice)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(oi => oi.Quantity)
                    .IsRequired();

                // Relationships configured in Order entity
            });

            // 4. Item
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(i => i.Price)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(i => i.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(i => i.CreatedDate)
                    .IsRequired();

                entity.Property(i => i.Version)
                    .IsRequired()
                    .HasDefaultValue(0)
                    .IsConcurrencyToken();

                // Indexing Name
                entity.HasIndex(i => i.Name).IsUnique();
            });

            // 5. Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.CreatedDate)
                    .IsRequired();

                entity.Property(c => c.Version)
                    .IsRequired()
                    .HasDefaultValue(0)
                    .IsConcurrencyToken();

                // Quan hệ 1-N: Một Category có nhiều Items
                entity.HasMany(c => c.Items)
                    .WithOne(i => i.Category)
                    .HasForeignKey(i => i.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Forbid to delete category if it has items
            });

            // 6. ItemImages
            modelBuilder.Entity<ItemImages>(entity =>
            {
                entity.HasKey(ii => ii.Id);

                entity.Property(ii => ii.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(ii => ii.ItemId)
                    .IsRequired();

                entity.Property(ii => ii.IsDefault)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(ii => ii.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(ii => ii.CreatedDate)
                    .IsRequired();

                // Relationship: One Item can have many ItemImages
                entity.HasOne(ii => ii.Item)
                    .WithMany(i => i.ItemImages)
                    .HasForeignKey(ii => ii.ItemId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete images when item is deleted
            });

            // 7. Reservation
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.CustomerName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(r => r.Email)
                    .HasMaxLength(100);

                entity.Property(r => r.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(r => r.ReservationTime)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(r => r.SpecialRequests)
                    .HasMaxLength(500);

                entity.Property(r => r.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(r => r.Version)
                    .IsRequired()
                    .HasDefaultValue(0)
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Where)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(n => n.WhatHappen)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(n => n.TargetUrl)
                    .HasMaxLength(500);

                entity.Property(n => n.CreatedDate)
                    .IsRequired();

                entity.HasMany(n => n.UserNotifications)
                    .WithOne(un => un.Notification)
                    .HasForeignKey(un => un.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(un => un.Id);

                entity.Property(un => un.IsRead)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasIndex(un => new { un.UserId, un.NotificationId })
                    .IsUnique();

                entity.HasOne(un => un.User)
                    .WithMany()
                    .HasForeignKey(un => un.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(s => s.Key);

                entity.Property(s => s.Key)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Value)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(s => s.UpdatedAt)
                    .IsRequired();

                // Seed default values so the system works on first run
                entity.HasData(
                    new { Key = "WorkingHours.Open",             Value = "08:00",                    UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "WorkingHours.Close",            Value = "22:00",                    UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "Contact.Email",                 Value = "",                         UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "Contact.Facebook",              Value = "",                         UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "Contact.Instagram",             Value = "",                         UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "Contact.Twitter",               Value = "",                         UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "Email.ConfirmationEnabled",     Value = "True",                     UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) },
                    new { Key = "Notification.ShowCount",        Value = "True",                     UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }
                );
            });

            modelBuilder.Entity<WorkingSchedule>(entity =>
            {
                entity.HasKey(w => w.Day);

                entity.Property(w => w.Day)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(w => w.OpenTime)
                    .IsRequired()
                    .HasConversion(
                        t => t.ToString("HH:mm"),
                        s => TimeOnly.Parse(s))
                    .HasMaxLength(5);

                entity.Property(w => w.CloseTime)
                    .IsRequired()
                    .HasConversion(
                        t => t.ToString("HH:mm"),
                        s => TimeOnly.Parse(s))
                    .HasMaxLength(5);

                entity.Property(w => w.IsClosed)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Seed all 7 days with sensible defaults
                entity.HasData(
                    new WorkingSchedule(DayOfWeek.Monday,    new TimeOnly(8,  0), new TimeOnly(22, 0)),
                    new WorkingSchedule(DayOfWeek.Tuesday,   new TimeOnly(8,  0), new TimeOnly(22, 0)),
                    new WorkingSchedule(DayOfWeek.Wednesday, new TimeOnly(8,  0), new TimeOnly(22, 0)),
                    new WorkingSchedule(DayOfWeek.Thursday,  new TimeOnly(8,  0), new TimeOnly(22, 0)),
                    new WorkingSchedule(DayOfWeek.Friday,    new TimeOnly(8,  0), new TimeOnly(22, 0)),
                    new WorkingSchedule(DayOfWeek.Saturday,  new TimeOnly(9,  0), new TimeOnly(21, 0)),
                    new WorkingSchedule(DayOfWeek.Sunday,    new TimeOnly(9,  0), new TimeOnly(20, 0))
                );
            });

            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Date).IsRequired().HasConversion<DateOnlyConverter>().HasColumnType("date");
                entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
                entity.Property(h => h.IsRecurring).IsRequired().HasDefaultValue(false);
                entity.Property(h => h.IsActive).IsRequired().HasDefaultValue(true);
                entity.HasIndex(h => new { h.Date, h.IsActive });
            });
        }
    }

    // EF Core 8 value converter for DateOnly ↔ DateTime (SQL date column)
    public class DateOnlyConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyConverter()
            : base(d => d.ToDateTime(TimeOnly.MinValue), dt => DateOnly.FromDateTime(dt)) { }
    }
}
