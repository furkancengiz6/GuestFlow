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
  - ✅ Invoice detail’da “Journal Posted” durumu artık **JE #id + posting date** ile görünür (post sonrası otomatik refresh)

### Sıradaki Yapılması Gerekenler (Önerilen Sıra)
- **S3.1 — API sözleşmesini sabitle**
  - Journal endpoint’leri tüm API’ler gibi **tek response şekline** sahip olsun (frontend’in `data.data` beklentisi ile uyumlu).
  - Swagger dokümantasyonunda örnek response’lar net olsun.
- **S3.2 — Journal Post idempotency (gerçek “muhasebe güvenliği”)**
  - Aynı invoice için ikinci post’u engelleme mevcut; bunu **DB düzeyinde garantiye** al:
    - Öneri A (temiz): `JournalEntry` içine `InvoiceId` alanı ekle + unique index.
    - Öneri B (minimal): `JournalLine.ReferenceId` üzerinden unique constraint / unique index (riskli: satır sayısı çok).
  - UI’da “already posted” durumunda Post butonu görünmesin/disabled + mesaj.
- **S3.3 — Journal Entry detay görüntüleme**
  - `GET /api/v1.0/Journal/by-invoice/{invoiceId}` (veya benzeri) ile:
    - `JournalEntryId`, `PostingDate`, `Currency`, `Lines[]` döndür.
  - Invoice detail’da “Journal Posted” chip’i tıklanınca JE detay dialog aç.
- **S3.4 — GL mapping (hardcode’dan çık)**
  - Şu an `AccountCode` default (`1100`, `4000`, `9999`) — bunu konfig/DB’ye taşı:
    - `ServiceType → RevenueAccountCode`
    - `ReceivableAccountCode`
    - `Rounding/AdjustmentAccountCode`
  - Admin UI: mapping ekranı + validation (boş/invalid code yok).
- **S3.5 — Vergi/KDV modeli**
  - `InvoiceItemEntity` üzerinde KDV oranı/tutarı yok → VAT satırları üretilemiyor.
  - Karar: (a) invoice item’a `VatRate`, `VatAmount` ekle veya (b) servis tablolarından derive et (transfer/tour/restaurant…).
- **S3.6 — Export (muhasebe çıktı)**
  - En küçük deliverable: “Journal Export by Date Range” (CSV/Excel).
  - Sonraki: Guest Ledger / Room Ledger / Supplier Cost export.
- **S3.7 — Test kapsamı**
  - Backend integration test:
    - Preview OK
    - Post OK
    - İkinci post 400 + mesaj
    - Unbalanced post 400
  - Frontend E2E: invoice detail → preview → post → “Journal Posted” görünür.

### Sprint 3 Kabul Kriterleri (Done Definition)
- Invoice detail’da **Preview → Post** akışı üretimde kullanılabilir ve tekrar post edilemez.
- Post sonrası JE id/tarih UI’da görünür; JE detayı API’dan okunabilir.
- GL mapping hardcode’dan çıkmış; en az 3 servis tipi için mapping yapılabilir.
- Muhasebe export’larından en az 1 tanesi (Journal by date range) çalışır ve test edilmiştir.

### Notlar / Riskler
- **Çoklu para birimi**: Post edilen JE’nin currency’si ve satır currency’si politikası netleşmeli (tek currency mi, multi-line currency mi?).
- **Rounding/discount**: invoice total ile item sum farkı için adjustment satırı var; muhasebe kuralı netleşmeli.
- **Audit**: “kim post etti” alanı şu an `system`; gerçek kullanıcı (personnel) claim’inden doldurulmalı.

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

