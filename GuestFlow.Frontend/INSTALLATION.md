# GuestFlow Frontend - Kurulum Talimatları

## Gereksinimler

- Node.js 18+ 
- npm veya yarn

## Kurulum Adımları

### 1. Bağımlılıkları Yükle

```bash
cd GuestFlow.Frontend
npm install
```

### 2. Environment Variables

`.env` dosyası oluşturun (`.env.example` dosyasını kopyalayın):

```bash
cp .env.example .env
```

`.env` dosyasını düzenleyin:

```
VITE_API_BASE_URL=http://localhost:5145/api/v1
```

### 3. Development Server'ı Başlat

```bash
npm run dev
```

Uygulama `http://localhost:5173` adresinde çalışacaktır.

## Kullanılabilir Komutlar

- `npm run dev` - Development server'ı başlat
- `npm run build` - Production build oluştur
- `npm run preview` - Production build'i önizle
- `npm run lint` - ESLint ile kod kontrolü

## Proje Yapısı

```
src/
├── components/        # Reusable UI components
│   └── Layout/       # Layout components (Sidebar, Header)
├── pages/            # Page components
│   ├── Auth/         # Authentication pages
│   └── Dashboard/    # Dashboard page
├── services/         # API services
│   └── api.ts        # Axios instance ve interceptors
├── stores/           # Zustand stores
│   └── authStore.ts  # Authentication state
├── hooks/            # Custom React hooks
├── types/            # TypeScript type definitions
├── utils/            # Utility functions
├── theme/            # MUI theme configuration
├── App.tsx           # Main app component
└── main.tsx          # Entry point
```

## Backend Bağlantısı

Frontend, backend API'ye `http://localhost:5145/api/v1` adresinden bağlanır.

Vite proxy yapılandırması sayesinde development modunda `/api` istekleri otomatik olarak backend'e yönlendirilir.

## İlk Kullanım

1. Backend API'nin çalıştığından emin olun (`http://localhost:5145`)
2. Frontend'i başlatın: `npm run dev`
3. Tarayıcıda `http://localhost:5173` adresine gidin
4. Login sayfasında backend'de kayıtlı bir kullanıcı ile giriş yapın

## Notlar

- Zustand persist middleware kullanılıyor - auth state localStorage'da saklanır
- React Query cache yönetimi için kullanılıyor
- Material-UI (MUI) component library kullanılıyor
- TypeScript strict mode aktif

