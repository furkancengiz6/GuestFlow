# GuestFlow - Production Deployment Checklist

## 🚀 Pre-Deployment Checklist

### ✅ 1. Environment Setup
- [ ] **Production Environment Variables**
  ```bash
  ASPNETCORE_ENVIRONMENT=Production
  ASPNETCORE_URLS=https://+:443;http://+:80
  ConnectionStrings__DefaultConnection="Server=prod-sql-server;Database=GuestFlow;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=False"
  ConnectionStrings__Redis="prod-redis-server:6379"
  JWT__SecretKey="your-256-bit-secret-key-here-make-it-very-secure"
  JWT__Issuer="https://api.guestflow.com"
  JWT__Audience="https://app.guestflow.com"
  Email__SmtpServer="smtp.gmail.com"
  Email__SmtpPort="587"
  Email__Username="noreply@guestflow.com"
  Email__Password="your-app-password"
  ```

- [ ] **SSL Certificates**
  - [ ] Let's Encrypt certificate for domain
  - [ ] Certificate installed on server
  - [ ] Certificate auto-renewal configured

- [ ] **Domain Configuration**
  - [ ] DNS A record: `api.guestflow.com` → server IP
  - [ ] DNS A record: `app.guestflow.com` → server IP
  - [ ] CDN configuration (CloudFlare/AWS CloudFront)

### ✅ 2. Database Setup
- [ ] **Database Server**
  - [ ] SQL Server 2022 installed and configured
  - [ ] Database created: `GuestFlow`
  - [ ] User permissions configured
  - [ ] Backup strategy implemented

- [ ] **Run Migrations**
  ```bash
  # Production migration
  dotnet ef database update --project GuestFlow.Persistence --environment Production
  ```

- [ ] **Initial Data Seeding**
  - [ ] Admin user created
  - [ ] Basic configuration data
  - [ ] Demo data (optional for production)

### ✅ 3. Infrastructure Setup
- [ ] **Redis Cache**
  - [ ] Redis server installed and running
  - [ ] Persistence configured
  - [ ] Memory limits set

- [ ] **Web Server (Nginx)**
  ```nginx
  # /etc/nginx/sites-available/guestflow
  server {
      listen 80;
      server_name api.guestflow.com;
      return 301 https://$server_name$request_uri;
  }

  server {
      listen 443 ssl http2;
      server_name api.guestflow.com;

      ssl_certificate /etc/letsencrypt/live/api.guestflow.com/fullchain.pem;
      ssl_certificate_key /etc/letsencrypt/live/api.guestflow.com/privkey.pem;

      location / {
          proxy_pass http://localhost:5000;
          proxy_set_header Host $host;
          proxy_set_header X-Real-IP $remote_addr;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Proto $scheme;
      }
  }

  server {
      listen 80;
      server_name app.guestflow.com;
      return 301 https://$server_name$request_uri;
  }

  server {
      listen 443 ssl http2;
      server_name app.guestflow.com;

      ssl_certificate /etc/letsencrypt/live/app.guestflow.com/fullchain.pem;
      ssl_certificate_key /etc/letsencrypt/live/app.guestflow.com/privkey.pem;

      location / {
          root /var/www/guestflow-frontend/dist;
          try_files $uri $uri/ /index.html;

          # Cache static assets
          location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
              expires 1y;
              add_header Cache-Control "public, immutable";
          }
      }

      # API proxy for development
      location /api/ {
          proxy_pass http://localhost:5000/;
          proxy_set_header Host $host;
          proxy_set_header X-Real-IP $remote_addr;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Proto $scheme;
      }
  }
  ```

- [ ] **Firewall Configuration**
  ```bash
  # UFW rules
  sudo ufw allow 22/tcp    # SSH
  sudo ufw allow 80/tcp    # HTTP
  sudo ufw allow 443/tcp   # HTTPS
  sudo ufw allow 6379/tcp  # Redis (internal only)
  sudo ufw --force enable
  ```

### ✅ 4. Application Deployment
- [ ] **Build Applications**
  ```bash
  # Backend
  cd GuestFlow.Api
  dotnet publish -c Release -o /var/www/guestflow-api

  # Frontend
  cd GuestFlow.Frontend
  npm run build:production
  cp -r dist/* /var/www/guestflow-frontend/
  ```

- [ ] **Service Configuration**
  ```bash
  # /etc/systemd/system/guestflow-api.service
  [Unit]
  Description=GuestFlow API
  After=network.target

  [Service]
  Type=simple
  User=www-data
  WorkingDirectory=/var/www/guestflow-api
  ExecStart=/usr/bin/dotnet GuestFlow.Api.dll
  Restart=always
  RestartSec=10
  Environment=ASPNETCORE_ENVIRONMENT=Production
  Environment=ASPNETCORE_URLS=http://localhost:5000

  [Install]
  WantedBy=multi-user.target
  ```

- [ ] **Process Manager**
  ```bash
  sudo systemctl daemon-reload
  sudo systemctl enable guestflow-api
  sudo systemctl start guestflow-api
  sudo systemctl status guestflow-api
  ```

### ✅ 5. Security Configuration
- [ ] **File Permissions**
  ```bash
  sudo chown -R www-data:www-data /var/www/guestflow-*
  sudo chmod -R 755 /var/www/guestflow-*
  ```

- [ ] **Security Headers (Verified)**
  - [ ] Content Security Policy active
  - [ ] HSTS enabled
  - [ ] XSS Protection enabled
  - [ ] Frame Options set

- [ ] **Rate Limiting (Verified)**
  - [ ] API rate limits configured
  - [ ] Login endpoint protection active

