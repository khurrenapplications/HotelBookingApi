using HotelBookingApi.Models;

namespace HotelBookingApi.Contracts;

public sealed record HotelResponse(int Id, string Name, int RoomCount);

public sealed record RoomAvailabilityResponse(
    int Id,
    string Number,
    RoomType RoomType,
    int Capacity);

public sealed record BookingRequest(
    int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int GuestCount,
    string GuestName,
    RoomType? PreferredRoomType);

public sealed record BookingResponse(
    string Reference,
    string HotelName,
    string RoomNumber,
    RoomType RoomType,
    int GuestCount,
    string GuestName,
    DateOnly CheckIn,
    DateOnly CheckOut);
