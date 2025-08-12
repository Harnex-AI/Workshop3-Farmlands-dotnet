
using System.Net.Http.Json;
using Farmlands.Shared.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Farmlands.Tests;

public class ReservationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReservationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact(Skip="TODO: Implement minimal test once HttpInventoryClient is ready.")]
    public async Task CreateReservation_Succeeds_WhenStockIsEnough()
    {
        // Arrange
        var client = _factory.CreateClient();
        var req = new ReservationRequest("AKL", "SUPER-MIX-20KG", 1);

        // Act
        var res = await client.PostAsJsonAsync("/reservations", req);

        // Assert
        Assert.True(res.IsSuccessStatusCode);
    }
}
