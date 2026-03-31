# GuestFlow — Tourism Operations Intelligence Layer

![GuestFlow Hero](file:///C:/Users/PAVILION/.gemini/antigravity/brain/27940113-49ba-4cf2-bf14-4be79c585b6e/guestflow_hero_dashboard_1774982236539.png)

[![Release](https://img.shields.io/badge/release-v1.5.0--stable-blue.svg)]()
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![React](https://img.shields.io/badge/React-18.2-61dafb.svg)](https://reactjs.org/)

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

## 🎬 Product Demo (v1.5.0 Release)

Experience the fluid, high-performance interface of GuestFlow v1.5.0.

![GuestFlow Product Demo](file:///C:/Users/PAVILION/.gemini/antigravity/brain/27940113-49ba-4cf2-bf14-4be79c585b6e/guestflow_product_demo_1774982305999.webp)

---

## 🌟 Visual Gallery (Operational Suite)

### 🧹 Real-time Housekeeping Panel
Manage room status with a clean, grid-based interface. Track cleaning progress and assignments in real-time.

![Housekeeping Dashboard](file:///C:/Users/PAVILION/.gemini/antigravity/brain/27940113-49ba-4cf2-bf14-4be79c585b6e/housekeeping_dashboard_v150_1774983675962.png)

### 🛠 Technical Maintenance Tracker
Ensure technical excellence with centralized issue tracking and prompt resolution protocols.

![Maintenance Tracker](file:///C:/Users/PAVILION/.gemini/antigravity/brain/27940113-49ba-4cf2-bf14-4be79c585b6e/maintenance_tracker_v150_1774983698598.png)

### 📦 Lost & Found Inventory Hub
Transform guest accidents into 'WOW' moments with automated lost item management and return tracking.

![Lost & Found Hub](file:///C:/Users/PAVILION/.gemini/antigravity/brain/27940113-49ba-4cf2-bf14-4be79c585b6e/lost_and_found_hub_v150_1774983718477.png)

---

## 🧠 Key Capabilities

### 🏢 Intelligence & Relationships

- **Human Relations Memory**: Tracks every touchpoint between guests and staff.
- **Sentiment Analysis**: Automatic mood detection from communication channels.
- **Graph Intelligence**: Maps complex relationships between guests, services, and time.

### 🏨 Concierge & Operational Hub

- **Unified Guest Profile**: 360-degree view combining PMS data and GuestFlow behavioral history.
- **Service Hub**: Automated management of Transfers, Tours, and Restaurant bookings.
- **Operations Suite**: Integrated **Housekeeping**, **Maintenance**, and **Lost & Found** management.

### 🔌 Enterprise Integrations

- **PMS Sync**: Native connectors for **Opera Cloud** and **Elektraweb**.
- **OTA Channel Manager**: Real-time availability sync with Booking.com and Expedia.
- **Financial Ledger**: Automated journal entries for ERP systems (SAP, Oracle, etc.).

---

## 🛠 Technology Stack

| Category | Technology |
| :--- | :--- |
| **Backend** | .NET 8 (C# 11), Web API, EF Core 8 |
| **Frontend** | React 18, TypeScript, Vite, Material UI 5 |
| **Intelligence** | Neo4j (Graph DB), ML.NET (Predictive Analytics) |
| **Deployment** | Docker, Nginx (Reverse Proxy), GitHub Actions |

---

## 🚀 Quick Start (Dockerized)

Ensure you have [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/) installed.

```bash
git clone https://github.com/furkancengiz6/GuestFlow.git
cd GuestFlow
docker-compose up -d --build
```

Access the application at `http://localhost`.

---

## ⚖️ License & Copyright

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2026 **Furkan Cengiz**
