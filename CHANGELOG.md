# GuestFlow Changelog

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.5.0] - 2026-03-31

### Added

- **Operational Suite**: Integrated **Housekeeping**, **Maintenance**, and **Lost & Found** management (v1.5.0 feature).
- **Consolidated Modules**: Finalized CRM, Intelligence, Logistics, and Financial integrations.
- **Visual Asset Overhaul**: Premium hero images and documentation gallery.
- **Mobile Edge App**: Production-ready React Native (Expo) app for field operations.
- **Predictive Analytics V2**: Advanced sentiment analysis and risk detection engine.
- **OTA Real-time Sync**: Real-time availability for Booking.com and Expedia.

### Changed

- **System Status**: Transitioned from "94% Complete" to "100% Stable Production".
- **Documentation**: Final verification of all integration guides and API references.

---

## [1.1.0] - 2026-02-03

### Added

- **Global Documentation Overhaul**: Completely rewrote all `.md` files to align with the "Tourism Operations Intelligence Layer" vision.
- **Architectural Diagrams**: Integrated Mermaid diagrams for system architecture and mobile strategy.
- **Strategic Vision Document**: Detailed the Triple-Layer Intelligence model (Transactional, Graph, Predictive).

### Changed

- **Branding Alignment**: Standardized project terminology around "Intelligence Layer" and "Human Relationship Memory."
- **API Reference**: Restructured `API.md` and `API_ENDPOINTS.md` into high-level Integration and low-level Reference guides.
- **Tech Stack Catalog**: Updated with rationale for Neo4j, ML.NET, and modern frontend tools.

---

## [1.0.1] - 2025-01-13

### Added

- **Hotel & Restaurant Modules**: Comprehensive CRUD operations and filtering for hospitality assets.
- **Itinerary Management**: Visual timeline engine for multi-service guest schedules.
- **Transfer Recommendations**: ML-driven suggestions for airport and excursion logistics.
- **Service Bundling**: Ability to package Transfers, Tours, and Reservations into single itineraries.

### Changed

- **Entity Schemas**: Expanded `TransferEntity` and `TourEntity` to support complex hotel/restaurant associations.
- **Authentication**: Hardened JWT rotation logic and HttpOnly cookie security.

### Fixed

- **Currency Handling**: Resolved rounding issues in multi-currency itinerary calculations.
- **Notification Race Conditions**: Fixed SignalR hub connection drops during high-concurrency event broadcasts.

---

## [1.0.0] - 2024-12-10

### Added

- **Core Operations Engine**: Initial release with Guest CRM and Transfer logs.
- **Financial Ledger**: Automated PDF invoicing and Journal Entry posting.
- **Security Core**: Role-Based Access Control (RBAC) and data sanitization.
- **PMS Adapters**: Initial support for Opera and Elektraweb synchronization.

---

## [Unreleased]

- No pending features. Project is in Stable Maintenance mode.
