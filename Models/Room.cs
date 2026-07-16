namespace HotelBookingApi.Models;

public sealed class Room
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public Hotel? Hotel { get; set; }

    public required string Number { get; set; }

    public RoomType RoomType { get; set; }

    public int Capacity { get; set; }

    public List<Booking> Bookings { get; set; } = [];
}
