using System.Data;
using HotelBookingApi.Contracts;
using HotelBookingApi.Data;
using HotelBookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Services;

public sealed class BookingService(HotelBookingDbContext db)
{
    public async Task<IReadOnlyList<RoomAvailabilityResponse>> FindAvailableRoomsAsync(
        int hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestCount,
        CancellationToken cancellationToken)
    {
        ValidateStay(checkIn, checkOut, guestCount);

        return await AvailableRoomsQuery(hotelId, checkIn, checkOut, guestCount)
            .OrderBy(room => room.Capacity)
            .ThenBy(room => room.Number)
            .Select(room => new RoomAvailabilityResponse(
                room.Id,
                room.Number,
                room.RoomType,
                room.Capacity))
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingResponse?> FindBookingAsync(string reference, CancellationToken cancellationToken)
    {
        return await db.Bookings
            .AsNoTracking()
            .Include(booking => booking.Room)
            .ThenInclude(room => room!.Hotel)
            .Where(booking => booking.Reference == reference)
            .Select(booking => new BookingResponse(
                booking.Reference,
                booking.Room!.Hotel!.Name,
                booking.Room.Number,
                booking.Room.RoomType,
                booking.GuestCount,
                booking.GuestName,
                booking.CheckIn,
                booking.CheckOut))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<BookingResponse> BookRoomAsync(BookingRequest request, CancellationToken cancellationToken)
    {
        ValidateStay(request.CheckIn, request.CheckOut, request.GuestCount);

        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            throw new ArgumentException("Guest name is required.", nameof(request));
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        // The room is selected and booked inside one transaction to reduce race windows.
        var room = await AvailableRoomsQuery(
                request.HotelId,
                request.CheckIn,
                request.CheckOut,
                request.GuestCount)
            .Where(room => request.PreferredRoomType == null || room.RoomType == request.PreferredRoomType)
            // Prefer the smallest suitable room so larger rooms remain available for larger parties.
            .OrderBy(room => room.Capacity)
            .ThenBy(room => room.Number)
            .FirstOrDefaultAsync(cancellationToken);

        if (room is null)
        {
            throw new InvalidOperationException("No room is available for the requested stay and guest count.");
        }

        var booking = new Booking
        {
            Reference = GenerateReference(),
            RoomId = room.Id,
            GuestName = request.GuestName.Trim(),
            GuestCount = request.GuestCount,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new BookingResponse(
            booking.Reference,
            room.Hotel!.Name,
            room.Number,
            room.RoomType,
            booking.GuestCount,
            booking.GuestName,
            booking.CheckIn,
            booking.CheckOut);
    }

    private IQueryable<Room> AvailableRoomsQuery(
        int hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestCount)
    {
        return db.Rooms
            .Include(room => room.Hotel)
            .Where(room => room.HotelId == hotelId)
            .Where(room => room.Capacity >= guestCount)
            // Date ranges are [check-in, check-out), so back-to-back stays do not overlap.
            .Where(room => !room.Bookings.Any(booking =>
                booking.CheckIn < checkOut && checkIn < booking.CheckOut));
    }

    private static void ValidateStay(DateOnly checkIn, DateOnly checkOut, int guestCount)
    {
        if (checkOut <= checkIn)
        {
            throw new ArgumentException("Check-out must be after check-in.");
        }

        if (guestCount <= 0)
        {
            throw new ArgumentException("Guest count must be greater than zero.");
        }
    }

    private static string GenerateReference()
    {
        // Short, human-readable references are backed by a unique database index.
        return $"HB-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
    }
}
