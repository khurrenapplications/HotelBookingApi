using HotelBookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Data;

public sealed class HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options)
    : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasIndex(hotel => hotel.Name).IsUnique();
            entity.Property(hotel => hotel.Name).HasMaxLength(160);
            // Resetting a hotel should remove its rooms and bookings in one predictable operation.
            entity.HasMany(hotel => hotel.Rooms)
                .WithOne(room => room.Hotel)
                .HasForeignKey(room => room.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(room => room.Number).HasMaxLength(20);
            // Store enum names so database rows remain readable during manual testing.
            entity.Property(room => room.RoomType).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(room => new { room.HotelId, room.Number }).IsUnique();
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            // Booking references are the public lookup key, so enforce uniqueness in MySQL too.
            entity.HasIndex(booking => booking.Reference).IsUnique();
            entity.Property(booking => booking.Reference).HasMaxLength(20);
            entity.Property(booking => booking.GuestName).HasMaxLength(160);
            entity.HasOne(booking => booking.Room)
                .WithMany(room => room.Bookings)
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
