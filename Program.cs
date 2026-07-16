using System.Text.Json.Serialization;
using HotelBookingApi.Data;
using HotelBookingApi.Endpoints;
using HotelBookingApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelBookingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("HotelBookingDb")
        ?? "Server=localhost;Port=3306;Database=hotel_booking;User=root;Password=;";
    var mysqlServerVersion = builder.Configuration["Database:MySqlServerVersion"] ?? "8.0.36";

    // Pomelo needs the server version to generate SQL that matches the target MySQL instance.
    options.UseMySql(connectionString, ServerVersion.Parse(mysqlServerVersion));
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
    // Keep the sample easy to run: MySQL tables are created on startup if the database exists.
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapHotelEndpoints();
app.MapBookingEndpoints();
app.MapTestDataEndpoints();

app.Run();
