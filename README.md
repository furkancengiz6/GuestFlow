# GuestFlow — Tourism Operations Intelligence Layer

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![React](https://img.shields.io/badge/React-18.2-61dafb.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.2-3178c6.svg)](https://www.typescriptlang.org/)

> **"GuestFlow acts as the Memory of Human Relations for 5-star hotels."**

GuestFlow is an enterprise-grade Guest Management and Operations platform. Unlike traditional PMS systems that focus on transactions, GuestFlow captures the **human story** behind every interaction, transforming guest staff services into a graph-based intelligence layer.

---

## 🏛 Platform Ecosystem & Intelligence

GuestFlow follows a triple-layer data architecture to ensure both operational stability and intelligent foresight.

```mermaid
graph TD
    A[Transactional Layer - MS SQL] -->|Source Data| B[Intelligence Layer - Neo4j Graph]
    B -->|Pattern Analysis| C[Predictive Layer - AI/ML]
    
    subgraph "Operations"
        A1[Guests] --> A
        A2[Transfers] --> A
        A3[Reservations] --> A
    end
    
    subgraph "Intelligence"
        B1[Interactions] --> B
        B2[Sentiments] --> B
        B3[Relationships] --> B
    end
    
    subgraph "Outcomes"
        C1[Personalized Offers] --> D[WOW Experience]
        C2[Risk Mitigation] --> D
        C3[Efficiency Boost] --> D
    end
```

---

## 🌟 Key Capabilities

### 🧠 Intelligence & Relationships

- **Human Relations Memory**: Tracks every touchpoint between guests and staff.
- **Sentiment Analysis**: Automatic mood detection from communication channels.
- **Graph Intelligence**: Maps complex relationships between guests, services, and time.

### 🏨 Concierge & Front Office

- **Unified Guest Profile**: 360-degree view combining PMS data and GuestFlow behavioral history.
- **Service Hub**: Automated management of Transfers, Tours (City/Yacht), and Restaurant bookings.
- **Real-time Monitoring**: Built-in health checks and structured logging for 99.9% uptime.

### 🔌 Enterprise Integrations

- **PMS Sync**: Native connectors for **Opera Cloud** and **Elektraweb**.
- **OTA Channel Manager**: Real-time availability sync with Booking.com and Expedia.
- **Financial Ledger**: Automated journal entries for ERP systems (SAP, Oracle, etc.).

---

## 🛠 Technology Stack

| Category | Technology |
| :--- | :--- |
| **Backend** | .NET 8 (C# 11), Web API, EF Core 8 |
| **Frontend** | React 18, TypeScript, Vite, Material UI (MUI) 5 |
| **Intelligence** | Neo4j (Graph DB), ML.NET (Predictive) |
| **Real-time** | SignalR, WebSocket |
| **Reporting** | QuestPDF, ClosedXML (Excel/CSV) |
| **Security** | JWT-based RBAC, PII Sanitization, AES-256 |

---

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (LTS)
- SQL Server (LocalDB or Enterprise)

### 1. Clone & Setup Backend

```bash
git clone https://github.com/furkancengiz6/GuestFlow.git
cd GuestFlow
# Update connection string in appsettings.json if needed
dotnet ef database update --project GuestFlow.Persistence
dotnet run --project GuestFlow.Api
```

### 2. Setup Frontend

```bash
cd GuestFlow.Frontend
npm install
npm run dev
```

---

## 📚 Documentation

Detailed guides for various project aspects:

- [📖 **Vision & AI Layer**](VISION_TURIZM_INTELLIGENCE_LAYER.md)
- [⚙️ **Technical Specifications**](docs/TECHNICAL_SPECIFICATIONS.md)
- [🛠 **Tech Stack & Libraries**](docs/TECH_STACK.md)
- [📡 **API Documentation**](docs/API.md)
- [🚢 **Deployment Guide**](docs/DEPLOYMENT.md)
- [🧪 **Testing Framework**](docs/TESTING.md)

---

## 🛣 Roadmap

We are currently at **94% completion**. Key remaining focus:

- [x] CRM & Concierge Core
- [x] Financial Integration
- [x] Graph Intelligence Layer
- [/] Advanced AI Predictions & ML Integration

See [ROADMAP.md](ROADMAP.md) for full details.

---

## ⚖️ License & Copyright

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 **Furkan Cengiz**

---

<img width="1898" height="1742" alt="guestflow" src="https://github.com/user-attachments/assets/f5a2be94-5337-4cc0-9b6c-e2a7cc4f8dfe" />

---

<img width="1859" height="916" alt="{F51866EB-8310-481C-993D-05780D3F63D5}" src="https://github.com/user-attachments/assets/1026733f-7fa7-44cc-9400-622ee1be5775" />

---
