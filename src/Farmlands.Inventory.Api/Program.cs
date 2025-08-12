
using Farmlands.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Simple in-memory stock store
var stock = new Dictionary<(string storeId, string sku), int>()
{
    { ("AKL", "SUPER-MIX-20KG"), 12 },
    { ("AKL", "FENCE-POST-1.8M"), 7 },
    { ("CHC", "SUPER-MIX-20KG"), 3 },
};

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/inventory/{storeId}/{sku}", (string storeId, string sku) =>
{
    stock.TryGetValue((storeId, sku), out var qty);
    return Results.Ok(new InventoryQueryResponse(storeId, sku, qty));
})
.WithName("GetInventory");

app.MapPost("/inventory/adjust", (InventoryAdjustRequest req) =>
{
    var key = (req.StoreId, req.Sku);
    stock.TryGetValue(key, out var current);
    var updated = current + req.Delta;
    if (updated < 0) return Results.BadRequest(new { error = "Negative quantity not allowed" });
    stock[key] = updated;
    return Results.Ok(new InventoryQueryResponse(req.StoreId, req.Sku, updated));
})
.WithName("AdjustInventory");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
