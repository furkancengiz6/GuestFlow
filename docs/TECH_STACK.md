# GuestFlow Technology & Capability Catalog

**Version**: v1.1.0  
**Last Updated**: February 2026  
**Status**: Comprehensive platform overview

---

## 🏛 Table of Contents

1. [Core Backend Stack](#-core-backend-stack)
2. [Intelligence & Graph Layer](#-intelligence--graph-layer)
3. [Modern Frontend Ecosystem](#-modern-frontend-ecosystem)
4. [Integrations & PMS Adapters](#-integrations--pms-adapters)
5. [Enterprise Quality & DevOps](#-enterprise-quality--devops)
6. [Strategic Functional Modules](#-strategic-functional-modules)
7. [Security & Compliance](#-security--compliance)

---

## 🖥 Core Backend Stack

### 🚀 Framework & Language

- **.NET 8.0 (LTS)**: Core platform framework.
- **C# 11.0**: Primary development language.
- **Domain-Driven Design (DDD)**: Architectural pattern ensuring business logic isolation.
- **MediatR**: Implementation of the CQRS pattern for clean request/response handling.

### 💾 Persistence & Data

- **Entity Framework Core 8**: Modern ORM for structural data.
- **MS SQL Server**: Primary transactional database.
- **Redis (Planned)**: For high-performance distributed caching and session management.

### 📡 Communication & API

- **ASP.NET Core Web API**: RESTful interface.
- **SignalR**: Real-time duplex communication for notifications.
- **Swagger/OpenAPI**: Automatic API documentation and testing interface.

---

## 🧠 Intelligence & Graph Layer

### 🕸 Relationship Modeling

- **Neo4j Graph Database**: Maps the complex web of human interactions.
- **Cypher Query Language**: For deep relational analysis.
- **Graph Nodes**: Guest, Staff, Service, Emotion, Time.

### 🔮 Predictive Analytics

- **ML.NET**: Integrated Machine Learning for behavior forecasting.
- **Sentiment Analysis**: Dynamic mood detection from text-based touchpoints.

---

## 🎨 Modern Frontend Ecosystem

### ⚛️ Core Framework

- **React 18.2**: Component-based UI engine.
- **TypeScript 5.2**: Full type safety across the frontend.
- **Vite**: Ultra-fast build tool and development server.

### 💅 UI & Experience

- **Material UI (MUI) 5**: Professional component library.
- **Zustand**: Lightweight and scalable state management.
- **TanStack Query (React Query)**: Advanced server-state synchronization and caching.

---

## 🔌 Integrations & PMS Adapters

### 🏨 Property Management Systems

- **Opera Cloud Connector**: Specialized adapter for industry-standard PMS.
- **Elektraweb Integration**: Native support for the popular regional PMS.
- **PMSSyncService**: Background synchronization engine with hashing for change detection.

### ✈️ Online Travel Agencies (OTA)

- **Booking.com Adapter**: Real-time availability and reservation sync.
- **Expedia Integration**: Automated inventory management across channels.

---

## 🛡 Security & Compliance

### 🔐 Authentication & Control

- **JWT (JSON Web Tokens)**: Secure, stateless authentication.
- **Role-Based Access Control (RBAC)**: Fine-grained permission management.
- **HMAC Signatures**: For validating incoming webhooks and external requests.

### 📑 Data Privacy

- **AES-256 Encryption**: Protects sensitive guest information at rest.
- **PII Sanitization**: Automatic masking of identifying labels in logs and UI.
- **Ganss.XSS**: Strategic HTML sanitization for all user inputs.

---

## 🛠 Enterprise Quality & DevOps

### 🧪 Testing Strategy

- **xUnit & FluentAssertions**: For robust unit and integration testing.
- **Playwright**: Modern E2E testing for critical user flows.
- **Jest & React Testing Library**: Component-level verification.

### 🚢 Deployment & Ops

- **Docker & Kubernetes**: Container-based orchestration ready.
- **GitHub Actions**: Fully automated CI/CD pipelines.
- **Serilog**: Structured logging with Seq/ELK integration support.

---

## 🎯 Strategic Functional Modules

### 👤 Guest CRM

- 360-degree guest profile management.
- VIP status and preference tracking.
- Behavioral history and sentiment trends.

### 🚗 Logistics & Concierge

- Multi-modal transfer management (Airport/Hotel/City).
- Tour operator scheduling (City/Yacht).
- Driver and vehicle asset allocation.

### 💹 Financial Operations

- Automated Journal Entry (JE) posting to ERP systems.
- Professional multi-currency PDF invoicing.
- Profitability and cost analysis per service.
