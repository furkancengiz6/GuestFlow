# GuestFlow API

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet) ![C#](https://img.shields.io/badge/C%23-11.0-brightgreen) ![License](https://img.shields.io/badge/license-MIT-blue)

**GuestFlow API** is a RESTful API designed to manage guest-related operations for a hospitality and tourism business. It provides functionalities for managing guests, airports, cities, transfers, city tours, yacht tours, daily revenues, invoices, and more.

The project is built using modern .NET practices, following a **Domain-Driven Design (DDD)** approach with a **layered architecture**.

---

## 📌 Table of Contents
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup and Installation](#setup-and-installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Maintenance Mode](#maintenance-mode)
- [Contributing](#contributing)
- [License](#license)

---

## 🚀 Features

- **Guest Management**: CRUD operations for guests with support for special guests.
- **Airport Management**: Manage airports with unique codes and associated cities.
- **City Management**: Manage cities and their associated airports, tours, and transfers.
- **Transfer Management**: Schedule and manage transfers with invoice and discount support.
- **City Tours & Yacht Tours**: Organize and manage tours with pricing, discounts, and invoice generation.
- **Daily Revenues**: Automatically calculate daily revenues.
- **Invoices**: Generate and manage invoices with PDF export support.
- **Daily Notes**: Add and manage daily notes for staff.
- **Vehicles**: Manage vehicles used for transfers.
- **Authentication & Authorization**: JWT-based authentication with role-based access (**Staff, Admin**).
- **Maintenance Mode**: Enable/disable maintenance mode to restrict API access.
- **Logging**: Comprehensive logging for debugging and monitoring.

---

## 🛠 Technologies Used

- **.NET 8.0** – Core framework for building the API.
- **C# 11.0** – Primary programming language.
- **Entity Framework Core** – ORM for database operations.
- **SQL Server** – Database backend (configurable to other EF Core-supported databases).
- **ASP.NET Core** – RESTful API development framework.
- **JWT Authentication** – Secure API endpoints with token-based authentication.
- **Dependency Injection** – Built-in DI for managing services and repositories.
- **Unit of Work & Repository Pattern** – Database transaction management.
- **Fluent API** – Entity Framework Core configuration.

---

## 📂 Project Structure

The project follows a **layered architecture** based on **Domain-Driven Design (DDD)**:

```
GuestFlow/
├── GuestFlow.Api/                # API layer (Controllers, Middleware, Filters)
├── GuestFlow.Application/        # Application layer (Business logic, Services, DTOs)
├── GuestFlow.Domain/             # Domain layer (Entities, Interfaces, Enums)
├── GuestFlow.Persistence/        # Persistence layer (DbContext, Repositories, Unit of Work)
└── README.md                     # Project documentation
```

---

## 🔧 Prerequisites

Before running the project, ensure you have the following installed:

- **.NET 8.0 SDK** → [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** → Database backend (or any EF Core-supported DB)
- **Visual Studio 2022** / **VS Code** (with C# extensions)
- **Git** → For cloning the repository

---

## 📥 Setup and Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/furkancengiz6/guestflow-api.git
   cd guestflow-api
   ```
2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```
3. **Set Up the Database**
   - Update the **connection string** in `appsettings.json`.
   - Run the migrations to create the database and tables:
     ```bash
     dotnet ef migrations add InitialCreate --project GuestFlow.Persistence
     dotnet ef database update --project GuestFlow.Persistence
     ```
4. **Build the Project**
   ```bash
   dotnet build
   ```

---

## ⚙️ Configuration

The project uses `appsettings.json` for configuration. Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GuestFlowDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "GuestFlowApi",
    "Audience": "GuestFlowApi",
    "ExpireMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

---

## ▶️ Running the Application

1. **Run the Application**
   ```bash
   dotnet run --project GuestFlow.Api
   ```
2. **Access the API**
   - Swagger UI: [https://localhost:5001/swagger](https://localhost:5001/swagger)
   - Use Postman or another tool to test endpoints.

---

## 📡 API Endpoints

| Method | Endpoint | Description | Roles |
|--------|-----------------|------------------------|--------------|
| POST   | /api/auth/register | Register a new user | None |
| POST   | /api/auth/login | Login and get a JWT token | None |
| GET    | /api/auth/me | Get current user info | Staff, Admin |
| POST   | /api/airports | Add a new airport | Staff, Admin |
| GET    | /api/airports | Get all airports | Staff, Admin |
| GET    | /api/airports/{id} | Get airport by ID | Staff, Admin |
| PATCH  | /api/settings | Toggle maintenance mode | Admin, Staff |
| GET    | /api/settings/maintenance | Get maintenance mode status | Admin |

_For a full list of endpoints, check the Swagger documentation._

---

## 🔐 Authentication

- **JWT-based authentication** is used.
- To access protected endpoints:
  1. Register via `/api/auth/register`
  2. Login via `/api/auth/login` and obtain a JWT token.
  3. Include the token in requests:
     ```
     Authorization: Bearer <your-token>
     ```

---

## 🔄 Maintenance Mode

- When enabled, all requests (except `/api/auth/login` and `/api/settings`) return **503 Service Unavailable**.
- Toggle maintenance mode via `/api/settings` (Admin, Staff).
- Check status via `/api/settings/maintenance` (Admin).

---

## 🤝 Contributing

1. **Fork the repository**.
2. **Create a new branch**:
   ```bash
   git checkout -b feature/your-feature
   ```
3. **Commit your changes**:
   ```bash
   git commit -m "Add your feature"
   ```
4. **Push & open a PR**:
   ```bash
   git push origin feature/your-feature
   ```

---

## 📜 License

This project is licensed under the **MIT License**.

## GuestFlow API Endpoints

![indir](https://github.com/user-attachments/assets/e01ef2a1-b7d9-48f2-8852-40c6c890cc40)

## GuestFlow Db Diagram

![resim](https://github.com/user-attachments/assets/3c534d60-cde1-43d0-81d0-b241034115a6)

