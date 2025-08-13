# DotNet Workshop – Copilot Agentic Workflow

## Overview
This workshop uses GitHub Copilot in an **agentic workflow** to plan → implement → test → fix features.  
Attendees will:
- Implement missing features in the **Farmlands Reservations API**.
- Improve test coverage, observability, and CI/CD.
- Use **Copilot prompts** to drive incremental development.

---

## Getting Started

### Kickoff Prompt
```plaintext
Analyze this repository and summarize the architecture, projects, and dependencies.
Then propose a concrete plan to complete all TODOs in 5–7 steps. For each step, list
files to change and acceptance criteria. Call out risks and test coverage.
```

### Iteration Prompts
- `"Apply step 1 now."`
- `"Generate the diffs for step 1 and run the tests. Fix failures."`

---

## TODO 1 – Replace `FakeInventoryClient` with `HttpInventoryClient`

**Goal:**  
Reservations API should call the real Inventory API over HTTP so stock levels actually change when a reservation is created.

**Acceptance Criteria:**
- Implement `HttpInventoryClient : IInventoryClient` using `HttpClient`.
  - `BaseAddress` from `INVENTORY_URL` env var (default: `http://localhost:5081`)
  - `GetAsync` → GET `/inventory/{storeId}/{sku}`
  - `AdjustAsync` → POST `/inventory/adjust`
- Register via `AddHttpClient` and remove `FakeInventoryClient`.
- Add **Polly retry policy**: 3 attempts, exponential backoff.
- Return HTTP 503 with `ProblemDetails` for transient failures.
- Manual smoke test: stock decrements after reservation.

**Test Prompt:**
```plaintext
Add an integration test using WebApplicationFactory that wires a TestServer for Inventory
and verifies Reservations decrements stock by calling the real HttpInventoryClient.
```

---

## TODO 2 – Idempotency for `POST /reservations`

**Goal:**  
Same `Idempotency-Key` + same request body returns the same response without applying side effects again.

**Acceptance Criteria:**
- Add idempotency middleware for `POST /reservations`.
- Require `Idempotency-Key` header.
- Hash request body; cache response (status + body) for **5 mins** in-memory.
- Replay with same key + hash returns cached response without decrementing inventory.
- Missing header returns HTTP 400 with `ProblemDetails`.
- Proper logging for cache hit/miss.

**Test Prompt:**
```plaintext
Add tests for idempotency: first call returns 201 with body X;
second call with same key returns same status/body without double decrement.
```

---

## TODO 3 – `ReplenishRequested` Event on Low Stock

**Goal:**  
After reservation, if quantity `< THRESHOLD` (env var, default `5`), publish `ReplenishRequested` via `IMessageBus`.

**Acceptance Criteria:**
- Threshold configurable (`THRESHOLD` env var).
- Event includes: `storeId`, `sku`, `currentQuantity`, `threshold`.
- Structured logging.
- Unit test with fake `IMessageBus` asserts event emission when below threshold.

---

## TODO 4 – Tests (Integration + Unit)

**Goal:**  
Green test suite with meaningful coverage.

**Acceptance Criteria:**
- Add **xUnit** tests in `Farmlands.Tests`.
- Use `WebApplicationFactory` for Reservations API.
- Replace `IInventoryClient` with test double for controlled scenarios.
- Cover:
  - 200 when stock sufficient
  - 400 when insufficient
  - Idempotent replay
  - Event emission logic

---

## TODO 5 – Observability & Health

**Goal:**  
Add basic logs, metrics, and correlation IDs.

**Acceptance Criteria:**
- Correlation ID middleware.
- Structured logging (include `storeId`, `sku`).
- Metrics counters for:
  - Reservations created
  - Inventory adjustments
- `/health` endpoint remains functional.

---

## TODO 6 – CI Improvements

**Goal:**  
Produce build artifacts and prepare for deployment.

**Acceptance Criteria:**
- Update `.github/workflows/ci.yml`:
  - Add `dotnet publish` step for both APIs (target `linux-x64`).
  - Upload publish directories as workflow artifacts.
- Scaffold `deploy.yml` for **Azure Web Apps** deployment.
  - Use `azure/webapps-deploy` action.
  - Environment variables for app names + publish-profile secrets.

---

## Debugging & Iteration Prompts
- `"Explain this new code Copilot generated, file by file. Call out potential bugs or edge cases."`
- `"Run all tests and list any failures. Propose fixes and apply them."`
- `"Tighten nullability and add guard clauses where appropriate."`

---

## Definition of Done
- Endpoints work via Swagger and `curl`.
- Reservations make **real inventory calls**.
- Idempotency verified with replay test.
- Event emitted when stock is low.
- Tests pass in CI.