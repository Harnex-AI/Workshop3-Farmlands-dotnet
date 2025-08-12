# Farmlands Click-&-Collect (Workshop)

A minimal **.NET 8** solution to demo **GitHub Copilot’s agentic workflow** (plan → implement → test → fix) for .NET developers.

- Two Minimal APIs: **Inventory** and **Reservations**
- Shared contracts and a tiny message bus
- Tests project + CI skeleton
- Deliberately incomplete with clear TODOs you’ll finish using Copilot

---

## Architecture (thin, on purpose)

```
+-------------------------+           +-----------------------+
|  Reservations API       |  HTTP     |  Inventory API        |
|  POST /reservations     +----------->  GET /inventory/...   |
|                         |           |  POST /inventory/...  |
|  IInventoryClient  ---- +           +-----------------------+
|  IMessageBus (events)   |
+-----------+-------------+
            | emits
            v
        ReplenishRequested (console now; swap to real bus later)
```

### Projects
```
src/
  Farmlands.Inventory.Api/     # Minimal API, in-memory stock, Swagger
  Farmlands.Reservations.Api/  # Minimal API, uses FakeInventoryClient (replace it)
  Farmlands.Shared/            # Contracts, simple ConsoleMessageBus
tests/
  Farmlands.Tests/             # xUnit; starter test scaffold
.github/workflows/
  ci.yml                       # build/test; extend to publish/deploy
```

### Endpoints

**Inventory (5081)**
- `GET /inventory/{storeId}/{sku}` → `InventoryQueryResponse`
- `POST /inventory/adjust` → `InventoryQueryResponse`
- `GET /health`

**Reservations (5082)**
- `POST /reservations` → `ReservationResponse`
- `GET /health`

Swagger is enabled on both services: `/swagger`

---

## Quick start

```bash
# prerequisites: .NET 8 SDK
dotnet --info

# build everything
dotnet build

# terminal 1 - Inventory API
dotnet run --project src/Farmlands.Inventory.Api/Farmlands.Inventory.Api.csproj

# terminal 2 - Reservations API
dotnet run --project src/Farmlands.Reservations.Api/Farmlands.Reservations.Api.csproj
```

- Inventory URL: `http://localhost:5081`
- Reservations URL: `http://localhost:5082`

If ports differ, pass `--urls http://localhost:PORT` or update `Properties/launchSettings.json` for each project.

---

## Smoke test

**Inventory**
```bash
# get current stock
curl http://localhost:5081/inventory/AKL/SUPER-MIX-20KG

# add stock
curl -X POST http://localhost:5081/inventory/adjust   -H "Content-Type: application/json"   -d '{ "storeId":"AKL", "sku":"SUPER-MIX-20KG", "delta": 5 }'
```

**Reservations**
```bash
curl -X POST http://localhost:5082/reservations   -H "Content-Type: application/json"   -d '{ "storeId":"AKL","sku":"SUPER-MIX-20KG","quantity":2 }'
```
> Initially the Reservations API uses a **FakeInventoryClient**, so Inventory stock will not change until you complete TODO #1.

---

## Environment variables

| Service        | Variable        | Purpose                                  | Default                 |
|----------------|-----------------|------------------------------------------|-------------------------|
| Reservations   | `INVENTORY_URL` | Base URL for Inventory HTTP client       | `http://localhost:5081` |
| Reservations   | `THRESHOLD`     | Low-stock threshold for event emission   | `5` (wired in code now) |

---

## Known limitations (intentional)

- No persistent database (in-memory only)
- Fake inventory calls in Reservations
- No idempotency on POST /reservations
- Observability is bare minimum
- CI only builds/tests; no publish/deploy yet

All of the above become your **Copilot-driven TODOs**.

---

## Troubleshooting

- **404 at root**: hit `/swagger` or real routes like `/health` or `/inventory/...`.
- **Port conflicts**: pass `--urls` or edit `launchSettings.json`.
- **CORS**: not needed here; both are server-side only.
- **Nothing happens on reservation**: expected with `FakeInventoryClient`. Complete TODO #1.

---
