# 🧪 GuestFlow Testing Guide

Bu rehber, GuestFlow projesini kapsamlı şekilde test etmek için gerekli tüm yaklaşımları ve komutları içerir.

## 🎯 Test Stratejisi

GuestFlow projesi aşağıdaki test katmanlarını destekler:

1. **Unit Tests** - Backend iş mantığı
2. **Integration Tests** - API endpoints ve veritabanı
3. **Frontend Tests** - React bileşenleri
4. **E2E Tests** - Tam kullanıcı akışları
5. **Performance Tests** - Yük testi
6. **Security Tests** - Güvenlik açıkları
7. **Docker Tests** - Container testi

## 📦 1. Backend Build Testi

```bash
# Release modunda derleme
dotnet build GuestFlow.Api --configuration Release --verbosity minimal

# Başarılı ise: ✅ Backend build successful!
# Başarısız ise: ❌ Backend build failed!
```

## 🌐 2. Frontend Build Testi

```bash
# Frontend dizinine git
cd GuestFlow.Frontend

# Production build
npm run build

# Ana dizine dön
cd ..

# Başarılı ise: ✅ Frontend build successful!
```

## 🧪 3. Unit Testler (Backend)

```bash
# Tüm unit testleri çalıştır
dotnet test GuestFlow.Application.Tests --verbosity normal

# Kod kapsamı ile birlikte
dotnet test GuestFlow.Application.Tests --collect:"XPlat Code Coverage"

# Mevcut Testler:
# - DashboardServiceTests
# - InMemoryCacheServiceTests
# - InputValidationServiceTests
# - AuthResponseSecurityTests
```

### Unit Test Yapısı:
```
GuestFlow.Application.Tests/
├── Operations/
│   ├── Caching/
│   │   └── InMemoryCacheServiceTests.cs
│   ├── Dashboard/
│   │   └── DashboardServiceTests.cs
│   └── Validation/
│       └── InputValidationServiceTests.cs
└── Helpers/
    ├── TestBase.cs
    └── TestDataBuilder.cs
```

## ⚛️ 4. Frontend Unit Testler

```bash
# Frontend dizinine git
cd GuestFlow.Frontend

# Tüm testleri çalıştır
npm test -- --watchAll=false

# Kod kapsamı raporu
npm run test:coverage

# Mevcut Testler:
# - Auth ProtectedRoute
# - Common ExportButton
# - Transfers TransferForm
# - Hooks useNotification
# - Utils formatters & validation
```

### Frontend Test Yapısı:
```
GuestFlow.Frontend/src/__tests__/
├── components/
│   ├── Auth/
│   ├── Common/
│   └── Transfers/
├── hooks/
├── services/
└── utils/
```

## 🔗 5. API Integration Testleri

```bash
# Backend'i başlat (test için)
$env:JWT__SecretKey = "MySuperSecretKeyThatIsAtLeast64CharactersLongForSecurityPurposes12345678901234567890"
dotnet run --project GuestFlow.Api --configuration Release --urls "http://localhost:5146"

# Health check
curl http://localhost:5146/health

# Auth endpoint testi
curl -X POST http://localhost:5146/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"wrong"}'

# Dashboard endpoint testi
curl http://localhost:5146/api/dashboard/quick-stats \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🎭 6. E2E (End-to-End) Testler

```bash
# Frontend dizinine git
cd GuestFlow.Frontend

# Tüm E2E testleri çalıştır
npx playwright test

# Headed mode (browser görünür)
npx playwright test --headed

# Belirli bir test
npx playwright test tests/e2e/auth.spec.ts

# HTML rapor
npx playwright show-report
```

### Playwright Konfigürasyonu:
- **Browser'lar**: Chromium, Firefox, Safari
- **Base URL**: http://localhost:5173
- **Video Recording**: Başarısız testlerde
- **Screenshot**: Başarısız testlerde

## ⚡ 7. Performance Testleri

```bash
# k6 yükleme (gerekirse)
choco install k6

# Load test çalıştırma
k6 run tests/performance/load-test.js

# Özel parametrelerle
k6 run tests/performance/load-test.js \
  --vus 50 \
  --duration 2m \
  --out json=results.json

