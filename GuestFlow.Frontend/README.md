# GuestFlow Frontend

GuestFlow Misafir Yönetim Sistemi - Frontend Uygulaması

## Teknoloji Stack

- **React 18+** - UI Framework
- **TypeScript** - Type Safety
- **Vite** - Build Tool
- **Material-UI (MUI)** - UI Component Library
- **React Query** - Server State Management
- **React Router v6** - Routing
- **Axios** - HTTP Client
- **React Hook Form** - Form Management
- **Zustand** - Global State Management

## Kurulum

```bash
# Bağımlılıkları yükle
npm install

# Development server'ı başlat
npm run dev

# Production build
npm run build

# Preview production build
npm run preview
```

## E2E Test (Playwright)

```bash
# Ortam değişkenleri (gerekirse)
# set E2E_BASE_URL=http://localhost:5173
# set E2E_USER_EMAIL=admin@example.com
# set E2E_USER_PASSWORD=Admin123!

npm install
npx playwright install --with-deps
npm run test:e2e
```

## Proje Yapısı

```
src/
├── components/     # Reusable components
├── pages/          # Page components
├── services/       # API services
├── hooks/          # Custom hooks
├── stores/         # Zustand stores
├── types/          # TypeScript types
├── utils/          # Utility functions
├── theme/          # MUI theme configuration
└── App.tsx         # Main app component
```

## API Endpoint

Backend API: `http://localhost:5145/api/v1`

Vite proxy yapılandırması sayesinde `/api` istekleri otomatik olarak backend'e yönlendirilir.

