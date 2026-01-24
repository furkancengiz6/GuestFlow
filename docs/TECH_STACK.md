# GuestFlow - Teknolojiler ve Özellikler Kataloğu

**Versiyon**: v1.0.0  
**Güncelleme Tarihi**: 2025-01-16  
**Kapsam**: Tüm teknolojiler, kütüphaneler, özellikler ve modüller

---

## 📚 İçindekiler

1. [Backend Teknolojileri](#backend-teknolojileri)
2. [Frontend Teknolojileri](#frontend-teknolojileri)
3. [Infrastructure & DevOps](#infrastructure--devops)
4. [Test Teknolojileri](#test-teknolojileri)
5. [Özellikler ve Modüller](#özellikler-ve-modüller)
6. [Entegrasyonlar](#entegrasyonlar)
7. [Güvenlik Özellikleri](#güvenlik-özellikleri)
8. [Performans ve Ölçeklenebilirlik](#performans-ve-ölçeklenebilirlik)

---

## 🖥️ Backend Teknolojileri

### Core Framework
- **.NET 8.0** (LTS)
  - C# 11.0
  - ASP.NET Core Web API
  - Minimal APIs
  - Dependency Injection

### ORM ve Veritabanı
- **Entity Framework Core 8.0.14**
  - Code-First yaklaşımı
  - Fluent API konfigürasyonları
  - Migration desteği
  - Change Tracking
  - Lazy Loading
- **SQL Server** (Primary Database)
  - Alternatif: PostgreSQL, MySQL (EF Core desteği ile)

### Authentication & Authorization
- **JWT (JSON Web Tokens)**
  - JWT Bearer Authentication
  - Refresh Token mekanizması
  - Token expiration yönetimi
- **Role-Based Access Control (RBAC)**
  - Admin, Staff rolleri
  - Permission-based authorization
  - Feature flags

### API ve Dokümantasyon
- **Swagger/OpenAPI 6.6.2**
  - Swagger UI
  - API versioning (`/api/v1.0/...`)
  - XML comments desteği
  - Response examples

### Logging ve Monitoring
- **Serilog**
  - Structured logging
  - Console sink
  - File sink
  - Seq sink (opsiyonel)
- **Health Checks**
  - `/health` - Basic health check
  - `/health/ready` - Readiness probe
  - `/health/live` - Liveness probe
  - `/health/detailed` - Detailed health info

### Real-time Communication
- **SignalR**
  - WebSocket desteği
  - Real-time notifications
  - Hub: `/hubs/notifications`

### Object Mapping
- **AutoMapper 12.0.1**
  - Entity ↔ DTO mapping
  - Profile-based configuration
  - 521+ satır mapping kodu

### PDF Generation
- **QuestPDF 2024.3.10**
  - Invoice PDF generation
  - Report PDF export
  - Template-based design

### Excel/CSV Export
- **ClosedXML 0.102.1**
  - Excel export
  - Report generation
- **CsvHelper 30.0.1**
  - CSV export
  - Data transformation

### Image Processing
- **SixLabors.ImageSharp 3.1.12**
  - Image manipulation
  - QR code generation
  - Thumbnail creation

### QR Code
- **QRCoder 1.7.0**
  - QR code generation
  - Invoice QR codes
  - Service QR codes

### Email
- **MailKit 4.8.0**
  - SMTP email sending
  - HTML email templates
  - Attachment support

### OTP (One-Time Password)
- **Otp.NET 1.3.0**
  - TOTP generation
  - 2FA support

### Calendar
- **Ical.Net 4.2.0**
  - iCalendar format
  - Event export
  - Calendar integration

### Cloud Storage
- **Azure.Storage.Blobs 12.19.1**
  - File upload/download
  - Document storage
  - Image storage

### Security
- **Ganss.XSS** (HTML Sanitization)
  - XSS protection
  - HTML sanitization
  - Request body cleaning

### Package Management
- **NuGet**
  - Package references
  - Version management

---

## 🎨 Frontend Teknolojileri

### Core Framework
- **React 18.2.0**
  - Functional components
  - Hooks (useState, useEffect, useContext, etc.)
  - Context API
- **TypeScript 5.2.2**
  - Type safety
  - Interface definitions
  - Generic types

### Build Tool
- **Vite 5.4.21**
  - Fast HMR (Hot Module Replacement)
  - Optimized production builds
  - Code splitting
  - Tree shaking

### UI Framework
- **Material-UI (MUI) 5.15.0**
  - Component library
  - Theming
  - Responsive design
  - Dark mode support
- **@mui/icons-material 5.15.0**
  - Icon library
  - 2000+ icons

### Data Grid
- **@mui/x-data-grid 8.23.0**
  - Advanced data tables
  - Sorting, filtering, pagination
  - Export functionality

### Date Pickers
- **@mui/x-date-pickers 8.21.0**
  - Date selection
  - Time pickers
  - Date range selection

### State Management
- **Zustand 4.4.7**
  - Global state management
  - Auth store
  - Settings store
- **@tanstack/react-query 5.17.0**
  - Server state management
  - Caching
  - Background refetching
  - Optimistic updates

### Routing
- **react-router-dom 6.30.3**
  - Client-side routing
  - Lazy loading
  - Code splitting
  - Protected routes

### Forms
- **react-hook-form 7.49.2**
  - Form management
  - Validation
  - Performance optimized
- **@hookform/resolvers 5.2.2**
  - Zod integration
  - Validation resolvers
- **zod 4.1.13**
  - Schema validation
  - Type inference
  - Runtime validation

### HTTP Client
- **axios 1.6.2**
  - REST API calls
  - Interceptors
  - Request/response transformation

### Real-time Communication
- **@microsoft/signalr 10.0.0**
  - WebSocket connection
  - Real-time notifications
  - Hub connection

### Charts & Visualization
- **recharts 3.5.1**
  - Line charts
  - Bar charts
  - Pie charts
  - Dashboard visualizations

### Maps
- **@react-google-maps/api 2.20.8**
  - Google Maps integration
  - Location display
  - Route visualization

### Internationalization
- **i18next 25.7.2**
  - Multi-language support
  - Translation management
- **react-i18next 16.5.0**
  - React integration
  - Hooks for translations

### Notifications
- **notistack 3.0.2**
  - Toast notifications
  - Snackbar messages
  - Success/error/info/warning

### Date Utilities
- **date-fns 4.1.0**
  - Date formatting
  - Date manipulation
  - Locale support

### Styling
- **@emotion/react 11.11.1**
  - CSS-in-JS
  - Styled components
- **@emotion/styled 11.11.0**
  - Styled components

---

## 🏗️ Infrastructure & DevOps

### Containerization
- **Docker**
  - Multi-stage builds
  - Docker Compose
  - Container orchestration

### Orchestration
- **Kubernetes (k8s)**
  - Deployment configurations
  - Service definitions
  - ConfigMaps
  - Secrets

### Web Server
- **Nginx**
  - Reverse proxy
  - Load balancing
  - SSL termination

### Caching (Planned)
- **Redis**
  - Session storage
  - Cache layer
  - Rate limiting

### Monitoring (Planned)
- **Prometheus**
  - Metrics collection
- **Grafana**
  - Visualization
  - Dashboards

### CI/CD
- **GitHub Actions**
  - Automated builds
  - Test execution
  - Deployment automation

### Cloud Platforms (Supported)
- **Microsoft Azure**
  - App Service
  - SQL Database
  - Blob Storage
- **AWS**
  - EC2
  - RDS
  - S3
- **Google Cloud Platform (GCP)**
  - Compute Engine
  - Cloud SQL
  - Cloud Storage

---

## 🧪 Test Teknolojileri

### Backend Testing
- **xUnit**
  - Unit tests
  - Integration tests
  - Test fixtures

### Frontend Testing
- **Jest 30.2.0**
  - Unit tests
  - Component tests
  - Snapshot tests
- **@testing-library/react 16.3.1**
  - React component testing
  - User interaction simulation
- **@testing-library/jest-dom 6.9.1**
  - DOM matchers
  - Custom assertions
- **@testing-library/user-event 14.6.1**
  - User interaction events
- **@swc/jest 0.2.39**
  - Fast test execution
  - TypeScript support

### E2E Testing
- **Playwright 1.57.0**
  - End-to-end tests
  - Cross-browser testing
  - Visual regression testing

### Test Coverage
- **Jest Coverage**
  - Code coverage reports
  - Coverage thresholds
  - HTML reports

---

## 🎯 Özellikler ve Modüller

### 1. Misafir Yönetimi (Guest Management)
- ✅ Guest CRUD operations
- ✅ Guest search and filtering
- ✅ Guest detail pages
- ✅ Special guest management (VIP)
- ✅ Guest history tracking
- ✅ Guest preferences management
- ✅ Guest communication history
- ✅ Guest invoice history
- ✅ Guest room assignment history

### 2. Transfer Yönetimi (Transfer Management)
- ✅ Transfer CRUD operations
- ✅ Multiple transfer types:
  - Airport to Hotel
  - Hotel to Airport
  - Hotel to Restaurant
  - Restaurant to Hotel
  - Hotel to City
  - City to Hotel
  - Hotel to Hotel
- ✅ Transfer scheduling
- ✅ Driver assignment
- ✅ Vehicle assignment
- ✅ Transfer pricing
- ✅ Discount management
- ✅ Invoice generation
- ✅ Transfer recommendations
- ✅ Transfer status tracking

### 3. Tur Yönetimi (Tour Management)

#### City Tours
- ✅ City tour CRUD
- ✅ Tour scheduling
- ✅ Guest assignment
- ✅ Pricing and discounts
- ✅ Invoice generation
- ✅ Tour status tracking

#### Yacht Tours
- ✅ Yacht tour CRUD
- ✅ Tour scheduling
- ✅ Guest assignment
- ✅ Pricing and discounts
- ✅ Invoice generation
- ✅ Tour status tracking

### 4. Faturalandırma (Invoicing)
- ✅ Invoice CRUD operations
- ✅ Invoice item management
- ✅ PDF generation (QuestPDF)
- ✅ Email sending
- ✅ Invoice status tracking
- ✅ Payment tracking
- ✅ Currency support
- ✅ Tax calculation
- ✅ Discount application

### 5. Ödeme Yönetimi (Payment Management)
- ✅ Payment recording
- ✅ Payment methods:
  - Cash
  - Credit Card
  - Bank Transfer
  - Other
- ✅ Payment status tracking
- ✅ Payment reminders
- ✅ Outstanding balance tracking
- ✅ Payment history

### 6. Muhasebe Entegrasyonu (Accounting Integration)
- ✅ Journal entry creation
- ✅ GL (General Ledger) mapping
- ✅ Debit/Credit balance
- ✅ Journal preview
- ✅ Journal posting
- ✅ Idempotency control
- ✅ Accounting export (CSV/Excel)

### 7. Rezervasyon Sistemi (Reservation System)
- ✅ Restaurant reservations
- ✅ Reservation CRUD
- ✅ Transfer integration
- ✅ Guest assignment
- ✅ Reservation status tracking
- ✅ Reservation calendar view

### 8. Oda Yönetimi (Room Management)
- ✅ Room assignment tracking
- ✅ Room assignment history
- ✅ Check-in/check-out dates
- ✅ Room number management
- ✅ PMS room status sync

### 9. Otel Yönetimi (Hotel Management)
- ✅ Hotel CRUD operations
- ✅ Hotel search and filtering
- ✅ Star rating
- ✅ Location information
- ✅ Amenities

### 10. Restoran Yönetimi (Restaurant Management)
- ✅ Restaurant CRUD operations
- ✅ Restaurant search and filtering
- ✅ Cuisine types
- ✅ Capacity management
- ✅ Reservation requirements

### 11. Havalimanı Yönetimi (Airport Management)
- ✅ Airport CRUD operations
- ✅ Airport codes (IATA)
- ✅ City association
- ✅ Location information

### 12. Şehir Yönetimi (City Management)
- ✅ City CRUD operations
- ✅ City search and filtering
- ✅ Airport association
- ✅ Tour association

### 13. Araç Yönetimi (Vehicle Management)
- ✅ Vehicle CRUD operations
- ✅ Vehicle types
- ✅ Capacity information
- ✅ Assignment to transfers

### 14. Personel Yönetimi (Personnel Management)
- ✅ User CRUD operations
- ✅ Role management (Admin, Staff)
- ✅ Permission management
- ✅ User authentication
- ✅ User activity tracking

### 15. Tedarikçi Yönetimi (Supplier Management)
- ✅ Supplier CRUD operations
- ✅ Supplier types
- ✅ Cost tracking
- ✅ Profitability analysis
- ✅ Supplier performance metrics

### 16. Raporlama (Reporting)
- ✅ Dashboard overview
- ✅ Daily revenue reports
- ✅ Profitability reports
- ✅ Guest statistics
- ✅ Service statistics
- ✅ Financial reports
- ✅ Export functionality (PDF, Excel, CSV)

### 17. Günlük Operasyonlar (Daily Operations)
- ✅ Daily notes
- ✅ Daily revenue calculation
- ✅ Service confirmations
- ✅ Upcoming services
- ✅ Unpaid services
- ✅ Risk flags

### 18. Concierge Dashboard
- ✅ Today's check-ins
- ✅ Today's check-outs
- ✅ Active guests
- ✅ Upcoming services
- ✅ Guest status indicators (VIP, special requests, etc.)
- ✅ Quick actions
- ✅ Unified guest profile
- ✅ Guest history dashboard

### 19. İletişim Merkezi (Communication Hub)
- ✅ Unified communication history
- ✅ Email integration
- ✅ SMS integration
- ✅ WhatsApp integration
- ✅ In-app notifications
- ✅ Smart notifications:
  - Pre-Arrival
  - Arrival
  - During Stay
  - Pre-Departure
  - Special Occasions
- ✅ Communication templates
- ✅ Multi-channel messaging

### 20. PMS Entegrasyonları (PMS Integrations)
- ✅ Opera Cloud integration
- ✅ Elektraweb integration
- ✅ Real-time webhook processing
- ✅ Guest profile sync
- ✅ Reservation sync
- ✅ Room status sync
- ✅ Folio (invoice) sync
- ✅ Polling fallback mechanism
- ✅ Signature validation (HMAC SHA256)

### 21. OTA Entegrasyonları (OTA Integrations) - Development
- ⚠️ Booking.com integration (in progress)
- ⚠️ Expedia integration (planned)
- ⚠️ Channel manager
- ⚠️ Availability sync
- ⚠️ Rate sync

### 22. Dosya Yönetimi (File Management)
- ✅ File upload/download
- ✅ Document storage
- ✅ Image storage
- ✅ Azure Blob Storage integration
- ✅ File type validation

### 23. Takvim (Calendar)
- ✅ Calendar view
- ✅ Event management
- ✅ iCalendar export
- ✅ Service scheduling

### 24. Para Birimi Yönetimi (Currency Management)
- ✅ Currency CRUD
- ✅ Default currency
- ✅ Currency conversion
- ✅ Multi-currency support

### 25. Servis Paketleri (Service Packages)
- ✅ Package creation
- ✅ Transfer + Tour + Restaurant bundling
- ✅ Package pricing
- ✅ Package management

### 26. Itinerary Yönetimi (Itinerary Management)
- ✅ Travel plan creation
- ✅ Timeline visualization
- ✅ Itinerary items
- ✅ Status tracking

### 27. Bildirimler (Notifications)
- ✅ Real-time notifications (SignalR)
- ✅ Email notifications
- ✅ SMS notifications
- ✅ In-app notifications
- ✅ Notification history
- ✅ Notification preferences

### 28. Ayarlar (Settings)
- ✅ System settings
- ✅ User preferences
- ✅ Feature flags
- ✅ Maintenance mode

### 29. Günlük Notlar (Daily Notes)
- ✅ Note creation
- ✅ Note management
- ✅ Staff notes
- ✅ Date-based filtering

### 30. Günlük Gelirler (Daily Revenues)
- ✅ Automatic revenue calculation
- ✅ Daily revenue tracking
- ✅ Revenue reports
- ✅ Revenue analysis

---

## 🔌 Entegrasyonlar

### PMS (Property Management Systems)
1. **Opera Cloud**
   - REST API integration
   - Webhook support
   - Real-time sync
   - Guest profile sync
   - Reservation sync
   - Room status sync
   - Folio sync

2. **Elektraweb**
   - REST API integration
   - Webhook support
   - Real-time sync
   - Guest profile sync
   - Reservation sync

### OTA (Online Travel Agencies) - Development
1. **Booking.com** (in progress)
   - API v2/v3 integration
   - Webhook handling
   - Reservation sync
   - Availability sync

2. **Expedia** (planned)
   - EPS API integration
   - Reservation sync
   - Rate sync

### Communication Channels
1. **Email (SMTP)**
   - MailKit integration
   - HTML templates
   - Attachment support

2. **SMS**
   - Provider integration
   - Template support
   - Delivery tracking

3. **WhatsApp Business**
   - WhatsApp Business API
   - Message templates
   - Delivery tracking

4. **Google Maps**
   - Location display
   - Route visualization
   - Geocoding

### Cloud Services
1. **Azure Blob Storage**
   - File storage
   - Document storage
   - Image storage

### Payment Gateways (Planned)
- Stripe
- PayPal
- Local payment providers

---

## 🔒 Güvenlik Özellikleri

### Authentication
- ✅ JWT-based authentication
- ✅ Refresh token mechanism
- ✅ Token expiration
- ✅ Password hashing (BCrypt)
- ✅ 2FA support (OTP)

### Authorization
- ✅ Role-based access control (RBAC)
- ✅ Permission-based authorization
- ✅ Feature flags
- ✅ Resource-level permissions

### Security Headers
- ✅ Content Security Policy (CSP)
- ✅ HTTP Strict Transport Security (HSTS)
- ✅ X-XSS-Protection
- ✅ X-Content-Type-Options
- ✅ X-Frame-Options
- ✅ Referrer-Policy

### Input Validation
- ✅ HTML sanitization (Ganss.XSS)
- ✅ Request body cleaning
- ✅ SQL injection prevention (EF Core parameterized queries)
- ✅ XSS protection

### Rate Limiting
- ✅ API rate limiting
- ✅ Request throttling
- ✅ IP-based limiting

### Audit Logging
- ✅ Entity change tracking
- ✅ User activity logging
- ✅ Security event logging

### Data Protection
- ✅ PII (Personally Identifiable Information) management
- ✅ Data anonymization
- ✅ Privacy action history

---

## ⚡ Performans ve Ölçeklenebilirlik

### Backend Performance
- ✅ Async/await patterns
- ✅ Database query optimization
- ✅ Lazy loading
- ✅ Eager loading (Include)
- ✅ Pagination
- ✅ Caching (planned)

### Frontend Performance
- ✅ Code splitting
- ✅ Lazy loading
- ✅ React Query caching
- ✅ Optimized bundle size
- ✅ Image optimization

### Scalability
- ✅ Stateless API design
- ✅ Horizontal scaling ready
- ✅ Database connection pooling
- ✅ Background job processing
- ✅ Microservices-ready architecture

### Monitoring
- ✅ Health checks
- ✅ Structured logging
- ✅ Error tracking (planned)
- ✅ Performance monitoring (planned)
- ✅ APM (Application Performance Monitoring) (planned)

---

## 📊 İstatistikler

### Backend
- **Controllers**: 40+
- **Operations**: 200+
- **Services**: 50+
- **Entities**: 60+
- **DTOs**: 100+
- **Migrations**: 30+

### Frontend
- **Pages**: 36+
- **Components**: 73+
- **Services**: 30+
- **Hooks**: 20+
- **Types**: 50+

### Test Coverage
- **Backend**: ~65%
- **Frontend**: ~60%
- **E2E**: ~40%

---

**Dokümantasyon Hazırlayan**: AI Assistant  
**Son Güncelleme**: 2025-01-16  
**Versiyon**: v1.0.0
