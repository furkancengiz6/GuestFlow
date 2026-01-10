# GuestFlow — Tek Yol Haritası (Roadmap)

**Amaç**: Bu repo içindeki dağınık/çelişkili dokümanları tek bir “gerçekçi” yol haritasında birleştirmek. Bu dosya, **koddaki mevcut duruma göre** hazırlanmıştır (backend `.NET 8`, frontend `React + TS`).

## Durum Fotoğrafı (Kod Bazlı)

### Backend (GuestFlow.Api)
- **Mimari**: DDD + katmanlı yapı (`GuestFlow.Domain`, `GuestFlow.Application`, `GuestFlow.Persistence`, `GuestFlow.Api`)
- **Güvenlik**:
  - JWT auth + refresh token
  - Rate limiting middleware (dev’de localhost bypass var)
  - Security headers: `SecurityHeadersMiddleware` (**tek kaynak**, duplikasyon yok)
  - Request body HTML sanitization (`Ganss.XSS`)
  - Audit logging (EF interceptor)
- **Operasyonel**:
  - Health check endpoint’leri: `/health`, `/health/ready`, `/health/live`, `/health/detailed`
  - Serilog (console + file + Seq)
  - SignalR hub: `/hubs/notifications`
- **Modüller**: Controller’lar mevcut (örnekler): Guests, Transfers, Tours (City/Yacht), Invoices, Payments, Reservations, Emails/SMS/Notifications, Files, Hotels/Restaurants/Itineraries, Suppliers/SupplierCosts, OTA, Journal, Reports/Dashboard, Currency, Calendar, RoomAssignments.

### Frontend (GuestFlow.Frontend)
- **Stack**: React 18 + TypeScript + Vite + MUI + React Query + Zustand + Zod + Playwright
- **Routing / Sayfalar**: Login, Dashboard, Guests, Transfers, Tours, Invoices, Reservations, Personnel(Admin), Reports(Admin), Settings(Admin), Airports/Cities/Vehicles(Admin), DailyNotes/Admin, DailyRevenues/Admin, SMS/Admin, Emails/Admin, Notifications, Files/Admin, Calendar, Hotels/Admin, Restaurants/Admin, Itineraries, Currency/Admin, ServicePackages, Payments, RoomAssignments, Suppliers/Admin, SupplierCosts/Admin.
- **Prod kalitesi**: Lazy loading + error boundary’ler + token refresh + session timeout.

## Sprint 0 (Hemen) — Dokümantasyon ve Netlik

### Hedef
- Repo içinde **tek yol haritası** bu dosya olsun.
- Çelişkili “tamamlandı/bitmedi” anlatımlarını kaldırıp, kaynak olarak **kod** referans alınsın.

### Yapılacaklar
- **Doküman sadeleştirme**:
  - Roadmap/TODO/Phase türü çakışan dosyaları kaldır (bu dosyaya taşındı).
  - “%100 tamamlandı / 12 sprint bitti” gibi **koddaki durumu aşan** iddiaları içeren dokümanları kaldır.
- **README güncelle**: Tek roadmap linki ver.

## Sprint 1 (1–2 hafta) — Güvenlik ve Middleware Temizliği

### Hedef
Güvenlik katmanını sadeleştirip doğru sıraya oturtmak; sürpriz davranışları azaltmak.

### Yapılacaklar
- **Security headers duplikasyonunu kaldır**: ✅ (tek kaynak: `SecurityHeadersMiddleware`)
- **Rate limit “BlockedUserAgents” yeniden değerlendir**:
  - ✅ Production-only UA blocking + Postman/curl QA/dev akışını bozmayacak şekilde düzenlendi.
- **CSP / connect-src**:
  - ✅ `SecurityHeaders` config’i eklendi (connect-src env bazlı yönetilebilir + ws/wss + dev localhost uyumu).

## Sprint 2 (2–4 hafta) — Kalite, Test, CI Disiplini

### Hedef
Regresyon riskini düşürmek ve release sürecini otomatikleştirmek.

### Yapılacaklar
- **Backend**:
  - Integration test’ler: auth + temel CRUD smoke (en az 5-10 senaryo).
  - Migration’lar için CI “smoke” (DB migrate + basic request) akışı.
- **Frontend**:
  - Jest + RTL smoke: auth store, critical pages (Dashboard/Guests/Transfers) en azından render + basic happy-path.
  - Playwright: login + 2 kritik akış (Guests list, Transfers list) CI’da stabil çalışsın.

### Tamamlananlar (Mevcut Durum)
- **CI**:
  - Backend build+test job ✅
  - Frontend Playwright smoke job ✅
  - Frontend Jest unit job ✅
- **Frontend Jest stabilizasyonu**:
  - Ignored/flaky testler geri açıldı (ExportButton / ProtectedRoute / TransferForm / auth integration) ✅
  - `test:ci` Windows-safe hale getirildi (coverage pattern quoting sorunu giderildi) ✅
  - Baseline coverage threshold eklendi (kademeli artırmak için) ✅

## Sprint 3 (4–8 hafta) — Finans / Muhasebe (Mevcut Temele Dayalı)

### Hedef
Mevcut `Journal` ve `SupplierCost/Profitability` altyapısını gerçek operasyonel akışa dönüştürmek.

### Yapılacaklar
- **Invoice → Journal Preview → Post** akışını netleştir (UI + API):
  - “Önizleme” zorunlu, otomatik post yok.
- **GL mapping / şablonlar**:
  - Hizmet tipi bazlı hesap planı eşleştirmesi (Transfer/Tour/Restaurant/Package).
- **Export**:
  - Muhasebe için CSV/Excel export (Room Ledger / Guest Ledger gibi çıktılar).

### Tamamlananlar (Mevcut Durum)
- **API (Journal)**:
  - ✅ Versioned route: `GET /api/v1.0/Journal/preview?invoiceId=...`, `POST /api/v1.0/Journal/post` (roles: Staff/Admin)
  - ✅ Post guard’ları: aynı invoice için tekrar post engeli + debit/credit balance kontrolü
- **Backend altyapı**:
  - ✅ `IUnitOfWork` artık `JournalLines` repository’sini expose ediyor
- **Frontend (Invoices)**:
  - ✅ `InvoiceDetailPage` üzerinde “Journal Preview” butonu + satır/total gösteren dialog

## Sprint 4 (8+ hafta) — Operasyon, Entegrasyonlar, Ürünleşme

### Hedef
Gerçek saha kullanımında operasyonel değer yaratacak adımlar (opt-in).

### Yapılacaklar (seçmeli)
- OTA entegrasyonlarının gerçek provider API’leri ile hardening’i (retry, idempotency, monitoring).
- Mobil-first operasyon ekranları (özellikle “bugünkü hizmetler”, “yaklaşanlar”, “ödemesi alınmayanlar”).
- Observability: OpenTelemetry trace/metric + dashboard.

## Kabul Kriterleri (Roadmap’in “bitti” sayılması)
- Bu dosya dışında repo kökünde “roadmap/todo/phase” amaçlı **ikinci bir yol haritası dokümanı kalmaması**.
- README’de tek bir yol haritasına yönlendirme olması.
- Sprint 1 sonunda middleware davranışlarının (headers + rate limit) tutarlı ve test edilebilir olması.

