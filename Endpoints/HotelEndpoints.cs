using HotelBookingApi.Contracts;
using HotelBookingApi.Data;
using HotelBookingApi.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Endpoints;

public static class HotelEndpoints
{
    public static void MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hotels")
            .WithTags("Hotels");

        group.MapGet("/", async (
                string? name,
                HotelBookingDbContext db,
                CancellationToken cancellationToken) =>
            {
                var query = db.Hotels
                    .AsNoTracking()
                    .Include(hotel => hotel.Rooms)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    // Partial name matching keeps the hotel search friendly for API consumers.
                    query = query.Where(hotel => EF.Functions.Like(hotel.Name, $"%{name.Trim()}%"));
                }

                var hotels = await query
                    .OrderBy(hotel => hotel.Name)
                    .Select(hotel => new HotelResponse(hotel.Id, hotel.Name, hotel.Rooms.Count))
                    .ToListAsync(cancellationToken);

                return Results.Ok(hotels);
            })
            .WithName("FindHotels")
            .WithOpenApi();

        group.MapGet("/{hotelId:int}/available-rooms", async (
                int hotelId,
                DateOnly checkIn,
                DateOnly checkOut,
                int guests,
                BookingService bookings,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var rooms = await bookings.FindAvailableRoomsAsync(
                        hotelId,
                        checkIn,
                        checkOut,
                        guests,
                        cancellationToken);

                    return Results.Ok(rooms);
                }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
            })
            .WithName("FindAvailableRooms")
            .WithOpenApi();
    }
}
