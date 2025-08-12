
using Farmlands.Shared.Contracts;
using Farmlands.Shared.Messaging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Farmlands Reservations API", Version = "v1" });
});

// TODO: Replace FakeInventoryClient with HttpInventoryClient that calls Inventory API via HttpClient.
// Use env var INVENTORY_URL (default http://localhost:5081).
builder.Services.AddSingleton<IInventoryClient, FakeInventoryClient>();

// TODO: Add Idempotency middleware based on 'Idempotency-Key' header.
builder.Services.AddSingleton<IMessageBus, ConsoleMessageBus>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/reservations", async (ReservationRequest req, IInventoryClient inv, IMessageBus bus, CancellationToken ct) =>
{
    // 1) Check stock
    var snapshot = await inv.GetAsync(req.StoreId, req.Sku, ct);
    if (snapshot.Quantity < req.Quantity)
    {
        return Results.BadRequest(new { error = "Insufficient stock" });
    }

    // 2) Decrement stock
    var updated = await inv.AdjustAsync(req.StoreId, req.Sku, -req.Quantity, ct);

    // 3) Emit replenish event if below threshold
    const int threshold = 5;
    if (updated.Quantity < threshold)
    {
        await bus.PublishAsync(new ReplenishRequested(req.StoreId, req.Sku, updated.Quantity, threshold), ct);
    }

    // 4) Return reservation
    return Results.Created($"/reservations/{Guid.NewGuid()}", new ReservationResponse(Guid.NewGuid(), "Created"));
})
.WithName("CreateReservation");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public interface IInventoryClient
{
    Task<InventoryQueryResponse> GetAsync(string storeId, string sku, CancellationToken ct = default);
    Task<InventoryQueryResponse> AdjustAsync(string storeId, string sku, int delta, CancellationToken ct = default);
}

public sealed class FakeInventoryClient : IInventoryClient
{
    // In-memory backing store keeps the workshop running even before you implement HTTP.
    private readonly Dictionary<(string storeId, string sku), int> _stock = new()
    {
        { ("AKL", "SUPER-MIX-20KG"), 12 },
        { ("AKL", "FENCE-POST-1.8M"), 7 },
        { ("CHC", "SUPER-MIX-20KG"), 3 },
    };

    public Task<InventoryQueryResponse> GetAsync(string storeId, string sku, CancellationToken ct = default)
    {
        _stock.TryGetValue((storeId, sku), out var qty);
        return Task.FromResult(new InventoryQueryResponse(storeId, sku, qty));
    }

    public Task<InventoryQueryResponse> AdjustAsync(string storeId, string sku, int delta, CancellationToken ct = default)
    {
        var key = (storeId, sku);
        _stock.TryGetValue(key, out var current);
        var updated = current + delta;
        if (updated < 0) throw new InvalidOperationException("Negative quantity not allowed");
        _stock[key] = updated;
        return Task.FromResult(new InventoryQueryResponse(storeId, sku, updated));
    }
}

// TODO: Implement HttpInventoryClient : IInventoryClient that uses HttpClient.
// - BaseAddress from env INVENTORY_URL (default http://localhost:5081).
// - GET /inventory/{storeId}/{sku} and POST /inventory/adjust.
// - Add Polly retry (e.g., 3 attempts, exponential backoff).
