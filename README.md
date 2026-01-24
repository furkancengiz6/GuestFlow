# GuestFlow

GuestFlow is an enterprise-grade Guest Management and Operations platform specifically designed for 5-star hotel concierge desks. It acts as the "Memory of Human Relations" for hotels, transforming interactions between guests, staff, and services into actionable intelligence.

## 🌟 Key Features

- **Comprehensive Guest Management**: VIP status tracking, preferences, and detailed history.
- **Service & Operatons Hub**:
  - **Transfers**: Airport/Hotel/Restaurant/City transfers with driver and vehicle assignment.
  - **Tours**: Managed City and Yacht tours with automated scheduling.
  - **Reservations**: Integrated restaurant bookings.
- **Financial Integration**: Automated invoice generation (PDF), multi-currency support, and journal entry posting for accounting (ERP) systems.
- **Enterprise Ready**:
  - **PMS Integration**: Real-time sync with Opera Cloud and Elektraweb.
  - **Security**: JWT-based RBAC, PII management, and comprehensive audit logging.
  - **Monitoring**: Built-in health checks and structured logging.

## 🛠 Tech Stack

### Backend

- **Framework**: .NET 8 (C# 11)
- **Database**: SQL Server (EF Core 8)
- **Reporting**: QuestPDF, ClosedXML
- **Identity**: JWT Bearer, Role-Based Access Control

### Frontend

- **Library**: React 18 (TypeScript)
- **Build Tool**: Vite
- **UI Framework**: Material UI (MUI) 5
- **State/Data**: Zustand, React Query

## 📂 Project Structure

```text
GuestFlow/
├── GuestFlow.Api/          # API Controllers and Middleware
├── GuestFlow.Application/  # Business Logic and Services
├── GuestFlow.Domain/       # Domain Entities and Interfaces
├── GuestFlow.Persistence/ # Database Context and Repositories
├── GuestFlow.Frontend/    # React Application
└── docs/                  # Project Documentation
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Node.js (Latest LTS)
- SQL Server

### Backend Setup

1. Update connection string in `appsettings.json`.
2. Run database migrations:

   ```bash
   dotnet ef database update --project GuestFlow.Persistence
   ```

3. Start the API:

   ```bash
   dotnet run --project GuestFlow.Api
   ```

### Frontend Setup

1. Navigate to `GuestFlow.Frontend`.
2. Install dependencies:

   ```bash
   npm install
   ```

3. Start dev server:

   ```bash
   npm run dev
   ```

## 📚 Documentation

For more detailed information, please refer to the files in the `docs/` directory:

- [API Reference](docs/API.md)
- [Endpoints List](docs/API_ENDPOINTS.md)
- [Technical Stack](docs/TECH_STACK.md)
- [Deployment Guide](docs/DEPLOYMENT.md)
- [Testing Guide](docs/TESTING.md)

## ⚖️ License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 Furkan Cengiz