# Test Senaryosu:
# - 50 kullanıcıya kadar ramp-up
# - 100 kullanıcıda 5 dakika sabit yük
# - Response time < 500ms (95th percentile)
# - Success rate > 95%
```

### Performance Test Yapısı:
```
tests/performance/
└── load-test.js          # k6 script
```

## 🔒 8. Security Testleri

```bash
# Rate limiting testi
for i in {1..15}; do
  curl -X POST http://localhost:5146/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com","password":"wrong"}'
done

# JWT olmadan erişim testi
curl http://localhost:5146/api/dashboard/quick-stats
# Expected: 401 Unauthorized

# Geçersiz token testi
curl http://localhost:5146/api/dashboard/quick-stats \
  -H "Authorization: Bearer invalid_token"
# Expected: 401 Unauthorized

# SQL injection testi
curl -X POST http://localhost:5146/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin'; DROP TABLE users;--","password":"pass"}'
# Expected: Sanitized input, no injection
```

## 🐳 9. Docker Testleri

```bash
# Docker image build
docker build -t guestflow-test GuestFlow.Api/

# Container çalıştırma
docker run -d -p 5147:5000 --name guestflow-container guestflow-test

# Health check testi
docker exec guestflow-container curl -f http://localhost:5000/health

# Log kontrolü
docker logs guestflow-container

# Temizlik
docker stop guestflow-container
docker rm guestflow-container
docker rmi guestflow-test
```

## 📊 10. CI/CD Pipeline Testi

```bash
# GitHub Actions workflow (mevcut)
# .github/workflows/ci-cd.yml

# Local CI/CD simülasyonu
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
npm install
npm run build
npm test
npx playwright install
npx playwright test
```

## 🛠️ Test Araçları ve Teknolojileri

### Backend Testing:
- **xUnit** - Test framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework
- **coverlet** - Code coverage

### Frontend Testing:
- **Jest** - Test runner
- **React Testing Library** - Component testing
- **jsdom** - DOM environment

### E2E Testing:
- **Playwright** - Browser automation
- **Multi-browser support** - Chromium, Firefox, WebKit

### Performance Testing:
- **k6** - Load testing tool
- **Custom scenarios** - Ramp-up, steady load
- **Thresholds** - Response time, error rate

## 📋 Test Coverage Hedefleri

```
✅ Backend Unit Tests: 70%+ coverage
✅ Frontend Unit Tests: 60%+ coverage
✅ API Integration: Core endpoints covered
✅ E2E Tests: Critical user flows
✅ Performance: Load testing implemented
✅ Security: Basic security tests
✅ Docker: Container testing
```

## 🚀 Hızlı Test Komutları

```bash
# Tüm backend testleri
./run-tests.bat

# Sadece unit testler
dotnet test GuestFlow.Application.Tests

# Sadece frontend testler
cd GuestFlow.Frontend && npm test

# Performance testi
k6 run tests/performance/load-test.js

# E2E testler
cd GuestFlow.Frontend && npx playwright test
```

## 📊 Test Raporları

### Backend:
- Unit test results: Console output
- Code coverage: `coverage/` directory

### Frontend:
- Test results: Console output
- Coverage: `coverage/lcov-report/index.html`

### Performance:
- k6 results: Console output
- JSON export: `results.json`
- HTML report: Auto-generated

### E2E:
- Playwright report: `playwright-report/index.html`
- Screenshots: `test-results/` directory

## 🔧 Troubleshooting

### Unit Test Sorunları:
```bash
# Cache temizleme
dotnet clean
dotnet restore

# Tekrar çalıştırma
dotnet test --verbosity detailed
```

### Frontend Test Sorunları:
```bash
# Node modules yeniden yükleme
rm -rf node_modules package-lock.json
npm install

# Test çalıştırma
npm test -- --resetMocks
```

### E2E Test Sorunları:
```bash
# Browser yeniden yükleme
npx playwright install

# Debug mode
npx playwright test --debug
```

## 📈 Sürekli İyileştirme

1. **Test Coverage** artırın
2. **Performance benchmark'ları** ekleyin
3. **Visual regression testing** ekleyin
4. **API contract testing** implement edin
5. **Chaos engineering** testleri ekleyin

---

Bu testing guide'ı kullanarak GuestFlow projesini kapsamlı şekilde test edebilirsiniz. Her test katmanı farklı bir risk alanını kapsar ve birlikte production-ready bir sistem sağlar.