# GuestFlow Frontend - Deployment Guide

Bu dokümantasyon, GuestFlow Frontend uygulamasının deployment süreçlerini açıklar.

## 📋 İçindekiler

- [Prerequisites](#prerequisites)
- [Environment Variables](#environment-variables)
- [Build Process](#build-process)
- [Deployment Options](#deployment-options)
- [CI/CD](#cicd)
- [Docker Deployment](#docker-deployment)

## Prerequisites

- Node.js 18+ ve npm
- Git
- (Opsiyonel) Docker ve Docker Compose
- (Opsiyonel) Azure CLI, AWS CLI, veya FTP client

## Environment Variables

### Development

`.env` dosyası oluşturun:

```bash
cp .env.example .env
```

Gerekli environment variables:

```env
VITE_API_BASE_URL=http://localhost:5146/api/v1.0
VITE_ENV=development
VITE_APP_NAME=GuestFlow
VITE_APP_VERSION=1.0.0
```

### Production

Production için environment variables:

```env
VITE_API_BASE_URL=https://api.guestflow.com/api/v1.0
VITE_ENV=production
VITE_APP_NAME=GuestFlow
VITE_APP_VERSION=1.0.0
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_ERROR_TRACKING=true
```

## Build Process

### Development Build

```bash
npm run dev
```

### Production Build

```bash
npm run build
```

Build çıktısı `dist/` klasöründe oluşturulur.

### Build Analysis

Bundle analizi için:

```bash
npm run build:analyze
```

Analiz raporu `dist/stats.html` dosyasında oluşturulur.

## Deployment Options

### 1. Generic Deployment Script

```bash
chmod +x scripts/deploy.sh
./scripts/deploy.sh production
```

Bu script:
- Dependencies yükler
- Lint ve type check çalıştırır
- Testleri çalıştırır
- Production build oluşturur
- Deployment package hazırlar

### 2. Azure Static Web Apps

```bash
chmod +x scripts/deploy-azure.sh
export AZURE_STATIC_WEB_APPS_API_TOKEN=your-token
./scripts/deploy-azure.sh
```

### 3. AWS S3 + CloudFront

```bash
chmod +x scripts/deploy-aws.sh
./scripts/deploy-aws.sh bucket-name cloudfront-distribution-id
```

### 4. FTP/SFTP

```bash
chmod +x scripts/deploy-ftp.sh
export FTP_HOST=ftp.example.com
export FTP_USER=username
export FTP_PASSWORD=password
export FTP_REMOTE_PATH=/public_html
./scripts/deploy-ftp.sh
```

## CI/CD

### GitHub Actions

Proje GitHub Actions ile CI/CD pipeline içerir:

- **Lint**: ESLint kontrolü
- **Type Check**: TypeScript type kontrolü
- **Test**: Jest unit testleri
- **Build**: Production build
- **E2E**: Playwright E2E testleri
- **Deploy**: Otomatik deployment (main branch için)

Pipeline `.github/workflows/ci.yml` dosyasında tanımlıdır.

### Local CI Simulation

```bash
# Lint
npm run lint

# Type check
npx tsc --noEmit

# Tests
npm run test:ci

# Build
npm run build

# E2E tests
npm run test:e2e
```

## Docker Deployment

### Build Docker Image

```bash
docker build -t guestflow-frontend:latest \
  --build-arg VITE_API_BASE_URL=https://api.guestflow.com/api/v1.0 \
  --build-arg VITE_ENV=production \
  .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

### Environment Variables for Docker

`docker-compose.yml` dosyasını düzenleyin veya environment variables set edin:

```bash
export VITE_API_BASE_URL=https://api.guestflow.com/api/v1.0
docker-compose up -d
```

## Nginx Configuration

Production deployment için `nginx.conf` dosyası hazırdır. Bu configuration:

- SPA routing desteği
- Gzip compression
- Static asset caching
- Security headers
- API proxy (opsiyonel)
- SignalR proxy (opsiyonel)
- Health check endpoint

## Health Check

Uygulama `/health` endpoint'i ile health check sağlar:

```bash
curl http://localhost/health
# Response: healthy
```

## Build Validation

Build'i validate etmek için:

```bash
chmod +x scripts/validate-build.sh
./scripts/validate-build.sh
```

Bu script:
- Build directory'nin varlığını kontrol eder
- Gerekli dosyaların varlığını kontrol eder
- index.html'in geçerliliğini kontrol eder
- Build info'yu gösterir

## Health Check

Uygulamanın sağlığını kontrol etmek için:

```bash
chmod +x scripts/health-check.sh
./scripts/health-check.sh http://localhost/health
```

## Troubleshooting

### Build Fails

1. Node.js versiyonunu kontrol edin: `node --version` (18+ olmalı)
2. Dependencies'i temizleyin: `rm -rf node_modules package-lock.json && npm install`
3. Cache'i temizleyin: `rm -rf dist .vite`
4. TypeScript hatalarını kontrol edin: `npx tsc --noEmit`

### Environment Variables Not Working

1. `.env` dosyasının doğru yerde olduğundan emin olun
2. Environment variable'ların `VITE_` prefix'i ile başladığından emin olun
3. Build sonrası değişiklikler için rebuild yapın

### Docker Build Fails

1. Dockerfile'daki build arguments'ları kontrol edin
2. Multi-stage build'in doğru çalıştığından emin olun
3. Nginx configuration'ı kontrol edin

### Environment Variables Not Working

1. `.env` dosyasının doğru yerde olduğundan emin olun
2. Variable'ların `VITE_` prefix'i ile başladığından emin olun
3. Build sonrası değişiklikler için rebuild yapın

### Docker Build Fails

1. Build arguments'ları kontrol edin
2. Dockerfile'daki stage'leri kontrol edin
3. Network bağlantısını kontrol edin

## Best Practices

1. **Environment Variables**: Production'da asla hardcode etmeyin
2. **Build Optimization**: Production build'de console.log'ları kaldırın
3. **Caching**: Static assets için uygun cache headers kullanın
4. **Security**: HTTPS kullanın ve security headers ekleyin
5. **Monitoring**: Production'da error tracking ve analytics aktif edin
6. **Testing**: Deployment öncesi tüm testleri çalıştırın

## Support

Sorularınız için:
- GitHub Issues: [Create an issue](https://github.com/your-repo/issues)
- Documentation: [README.md](./README.md)

