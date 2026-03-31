# 👩‍💻 GuestFlow Developer Guide

Welcome to the GuestFlow development documentation. This guide is designed to get you up and running with the GuestFlow platform as quickly as possible.

## 📋 Prerequisites

Ensure you have the following installed on your machine:

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**: Required for the backend.
- **[Node.js](https://nodejs.org/) (LTS)**: Required for the frontend (v18+ recommended).
- **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)**: LocalDB or Developer edition.
- **Visual Studio 2022** or **VS Code**: Recommended IDEs.

---

## 🚀 Getting Started

### 1. Backend Setup

The backend is an ASP.NET Core Web API following Clean Architecture principles.

1. **Navigate to the solution root:**

    ```bash
    cd GuestFlow
    ```

2. **Configure Database:**
    Update the connection string in `GuestFlow.Api/appsettings.Development.json` if necessary. By default, it points to a local instance.

3. **Apply Migrations:**
    Create the database and apply the schema:

    ```bash
    dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
    ```

4. **Seed Demo Data (Optional but Recommended):**
    To populate the database with realistic test data (guests, reservations, shifts), set the following in `appsettings.Development.json`:

    ```json
    "SeedDemoData": "true"
    ```

    *Note: This will generate random extensive data on startup.*

5. **Run the API:**

    ```bash
    dotnet run --project GuestFlow.Api
    ```

    The API will be available at `https://localhost:7020` (Swagger at `/swagger`).

### 2. Frontend Setup

The frontend is a React application built with Vite and TypeScript.

1. **Navigate to the frontend directory:**

    ```bash
    cd GuestFlow.Frontend
    ```

2. **Install Dependencies:**

    ```bash
    npm install
    ```

3. **Run Development Server:**

    ```bash
    npm run dev
    ```

    The application will launch at `http://localhost:5173`.

---

## 🏗 Project Architecture

The solution follows **Domain-Driven Design (DDD)** and **Clean Architecture**:

| Project | Layer | Description |
| :--- | :--- | :--- |
| **`GuestFlow.Domain`** | Domain | **The Core.** Entities, Enums, Value Objects, and Domain Events. No external dependencies. |
| **`GuestFlow.Application`** | Application | **Business Logic.** CQRS handlers (MediatR), DTOs, Validators, and Interfaces. Depends on Domain. |
| **`GuestFlow.Persistence`** | Infrastructure | **Data Access.** EF Core Context, Migrations, Repositories, and Seeders. Implements Application interfaces. |
| **`GuestFlow.Infrastructure`** | Infrastructure | **External Services.** Email, SMS, File Storage, Stripe, etc. (Merged into Persistence/Api in some contexts). |
| **`GuestFlow.Api`** | Presentation | **Entry Point.** Controllers, Middleware, Filters, and DI Configuration. |
| **`GuestFlow.Frontend`** | Presentation | **UI.** React SPA. |

---

## 🛠 Common Workflows

### Database Migrations

When you modify an entity in `GuestFlow.Domain`:

1. **Add Migration:**

    ```bash
    dotnet ef migrations add NameOfChange --project GuestFlow.Persistence --startup-project GuestFlow.Api
    ```

2. **Update Database:**

    ```bash
    dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
    ```

### Running Tests

Unit and Integration tests are located in `GuestFlow.Application.Tests`.

```bash
dotnet test
```

### Testing & Seeding Logic

The project uses a provider-agnostic database initialization strategy:

- **Development/Production**: Uses `MigrateAsync()` to apply standard Entity Framework migrations.
- **Integration Tests**: Uses an **SQLite In-Memory** database with `EnsureCreatedAsync()`. This ensures tests are fast, isolated, and avoid migration incompatibility issues in SQLite.
- **Seeding**: Demo data is seeded automatically when `SeedDemoData` is `true`. In tests, this is handled via `TestWebApplicationFactory`.

---

## 📚 Reference Documentation

- [**API Documentation**](API.md): Endpoints, Authentication, and Payload examples.
- [**Tech Stack**](TECH_STACK.md): Detailed breakdown of libraries and technologies used.
- [**Deployment**](DEPLOYMENT.md): Guide for deploying to production (Docker/IIS).
- [**Technical Specifications**](TECHNICAL_SPECIFICATIONS.md): Detailed system requirements and constraints.

---

## 🆘 Troubleshooting

- **Build Errors?** specific to `HtmlSanitizer`? Run `dotnet restore` to pull the latest patched versions.
- **Missing Data?** Ensure `SeedDemoData` is set to `true` in your environment variables or appsettings.
- **Port Conflicts?** Check `GuestFlow.Api/Properties/launchSettings.json` or `GuestFlow.Frontend/vite.config.ts`.
