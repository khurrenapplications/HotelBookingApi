using System.Text.Json.Serialization;
using HotelBookingApi.Data;
using HotelBookingApi.Endpoints;
using HotelBookingApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure the database context using your SQL Server connection string
builder.Services.AddDbContext<HotelBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HotelBookingDb")));

builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<TestDataService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// 2. Standard ASP.NET Core Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Automatically create your SQL database tables on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// 4. Force Swagger to load on both local and production environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelBookingApi v1");
    options.RoutePrefix = string.Empty; // This makes Swagger load at the absolute root URL
});

app.MapHotelEndpoints();
app.MapBookingEndpoints();
app.MapTestDataEndpoints();

app.Run();
