# GuestFlow Quality Engineering & Testing Guide

This comprehensive guide outlines the multi-layered testing strategy for GuestFlow, ensuring the "Tourism Operations Intelligence Layer" remains robust, secure, and high-performing.

---

## ⚡ Quick Start: Zero-Touch Verification

### Run Core Suite (Windows)

```powershell
cd C:\GuestFlow
.\run-tests.bat
```

*This command executes Backend build, Backend unit tests, Frontend build, and Frontend Jest tests.*

### Global Health Check (Playwright + Integration)

```powershell
cd C:\GuestFlow
$env:RUN_E2E="true"
.\test-all.ps1
```

---

## 🎯 Strategic Test Pyramid

| Tier | Focus | Tooling |
| :--- | :--- | :--- |
| **E2E / UAT** | Business Flow & UX | Playwright |
| **Integration** | Service Interaction & API | xUnit + HttpClient |
| **Unit (FE)** | Component Logic | Jest + RTL |
| **Unit (BE)** | Business Rules & Logic | xUnit + Moq |

---

## 📦 1. Core Verification (Static & Unit)

### Backend Services

```bash
dotnet build GuestFlow.Api --configuration Release
dotnet test GuestFlow.Application.Tests --collect:"XPlat Code Coverage"
```

- **Key Suites**: Dashboard logic, validation engines, cache providers.
- **Goal**: 70%+ Code Coverage.

### Frontend Components

```bash
cd GuestFlow.Frontend
npm run build
npm run test:coverage
```

- **Key Suites**: Navigation guards, shared UI components, state management (Zustand).
- **Goal**: 60%+ Code Coverage.

---

## 🎭 2. Integration & E2E Excellence

### Playwright End-to-End

```bash
cd GuestFlow.Frontend
npx playwright test
```

- **Scenario Discovery**: Login sequences, Transfer creation, Invoice generation flows.
- **Visuals**: Automated screenshots and video recording on failure.

### Staging Smoke Tests (Go-Live Gate)

Tests against non-mocked, live staging environments to verify real-world connectivity (DB, PMS Sync, Storage).

```bash
npm run test:e2e:staging
```

---

## ⚡ 3. Advanced Quality Assurance

### Performance & Load (k6)

Ensures the platform can handle peak holiday season traffic.

```bash
k6 run tests/performance/load-test.js --vus 100 --duration 5m
```

- **Thresholds**: 95th percentile latency < 500ms; Success rate > 99%.

### Security & Compliance

- **Rate Limit Verification**: Scripts to test brute-force protection on Auth endpoints.
- **PII Sanitization Checks**: Automated verification of data masking in logs.
- **XSS/SQLi Probes**: Input sanitization tests via `Ganss.XSS`.

---

## 📊 Quality Reporting & CI/CD

GuestFlow utilizes **GitHub Actions** for continuous quality monitoring.

- **Pipeline**: `.github/workflows/ci-cd.yml`
- **Artifacts**:
  - Backend Coverage (lcov)
  - Playwright HTML Reports
  - Performance Benchmarks

---

## 🔧 Troubleshooting Guides

### Environment Reset

```bash
dotnet clean
rm -rf node_modules
npm install
```

### Playwright Debugging

```bash
npx playwright test --debug
```

*This guide ensures GuestFlow maintains a production-ready posture at all times.*
