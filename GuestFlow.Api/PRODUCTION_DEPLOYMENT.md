# GuestFlow API - Production Deployment Guide

> **⚠️ ÖNEMLİ**: Bu doküman referans amaçlıdır. Güncel ve detaylı deployment bilgileri için **[DEPLOYMENT_CHECKLIST.md](../../DEPLOYMENT_CHECKLIST.md)** dosyasını kullanın.
>
> **Source of Truth**: `DEPLOYMENT_CHECKLIST.md` - Tüm deployment adımları ve checklist'ler bu dosyada tutulmaktadır.

## Prerequisites

- .NET 8.0 Runtime installed
- SQL Server database (Azure SQL, AWS RDS, or on-premises)
- SMTP server for email notifications
- Reverse proxy (nginx, IIS, or cloud load balancer)
- SSL certificate configured

## Environment Setup

1. **Copy environment configuration:**
   ```bash
   cp env.production.example .env
   ```

2. **Configure environment variables:**
   - Edit `.env` file with your production values
   - Set database connection string
   - Generate and set JWT secret key
   - Configure CORS origins
   - Set up SMTP settings

## Database Setup

1. **Create production database:**
   ```sql
   CREATE DATABASE GuestFlowDb;
   ```

2. **Run migrations:**
   ```bash
   dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
   ```

3. **Optional: Seed initial data:**
   - Set `SeedDemoData=true` temporarily
   - Start the application once to seed data
   - Set `SeedDemoData=false` afterwards

## Application Deployment

1. **Build for production:**
   ```bash
   dotnet publish GuestFlow.Api --configuration Release --output ./publish
   ```

2. **Configure reverse proxy:**
   - Set up nginx/IIS to proxy to Kestrel
   - Configure SSL termination
   - Set appropriate headers

3. **Start the application:**
   ```bash
   cd publish
   dotnet GuestFlow.Api.dll
   ```

## Health Checks

After deployment, verify:

- **Health endpoints:**
  - `GET /health` - Overall health
  - `GET /health/ready` - Readiness check
  - `GET /health/live` - Liveness check

- **Database connectivity:**
  - Health check should report database as healthy

- **Authentication:**
  - Test login endpoint
  - Verify JWT token generation

## Security Checklist

- [ ] JWT secret key is unique and secure (256-bit minimum)
- [ ] Database connection uses secure credentials
- [ ] CORS origins are restricted to your domains
- [ ] Rate limiting is enabled
- [ ] HTTPS is enforced
- [ ] No sensitive data in logs

## Monitoring Setup

1. **Enable structured logging:**
   - Configure Seq server URL in environment variables
   - Or set up alternative logging sink

2. **Set up health monitoring:**
   - Configure load balancer health checks
   - Set up alerts for health endpoint failures

## Troubleshooting

### Common Issues

**400 Bad Request - Invalid Hostname:**
- Check AllowedHosts configuration
- Ensure reverse proxy forwards correct Host header

**Database Connection Failed:**
- Verify connection string
- Check firewall settings
- Ensure SQL Server allows connections

**JWT Authentication Failed:**
- Verify JWT secret key is set
- Check token expiration settings
- Ensure issuer/audience match

**Health Checks Failing:**
- Verify database connectivity
- Check Redis connection (if enabled)
- Review health check logs

## Performance Tuning

**Production Settings:**
- Rate limits are stricter than development
- Cache durations are longer
- Logging level is set to Warning
- Token expiration is shorter for security

**Scaling Considerations:**
- Configure Redis for distributed caching in multi-instance deployments
- Set up database connection pooling
- Configure appropriate worker process limits