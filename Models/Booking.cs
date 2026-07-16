namespace HotelBookingApi.Models;

public sealed class Booking
{
    public int Id { get; set; }

    public required string Reference { get; set; }

    public int RoomId { get; set; }

    public Room? Room { get; set; }

    public required string GuestName { get; set; }

    public int GuestCount { get; set; }

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
