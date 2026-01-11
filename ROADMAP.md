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
  - Integration test’ler: auth + temel CRUD smoke (en az 30-40 senaryo).
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
- ✅ **S3.1 — API sözleşmesini sabitle (DONE)**
  - Journal endpoint’leri tüm API’ler gibi **tek response şekline** sahip (frontend’in `data.data` beklentisi ile uyumlu).
  - Swagger’da örnek response’lar netleştirildi.
- ✅ **S3.2 — Journal Post idempotency (DONE)**
  - DB düzeyi garanti: `JournalEntry.InvoiceId` + unique index (InvoiceId null değilken).
  - UI’da “already posted” durumunda Post butonu disabled + mesaj.
- ✅ **S3.3 — Journal Entry detay görüntüleme (DONE)**
  - `GET /api/v1.0/Journal/by-invoice/{invoiceId}` ile `JournalEntryId`, `PostingDate`, `Currency`, `Lines[]` döner.
  - Invoice detail’da “Journal Posted” chip’i tıklanınca JE detay dialog açılır.
- ✅ **S3.4 — GL mapping (hardcode’dan çık) (DONE)**
  - `Accounting:Journal` config’i ile `ReceivableAccountCode`, `DefaultRevenueAccountCode`, `AdjustmentAccountCode`, `RevenueAccountByServiceType` hardcode’dan çıktı.
  - (Opsiyonel sonraki) Admin UI mapping ekranı + validation.
- ✅ **S3.5 — Vergi/KDV modeli (DONE)**
  - Problem: `InvoiceItemEntity` üzerinde KDV oranı/tutarı yok → VAT satırları üretilemiyor.
  - Karar (öneri): **(a)** invoice item’a snapshot alanları ekle:
    - `VatRate` (örn: 0.20), `VatAmount` ve (gerekiyorsa) `NetAmount`/`GrossAmount` netliği.
  - Journal tasarımı:
    - Preview/Post sırasında **KDV satır(lar)ı** üret (örn: `191/391` mantığına uygun hesaplar).
    - KDV hesabı ve hesap kodu config’ten gelsin (örn: `Accounting:Journal:VatPayableAccountCode` + opsiyonel `VatRateByServiceType`).
  - Kabul kriteri:
    - Preview’da KDV satırları görünür ve toplamlar (debit/credit) dengelidir.
    - Post sonrası KDV satırları JE içinde kalıcıdır; aynı invoice tekrar post edilemez.
- ✅ **S3.6 — Export (muhasebe çıktı) (DONE)**
  - ✅ “Journal Export by Date Range” (CSV + Excel) eklendi (API + test).
  - Sonraki: Guest Ledger / Room Ledger / Supplier Cost export.
- **S3.7 — Test kapsamı (NEXT)**
  - Backend integration test:
    - ✅ Preview OK
    - ✅ Post OK
    - ✅ İkinci post 400 + mesaj
    - ✅ Unbalanced post 400
    - ✅ Auth required (401) + role enforcement (403) (Journal + Export)
    - ✅ Post: invalid posting date → 400
    - ✅ Post: empty lines → 400
  - ✅ Frontend Playwright smoke: invoice detail → preview → post → "Journal Posted" görünür.
    - Not: Smoke test mocked auth + mocked API ile çalışır (CI stabilitesi için). Opsiyonel: staging ortamına karşı “real backend” E2E suite.

- **S3.8 — Muhasebe Export’ları (Opsiyonel)**
  - Hedef: Guest Ledger / Room Ledger / Supplier Cost export.
  - En az 1 export: Journal by date range (API + test).

### Sprint 3 Kabul Kriterleri (Done Definition)
- Invoice detail’da **Preview → Post** akışı üretimde kullanılabilir ve tekrar post edilemez.
- Post sonrası JE id/tarih UI’da görünür; JE detayı API’dan okunabilir.
- GL mapping hardcode’dan çıkmış; en az 3 servis tipi için mapping yapılabilir.
- Muhasebe export’larından en az 1 tanesi (Journal by date range) çalışır ve test edilmiştir.

### Notlar / Riskler
- **Çoklu para birimi**: Post edilen JE’nin currency’si ve satır currency’si politikası netleşmeli (tek currency mi, multi-line currency mi?).
- **Rounding/discount**: invoice total ile item sum farkı için adjustment satırı var; muhasebe kuralı netleşmeli.
- **Audit**: “kim post etti” alanı şu an `system`; gerçek kullanıcı (personnel) claim’inden doldurulmalı.

## Sprint 4 (8+ hafta) — Productionization / Go-Live (Operasyon + Ürünleşme)

### Hedef
Projeyi **üretime güvenli şekilde çıkarma** (security + ops + QA gate) ve saha kullanımında operasyonel değeri artırma.

