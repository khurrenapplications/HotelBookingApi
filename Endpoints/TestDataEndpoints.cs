using HotelBookingApi.Services;

namespace HotelBookingApi.Endpoints;

public static class TestDataEndpoints
{
    public static void MapTestDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/test-data")
            .WithTags("Test data");

        group.MapPost("/seed", async (
                TestDataService testData,
                CancellationToken cancellationToken) =>
            {
                await testData.SeedAsync(cancellationToken);

                return Results.Ok(new { message = "Seed data is ready." });
            })
            .WithName("SeedTestData")
            .WithOpenApi();

        group.MapPost("/reset", async (
                TestDataService testData,
                CancellationToken cancellationToken) =>
            {
                await testData.ResetAsync(cancellationToken);

                return Results.NoContent();
            })
            .WithName("ResetTestData")
            .WithOpenApi();
    }
}