- [ ] **Audit Logging (Verified)**
  - [ ] Database migration applied
  - [ ] Audit logs table created
  - [ ] Interceptor configured

### ✅ 6. Monitoring & Logging
- [ ] **Application Logging**
  ```json
  // appsettings.Production.json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "GuestFlow": "Debug"
      }
    }
  }
  ```

- [ ] **Log Aggregation**
  - [ ] Serilog configured
  - [ ] Seq or ELK stack setup
  - [ ] Log retention policy

- [ ] **Health Monitoring**
  ```bash
  # Health checks
  curl https://api.guestflow.com/health
  curl https://api.guestflow.com/health/db
  curl https://api.guestflow.com/health/redis
  ```

### ✅ 7. Backup & Recovery
- [ ] **Database Backup**
  ```bash
  # Daily backup script
  #!/bin/bash
  DATE=$(date +%Y%m%d_%H%M%S)
  BACKUP_DIR="/var/backups/guestflow"
  mkdir -p $BACKUP_DIR

  sqlcmd -S localhost -U sa -P 'YourPassword' -Q "
  BACKUP DATABASE GuestFlow
  TO DISK = '$BACKUP_DIR/guestflow_$DATE.bak'
  WITH FORMAT, MEDIANAME = 'GuestFlow_Backup', NAME = 'Full Backup of GuestFlow';"
  ```

- [ ] **File Backup**
  ```bash
  # Uploaded files backup
  tar -czf $BACKUP_DIR/uploads_$DATE.tar.gz /var/www/guestflow-api/uploads/
  ```

- [ ] **Automated Backups**
  ```bash
  # Crontab
  0 2 * * * /path/to/backup-script.sh  # Daily at 2 AM
  0 3 * * 0 /path/to/weekly-cleanup.sh  # Weekly cleanup
  ```

### ✅ 8. Testing & Verification
- [ ] **API Endpoints**
  ```bash
  # Test all endpoints
  curl -X GET "https://api.guestflow.com/api/health" -H "accept: application/json"
  curl -X POST "https://api.guestflow.com/api/v1.0/auth/login" -H "Content-Type: application/json" -d '{"email":"admin@guestflow.com","password":"password"}'
  ```

- [ ] **Frontend Application**
  ```bash
  # Test frontend loading
  curl -I https://app.guestflow.com/
  curl https://app.guestflow.com/assets/index-*.js # Check if JS loads
  ```

- [ ] **Security Tests**
  ```bash
  # Test XSS protection
  curl -X POST "https://api.guestflow.com/api/guests" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer YOUR_TOKEN" \
    -d '{"firstName":"<script>alert(1)</script>","lastName":"Test"}'

  # Test rate limiting
  for i in {1..20}; do
    curl -X GET "https://api.guestflow.com/api/health" &
  done
  ```

### ✅ 9. Performance Optimization
- [ ] **Bundle Analysis**
  ```bash
  cd GuestFlow.Frontend
  npm run build:analyze
  # Check dist/stats.html for bundle analysis
  ```

- [ ] **Database Optimization**
  ```sql
  -- Check query performance
  SELECT TOP 10
      qs.execution_count,
      qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
      qs.total_logical_reads / qs.execution_count AS avg_logical_reads,
      t.text
  FROM sys.dm_exec_query_stats qs
  CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) t
  ORDER BY qs.total_elapsed_time DESC;
  ```

### ✅ 10. Go-Live Checklist
- [ ] **DNS Propagation**
  - [ ] DNS changes propagated globally
  - [ ] SSL certificates valid

- [ ] **Load Testing**
  ```bash
  # Use tools like Artillery or k6
  artillery quick --count 50 --num 10 https://api.guestflow.com/api/health
  ```

- [ ] **Monitoring Setup**
  - [ ] Application Insights configured
  - [ ] Alert rules set up
  - [ ] Log monitoring active

- [ ] **Rollback Plan**
  - [ ] Previous version backup ready
  - [ ] Database rollback scripts prepared
  - [ ] Rollback procedures documented

---

## 🔍 Post-Deployment Monitoring

### **Key Metrics to Monitor**
- Response times (< 500ms for API calls)
- Error rates (< 1%)
- Database connection pool usage
- Memory and CPU usage
- SSL certificate expiry
- Disk space usage

### **Log Monitoring**
```bash
# Check application logs
sudo journalctl -u guestflow-api -f

# Check nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### **Performance Monitoring**
- Lighthouse scores for frontend
- API response times
- Database query performance
- Cache hit rates

---

## 🚨 Emergency Procedures

### **Rollback Steps**
1. Stop the application: `sudo systemctl stop guestflow-api`
2. Restore previous version: `cp -r /var/www/guestflow-api-backup/* /var/www/guestflow-api/`
3. Restore database: `sqlcmd -S localhost -U sa -P 'password' -Q "RESTORE DATABASE GuestFlow FROM DISK = '/var/backups/guestflow/rollback.bak'"`
4. Start application: `sudo systemctl start guestflow-api`

### **Common Issues & Solutions**
- **High CPU Usage**: Check for infinite loops, optimize queries
- **Memory Leaks**: Restart application, check for object disposal
- **Database Connection Issues**: Check connection string, restart SQL Server
- **SSL Certificate Issues**: Renew certificates, restart nginx

---

## 📞 Support Contacts

- **Technical Lead**: [Name] - [Email] - [Phone]
- **DevOps Engineer**: [Name] - [Email] - [Phone]
- **Security Officer**: [Name] - [Email] - [Phone]
- **Database Admin**: [Name] - [Email] - [Phone]

---

**Deployment Date**: ________
**Deployed By**: ________
**Approved By**: ________
**Rollback Plan Reviewed**: ✅