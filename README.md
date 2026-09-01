# 🏛️ Hall Rent API

A backend service for booking halls for weddings, banquets, conferences, and similar events: hall search by capacity and dates, booking with additional services, a service catalog, and revenue analytics.

Built with **ASP.NET Core 8 (Web API)** + **Entity Framework Core** + **SQL Server**, with validation via **FluentValidation** and a unified error-handling middleware.

---

## Contents

- [Stack](#-stack)
- [Architecture](#-architecture)
- [Quick Start](#-quick-start)
- [Configuration](#-configuration)
- [Database Migrations](#-database-migrations)
- [API](#-api)
- [Error Handling](#-error-handling)
- [Tests](#-tests)
- [Postman](#-postman)

---

## 🧰 Stack

| Component         | Technology                               |
|------------------|------------------------------------------|
| Runtime          | .NET 8 / ASP.NET Core Web API            |
| ORM              | Entity Framework Core 8 (SQL Server)     |
| Validation       | FluentValidation                         |
| API Docs         | Swashbuckle (Swagger / OpenAPI)          |
| Tests            | xUnit, Moq, FluentAssertions, EF Core InMemory/SQLite |

---

## 🏗 Architecture

A classic layered architecture:

```text
Controllers   → receive HTTP requests, validate them (FluentValidation), and map to DTOs
   ↓
Services      → business logic (capacity/availability checks, price calculation, transactions)
   ↓
Repositories  → data access via EF Core (UnitOfWork + DbContext)
   ↓
SQL Server
```

Key architectural decisions:

- **Booking runs inside a Serializable transaction.**  
  `BookingService.BookAsync` wraps the entire booking operation in a transaction with the `Serializable` isolation level (through `TransactionRunner`) to prevent race conditions when multiple requests try to book the same hall for overlapping time ranges. On conflict, SQL Server throws a serialization failure, which `SerializationConflictResolver` converts into `409 Conflict` — the client should retry the request.

- **Service price is frozen at booking time.**  
  `HallBookingFavorEntity.PriceAtBooking` stores the service price at the moment of booking, so later changes in the service catalog do not retroactively affect already created bookings.

- **Unified exception handling via Chain of Responsibility.**  
  `CustomExceptionHandlingMiddleware` catches any unhandled exception and passes it to `ExceptionDispatcher`, which iterates through registered `IExceptionResolver` instances in order (validation → serialization conflict → unique constraint → domain `AppException` → fallback 500) and returns a unified JSON error format. The resolver registration order matters — it is defined in `InfrastructureDi.AddExceptions`.

- **Hall availability search and occupancy checks** are based on the same half-open interval overlap formula `[From, To)`: `b.From < to && b.To > from`  
  (see `Helpers/Specification.cs`).

---

## 🚀 Quick Start

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local, Docker, or Azure SQL) — or LocalDB on Windows

### Run

```bash
git clone <repo-url>
cd Hall_rent

# set the database connection string (see "Configuration")
dotnet restore
dotnet ef database update --project Hall_rent
dotnet run --project Hall_rent
```

After launch:

- API: `http://localhost:5226`
- Swagger UI: `http://localhost:5226/swagger`

---

## ⚙️ Configuration

The connection string is defined in `Hall_rent/appsettings.Development.json`  
(the file is not committed; see `.gitignore`) under `ConnectionStrings:DefaultConnection`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HallRentDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## 🗄 Database Migrations

Migrations are stored in `Hall_rent/Migrations`. Apply them to the database:

```bash
dotnet ef database update --project Hall_rent
```

Create a new migration after changing models (`Entity/`, `AppDbContext.OnModelCreating`):

```bash
dotnet ef migrations add <Name> --project Hall_rent
```

---

## 📡 API

A full interactive list of endpoints, request/response schemas, and the ability to test them manually is available in Swagger UI (`/swagger`) after the application starts. Below is a brief resource map.

### Halls — `/Hall`

| Method | Path          | Description                                   |
|--------|---------------|-----------------------------------------------|
| POST   | `/Hall`       | Create a hall                                  |
| PATCH  | `/Hall/{id}`  | Update a hall (full replacement of data and services list) |
| DELETE | `/Hall/{id}`  | Delete a hall                                  |
| GET    | `/Hall/search`| Find hall IDs available for a time interval and capacity |

### Booking — `/Booking`

| Method | Path               | Description                                  |
|--------|-------------------|----------------------------------------------|
| POST   | `/Booking/{hallId}`| Book a hall for an interval with selected services |

### Favor — `/Favor`

| Method | Path        | Description              |
|--------|-------------|--------------------------|
| GET    | `/Favor`    | List all services         |
| POST   | `/Favor`    | Create a service          |
| PATCH  | `/Favor/{id}` | Update a service        |

### Analytics — `/analytics`

| Method | Path                    | Description                                   |
|--------|------------------------|-----------------------------------------------|
| GET    | `/analytics/revenue`   | Revenue and booking count by day for a period |
| GET    | `/analytics/favors/top` | Top services by revenue for a period (limit 1–100) |

---

## ❗ Error Handling

All errors are returned in a unified JSON format:

```json
{
  "title": "ValidationError",
  "status": 400,
  "errors": ["Persons: Persons must be greater than 0."],
  "traceId": "0HN..."
}
```

| Situation                                           | HTTP status |
|----------------------------------------------------|:-----------:|
| Request validation error (FluentValidation)       | 400         |
| Entity not found (hall/service)                    | 404         |
| Hall capacity exceeded                            | 409         |
| Hall is already occupied for the selected interval | 409         |
| Service is not offered by the hall                | 409         |
| Parallel transaction conflict (serialization failure) | 409     |
| Uniqueness violation (for example, hall name already exists) | 409 |
| Unhandled server error                            | 500         |

---

## ✅ Tests

The `Hall_rent.Tests` project (xUnit + Moq + FluentAssertions) covers controllers, services, repositories (using EF Core InMemory/SQLite), validators, and exception handling.

```bash
dotnet test
```

---

## 📮 Postman

The `Postman/` folder contains a ready-made collection and environment for end-to-end API verification in one click (create a service → create a hall → search → update → book → verify rejection on overlapping bookings → analytics → delete hall). Details are in `Postman/README.txt`.
