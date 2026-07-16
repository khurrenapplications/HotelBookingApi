using HotelBookingApi.Contracts;
using HotelBookingApi.Services;

namespace HotelBookingApi.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapGet("/{reference}", async (
                string reference,
                BookingService bookings,
                CancellationToken cancellationToken) =>
            {
                var booking = await bookings.FindBookingAsync(reference, cancellationToken);

                return booking is null ? Results.NotFound() : Results.Ok(booking);
            })
            .WithName("FindBookingByReference")
            .WithOpenApi();

        group.MapPost("/", async (
                BookingRequest request,
                BookingService bookings,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var booking = await bookings.BookRoomAsync(request, cancellationToken);

                    return Results.Created($"/api/bookings/{booking.Reference}", booking);
                }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
                catch (InvalidOperationException exception)
                {
                    // No matching room is a business conflict, not a malformed request.
                    return Results.Conflict(new { error = exception.Message });
                }
            })
            .WithName("BookRoom")
            .WithOpenApi();
    }
}