### Go-Live Gate (Çıkış için zorunlu)
- **Build/Test Gate (CI)**:
  - Backend: `dotnet build` + `dotnet test` ✅
    - `dotnet build` **warnings-as-errors** (PR’da yeni warning oluşamaz)
  - Frontend: `npm run build` + `npm run test:ci` ✅
  - Playwright smoke: login + invoice preview/post + export download (min 2–3 kritik akış)
  - **Staging E2E (real backend)**: login → invoice detail → journal preview/post → export + temel CRUD smoke (manual/dispatch veya nightly)
  - Dependency gate: `dotnet list package --vulnerable` + `npm audit` (moderate+ fail)
- **Secrets & Config**:
  - `JWT__SecretKey` zorunlu (prod’da boş olamaz), CORS origins prod domain ile sınırlı
  - `SecurityHeaders:ConnectSrc` prod ortamına göre ayarlı
  - `SeedDemoData=false` prod’da kilitli
- **DB / Migration**:
  - Prod deploy’da migration otomasyonu (release sırasında `dotnet ef database update`)
  - Kritik index’ler doğrulandı (özellikle invoice/journal idempotency index’leri)
- **Logging/Monitoring**:
  - Health endpoints (liveness/readiness) reverse proxy üzerinden doğrulanır
  - Merkezi log hedefi (Seq/ELK) + temel alert (5xx, latency, disk)
- **Backup & DR**:
  - DB backup stratejisi + restore drill (en az 1 kez)

### Kalite & Teknik Borç (yüksek ROI)
- **Build warning cleanup**:
  - Nullability uyarılarını kademeli azalt (hedef: < 20, sonra < 10)
  - EF “shadow FK” / relationship warning’larını temizle (özellikle `TransferEntity` tarafı)
  - Deprecated DTO alanları (`RevenueSummaryDto.TotalRevenue` vb.) refactor
- **Test coverage planı (kademeli)**:
  - Frontend coverage bugün düşük (CI barı var): hedefi sprint sprint artır (örn: 5% → 15% → 30%)
  - Backend: coverlet/coverage raporu + minimum threshold (modül bazlı)
- **Docs hygiene**:
  - `QA_TEST_REPORT.md` içindeki endpoint örnekleri `/api/v1.0/...` ile uyumlu hale getir
  - Deployment dokümanlarını tek bir “source of truth”e bağla (`DEPLOYMENT_CHECKLIST.md`)

### Yapılacaklar (seçmeli)
- OTA entegrasyonlarının gerçek provider API’leri ile hardening’i (retry, idempotency, monitoring).
- Mobil-first operasyon ekranları (özellikle “bugünkü hizmetler”, “yaklaşanlar”, “ödemesi alınmayanlar”).
- Observability: OpenTelemetry trace/metric + dashboard.

### Gelecek Özellikler / Backlog (öncelikli)
- **P0 — Finans/Muhasebe (yüksek ROI)**
  - Multi-currency muhasebe politikası: JE currency & satır currency standardı + rounding kuralları.
  - “Posted by” audit: JE’de `CreatedBy/PostedBy` gerçek kullanıcı claim’inden doldurulsun.
  - VAT raporlama: KDV tahakkuk (391) ve dönem bazlı KDV raporu + export.
  - Journal “unpost/reversal”: üretim politikası netleşsin (reversal entry mi, unpost yasak mı).
- **P0 — Operasyon**
  - Günlük operasyon ekranı: bugün/yaklaşan servisler + risk bayrakları + hızlı aksiyonlar.
  - Bildirim (Notification) kuralları: geciken ödeme, yaklaşan servis, atanmayan şoför vb.
- **P1 — Raporlama**
  - Dashboard’lar: filtrelenebilir rapor ekranları (tarih aralığı, servis tipi, personel).
  - Export paketleri: Guest Ledger, Supplier Ledger, Room Ledger (CSV/Excel).
- **P1 — Entegrasyonlar**
  - OTA webhook idempotency key + retry/backoff + dead-letter kuyruğu.
  - Provider bazlı rate limit ve circuit breaker.
- **P1 — Güvenlik/Compliance**
  - 2FA (Admin/Owner) + brute-force koruması + login audit ekranı.
  - PII yönetimi: veri maskeleme, silme/anonymize (KVKK/GDPR uyumu için).
- **P2 — UX / Platform**
  - Feature flags (staging’de deneme, prod rollout).
  - Role/permission matrisi: UI + API aynı kaynaktan üretilebilir hale gelsin.

## Kabul Kriterleri (Roadmap’in “bitti” sayılması)
- Bu dosya dışında repo kökünde “roadmap/todo/phase” amaçlı **ikinci bir yol haritası dokümanı kalmaması**.
- README’de tek bir yol haritasına yönlendirme olması.
- Sprint 1 sonunda middleware davranışlarının (headers + rate limit) tutarlı ve test edilebilir olması.

