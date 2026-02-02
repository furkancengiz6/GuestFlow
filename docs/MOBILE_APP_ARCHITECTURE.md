# GuestFlow Mobile Application Architecture (Sprint 8)

## 1. Vision & Goals

GuestFlow mobil uygulaması, otel personelinin (Concierge, Şoför, Kat Hizmetleri) operasyonel işlemleri sahada, anlık ve çevrimdışı destekle gerçekleştirmesini amaçlar.

## 2. Technology Stack

- **Framework**: React Native (Expo)
- **Language**: TypeScript
- **State Management**: Zustant (Lightweight ve hızlı)
- **Data Fetching**: TanStack Query (React Query)
- **Styling**: NativeWind (Tailwind CSS for React Native)
- **UI Components**: React Native Paper / Tamagui
- **Storage**: AsyncStorage + MMKV (Performans için)
- **Navigation**: React Navigation (Native Stack)

## 3. Core Modules (Phase 1)

### A. Authentication

- JWT based login (Backend ile uyumlu)
- Biometric auth (FaceID/Fingerprint)
- Token refresh mechanism

### B. Daily Operations (Dashboard)

- "Bugünkü İşlerim" listesi
- Yaklaşan transferler
- Önemli notlar ve duyurular

### C. Guest & Reservation

- Misafir arama ve profil görüntüleme
- Check-in/Check-out durum takibi
- QR Kod okuma ile hızlı işlem

### D. Communication

- In-app notifications
- Tek tıkla WhatsApp/Arama başlatma

## 4. Backend Integration

- **Base URL**: Environment bazlı (Dev/Prod)
- **API Client**: Axios (Interceptors ile token yönetimi)
- **Real-time**: SignalR React Native Client (Anlık operasyon güncellemeleri)

## 5. Folder Structure

```text
/src
  /api          # API services & hooks
  /assets       # İkonlar, resimler
  /components   # Shared UI components
  /constants    # Renkler, fontlar, config
  /hooks        # Custom hooks
  /navigation   # Navigation containers
  /screens      # Sayfalar
  /store        # Zustand stores
  /utils        # Helper functions
```

## 6. Success Criteria

- < 1sn sayfa geçiş hızı
- Çevrimdışı (offline) veri görüntüleme desteği
- Push notification gecikmesi < 5sn
