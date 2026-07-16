using System.Text.Json.Serialization;
using HotelBookingApi.Data;
using HotelBookingApi.Endpoints;
using HotelBookingApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelBookingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("HotelBookingDb")
        ?? "Data Source=hotel-booking.db";

    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<TestDataService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapHotelEndpoints();
app.MapBookingEndpoints();
app.MapTestDataEndpoints();

app.Run();
