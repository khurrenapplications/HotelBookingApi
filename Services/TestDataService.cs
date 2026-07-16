using HotelBookingApi.Data;
using HotelBookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Services;

public sealed class TestDataService(HotelBookingDbContext db)
{
    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        // Delete children first so this works consistently even if cascade settings change later.
        db.Bookings.RemoveRange(db.Bookings);
        db.Rooms.RemoveRange(db.Rooms);
        db.Hotels.RemoveRange(db.Hotels);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Seeding is idempotent; repeated calls should not duplicate the six-room hotel.
        if (await db.Hotels.AnyAsync(cancellationToken))
        {
            return;
        }

        // The brief requires six rooms across single, double, and deluxe room types.
        var hotel = new Hotel
        {
            Name = "Grand Azure Hotel",
            Rooms =
            [
                new Room { Number = "101", RoomType = RoomType.Single, Capacity = 1 },
                new Room { Number = "102", RoomType = RoomType.Single, Capacity = 1 },
                new Room { Number = "201", RoomType = RoomType.Double, Capacity = 2 },
                new Room { Number = "202", RoomType = RoomType.Double, Capacity = 2 },
                new Room { Number = "301", RoomType = RoomType.Deluxe, Capacity = 4 },
                new Room { Number = "302", RoomType = RoomType.Deluxe, Capacity = 4 }
            ]
        };

        db.Hotels.Add(hotel);
        await db.SaveChangesAsync(cancellationToken);
    }
}
