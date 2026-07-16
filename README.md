# Hotel Booking API

ASP.NET Core Web API for hotel room search and booking using EF Core with MySQL.

## Business Rules Covered

- A hotel has six rooms.
- Room types are `Single`, `Double`, and `Deluxe`.
- Seed data creates two rooms of each type.
- A room cannot be booked twice for overlapping nights.
- A booking is always assigned to one room for the full stay, so guests never need to change rooms.
- Booking references are unique.
- A booking cannot exceed room capacity.

## Run Locally

```bash
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS hotel_booking;"
dotnet restore
dotnet run --project HotelBookingApi
```

Open Swagger at the URL printed by `dotnet run`, followed by `/swagger`.

The API uses MySQL and creates the required tables automatically on startup. Update the `HotelBookingDb` connection string in `appsettings.json`, environment variables, or user secrets if your MySQL user, password, host, or database name is different.

Example environment-variable override:

```bash
ConnectionStrings__HotelBookingDb="Server=localhost;Port=3306;Database=hotel_booking;User=root;Password=your_password;" dotnet run --project HotelBookingApi
```

The configured MySQL server version defaults to `8.0.36` and can be changed with `Database:MySqlServerVersion`.

## Useful Endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/test-data/reset` | Removes hotels, rooms, and bookings. |
| `POST` | `/api/test-data/seed` | Adds one hotel, `Grand Azure Hotel`, with six rooms. |
| `GET` | `/api/hotels?name=azure` | Finds hotels by name. |
| `GET` | `/api/hotels/{hotelId}/available-rooms?checkIn=2026-08-01&checkOut=2026-08-03&guests=2` | Finds rooms available for the full stay. |
| `POST` | `/api/bookings` | Books the best matching available room. |
| `GET` | `/api/bookings/{reference}` | Finds booking details by booking reference. |

## Example Booking Request

```json
{
  "hotelId": 1,
  "checkIn": "2026-08-01",
  "checkOut": "2026-08-03",
  "guestCount": 2,
  "guestName": "Ada Lovelace",
  "preferredRoomType": "Double"
}
```

`preferredRoomType` is optional. Valid values are `Single`, `Double`, and `Deluxe`.

## Notes

- Date ranges are treated as hotel nights: check-in is inclusive and check-out is exclusive.
- Back-to-back stays are allowed. For example, one booking checking out on `2026-08-03` does not block another checking in on `2026-08-03`.
- Authentication is intentionally not configured, per the brief.
