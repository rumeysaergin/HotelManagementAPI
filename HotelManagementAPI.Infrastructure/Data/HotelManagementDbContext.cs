using HotelManagementAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace HotelManagementAPI.Infrastructure.Data
{
    public class HotelManagementDbContext : DbContext
    {
        public HotelManagementDbContext(
            DbContextOptions<HotelManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Hotel> Hotels { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>()
                .Property(x => x.PricePerNight)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(x => x.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(x => x.CancellationFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);
        }
    }
}