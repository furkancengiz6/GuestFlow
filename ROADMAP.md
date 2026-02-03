# GuestFlow Strategic Product Roadmap

> **📊 Current Completion**: 94% (High-Priority Milestone Core)  
> **Last Updated**: February 2026
> **Status**: Transitioning from Operational Core to Intelligence Layer

---

## 🎯 Global Vision & Mission

**Vision**: GuestFlow is the "Digital Memory of Human Relations" for the luxury hospitality industry.

**Mission**: To build the **Tourism Operations Intelligence Layer**—a sophisticated graph-based platform that models the complex interplay between Guests, Staff, Services, Time, and Emotions to deliver unprecedented personalized experiences.

---

## 📍 Platform Snapshot (Q1 2026)

### Core Backend (GuestFlow.Api)

- **Architecture**: Domain-Driven Design (DDD) with clear separation of concerns.
- **Security**: JWT-based Auth, HMAC Webhook Signatures, AES-256 PII Sanitization.
- **Operations**: Multi-channel Health Monitoring, Structured Serilog, Real-time SignalR Hubs.
- **Modules**: Guest CRM, Logistics (Transfers), Experience Hub (Tours), Financial Ledger (Invoicing/Journal).

### Core Frontend (GuestFlow.Frontend)

- **Stack**: React 18 + TypeScript + Vite + MUI 5.
- **Performance**: Edge-optimized components, Automated Cache Management (React Query).
- **UX**: Responsive Design for Concierge Desks, 60%+ Test Coverage.

---

## 📅 Roadmap Phases

### Phase 1: Operational Stabilization (Completed)

- **Security Hardening**: Standardized API security headers and rate limiting.
- **Quality Gates**: Automated CI/CD pipelines with Playwright E2E and Jest unit tests.
- **Documentation**: Unified technical and strategic documentation.

### Phase 2: Intelligence & Relationship Layer (Current Focus)

- **Graph Integration**: Porting high-priority guest interactions to Neo4j.
- **Relationship Mapping**: Linking guest sentiment and preferences to service delivery outcomes.
- **360° Profile**: Unified view of guest lifetime value and interaction frequency.

### Phase 3: Enterprise Integration & FinOps (Q2 2026)

- **PMS Deep-Sync**: Real-time two-way synchronization with Opera and Elektraweb.
- **Advanced FinOps**: Automated profitability analysis and supplier cost tracking per reservation.
- **Accounting Bridges**: Export-ready GL mappings for major ERP systems.

### Phase 4: Predictive Experience (Q3 2026)

- **ML Demand Forecasting**: Predicting peak transfer and tour loads 30 days in advance.
- **Sentiment Triggers**: Automated alerts for at-risk guest relationships based on interaction tone.
- **Dynamic Pricing**: Algorithmic price adjustments for ancillary services.

---

## 🛠 Active Technical Sprints

### Sprint A: Mobile Field Operations

- Deployment of the React Native (Expo) app for drivers and field concierge.
- Real-time task dispatching and GPS status reporting.

### Sprint B: OTA Channel Management

- Finalizing Booking.com and Expedia bi-directional availability sync.
- Centralized rate management for tour packages.

### Sprint C: Analytics Dashboard V2

- High-fidelity visual reports for hotel management.
- Real-time revenue leakage detection and VIP relationship heatmaps.

---

**Note**: For detailed task management and ticket tracking, please refer to the internal Project Management board.
