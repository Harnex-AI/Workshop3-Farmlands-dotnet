
namespace Farmlands.Shared.Contracts;

public record InventoryQueryResponse(string StoreId, string Sku, int Quantity);
public record InventoryAdjustRequest(string StoreId, string Sku, int Delta);

public record ReservationRequest(string StoreId, string Sku, int Quantity);
public record ReservationResponse(Guid ReservationId, string Status);

public record ReplenishRequested(string StoreId, string Sku, int CurrentQuantity, int Threshold);
