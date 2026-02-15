# 🏁 GuestFlow — Handover & Deployment Guide

Welcome to **GuestFlow**. This guide is designed to help you transition into ownership of the platform and setup a production-ready environment.

## 🗝 Secret Management

GuestFlow uses a tiered configuration system. **NEVER** commit production secrets to the repository.

### 1. Backend Secrets (`GuestFlow.Api`)

In production, use **Environment Variables** (or Azure Key Vault/AWS Secrets Manager). The following keys are required:

- `ConnectionStrings__DefaultConnection`: Production SQL Server connection string.
- `JWT__SecretKey`: A cryptographically strong string (minimum 32 characters).
- `NEO4J__Password`: Password for the Neo4j Graph DB.
- `OTA__BookingDotCom__Password`: API password for Booking.com integration.
- `PMS__Opera__ApiSecret`: Client secret for Opera Cloud.
- `Stripe__SecretKey`: Stripe live secret key.

### 2. Frontend Secrets (`GuestFlow.Frontend`)

Create a `.env` file based on `env.production.example`:

- `VITE_API_BASE_URL`: The public URL of your deployed API.
- `VITE_STRIPE_PUBLISHABLE_KEY`: Your Stripe live publishable key.

---

## 🚀 Deployment Options

### Docker (Recommended)

The project includes a `docker-compose.yml` that orchestrates:

- **API**: .NET 8 Web API
- **Frontend**: Nginx serving Static React Bundle
- **Database**: SQL Server
- **Graph**: Neo4j
- **Monitoring**: Grafana & Prometheus

Run: `docker-compose up -d --build`

### Manual Deployment (Azure/AWS)

1. **API**: Deploy to Azure App Service or AWS Elastic Beanstalk (Linux/Windows).
2. **Frontend**: Deploy `dist/` folder to Azure Static Web Apps, AWS S3 + CloudFront, or Vercel.
3. **Database**: Use Azure SQL or AWS RDS.
4. **Graph**: Use Neo4j Aura (Cloud) or a managed Neo4j instance.

---

## 📈 Scalability & Architecture

- **Domain-Driven Design (DDD)**: The core logic is isolated in `GuestFlow.Domain` and `GuestFlow.Application`.
- **Background Tasks**: Long-running operations (PMS Sync, PDF Generation) are handled via background workers.
- **Reporting**: Uses `QuestPDF` for high-performance PDF generation.
- **Intelligence Layer**:
  - **Graph Neural Network**: Neo4j-based relationship mapping (VIP networks, staff efficiency patterns).
  - **Automatic Actions**: Proactive interventions triggered by sentiment/behavior analysis.
  - **360° Profiles**: Unified view for Guests and Staff (Performance, Matches).
- **Security**:
  - **WhatsApp**: Webhook signature validation (`X-Hub-Signature-256`) is enforced.
  - **JWT**: Role-based access control (RBAC) for API endpoints.

---

## ✅ Implementation Status (Feb 2026)

- [x] Core PMS Integration (Opera & Mock)
- [x] Intelligence Layer (Graph & Proactive Service)
- [x] Staff Intelligence (Performance & Matching)
- [x] Financial Integration (Folios & Stripe)
- [x] WhatsApp Integration (Templates & Webhooks)

## 📞 Support & Maintenance

- **Health Checks**: Access `/health` and `/api/health` to monitor system status.
- **Logging**: Serilog is configured. In production, sink logs to Seq, ELK, or Azure Log Analytics.

---
*Prepared by Furkan Cengiz — February 2026*
