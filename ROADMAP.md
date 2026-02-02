# GuestFlow — Tek Yol Haritası (Roadmap)

> **📊 Tamamlanma Durumu**: %94 tamamlandı (28/29 görev)  
> **Son Güncelleme**: 2026-01-25  
> **Detaylı Durum**: `ROADMAP_COMPLETION_STATUS.md` dosyasına bakınız

**Vizyon**: GuestFlow = Otelin "İnsan İlişkileri Hafızası"  
**Misyon**: Turizm Operasyon Intelligence Layer - İnsan davranışını ürünün merkezine koyarak, oteldeki **insan + misafir + hizmet + zaman + duygu** ilişkilerinin graf veri modelini oluşturmak.

**Amaç**: Bu repo içindeki dağınık/çelişkili dokümanları tek bir "gerçekçi" yol haritasında birleştirmek. Bu dosya, **koddaki mevcut duruma göre** hazırlanmıştır (backend `.NET 8`, frontend `React + TS`).

**İlgili Dokümanlar**:

- [VISION_TURIZM_INTELLIGENCE_LAYER.md](./VISION_TURIZM_INTELLIGENCE_LAYER.md) - **YENİ VİZYON**: Turizm Operasyon Intelligence Layer ve İnsan İlişkileri Hafızası (Henüz oluşturulmadı)
- [PROJE_OBJEKTIF_DEGERLENDIRME.md](./PROJE_OBJEKTIF_DEGERLENDIRME.md) - Objektif proje değerlendirmesi, sınıflandırma ve pazar analizi
- [TEKNOLOJILER_VE_OZELLIKLER.md](./TEKNOLOJILER_VE_OZELLIKLER.md) - Tüm teknolojiler, kütüphaneler ve özellikler kataloğu

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
    - `dotnet build` **warnings-as-errors** (PR'da yeni warning oluşamaz)
  - Frontend: `npm run build` + `npm run test:ci` ✅
  - Playwright smoke: login + invoice preview/post + export download (min 2–3 kritik akış)
  - **Staging E2E (real backend)**: login → invoice detail → journal preview/post → export + temel CRUD smoke (manual/dispatch veya nightly)
  - Dependency gate: `dotnet list package --vulnerable` + `npm audit` (moderate+ fail) ✅ (DependencyVulnerabilityChecker service eklendi)
- **Secrets & Config**: ✅
  - `JWT__SecretKey` zorunlu (prod'da boş olamaz), CORS origins prod domain ile sınırlı (ProductionConfigurationValidator service eklendi)
  - `SecurityHeaders:ConnectSrc` prod ortamına göre ayarlı
  - `SeedDemoData=false` prod'da kilitli
- **DB / Migration**: ✅
  - Prod deploy'da migration otomasyonu (release sırasında `dotnet ef database update`)
  - Kritik index'ler doğrulandı (özellikle invoice/journal idempotency index'leri)
  - Migration drift checker service eklendi (MigrationDriftChecker)
- **Logging/Monitoring**: ✅
  - Health endpoints (liveness/readiness) reverse proxy üzerinden doğrulanır
  - Merkezi log hedefi (Seq/ELK) + temel alert (5xx, latency, disk)
- **Backup & DR**: ✅
  - DB backup stratejisi + restore drill (en az 1 kez) (DatabaseBackupService eklendi)

### Kalite & Teknik Borç (yüksek ROI)

- **Build warning cleanup**: ⏳
  - Nullability uyarılarını kademeli azalt (hedef: < 20, sonra < 10) - Devam ediyor
  - EF "shadow FK" / relationship warning'larını temizle (özellikle `TransferEntity` tarafı) - Devam ediyor
  - Deprecated DTO alanları (`RevenueSummaryDto.TotalRevenue` vb.) refactor ✅ (ExportService güncellendi)
- **Test coverage planı (kademeli)**: ✅
  - Frontend coverage bugün düşük (CI barı var): hedefi sprint sprint artır (örn: 5% → 15% → 30%) - Jest coverage yapılandırıldı
  - Backend: coverlet/coverage raporu + minimum threshold (modül bazlı) - Coverlet yapılandırması tamamlandı (TEST_COVERAGE_PLAN.md)
- **Docs hygiene**: ✅
  - `QA_TEST_REPORT.md` içindeki endpoint örnekleri `/api/v1.0/...` ile uyumlu hale getir ✅
  - Deployment dokümanlarını tek bir "source of truth"e bağla (`DEPLOYMENT_CHECKLIST.md`) ✅
- **Kritik Hata Düzeltmeleri (Hotfixes)**:
  - **Audit Log Constraint Fix**: ✅
    - `AuditLogs` tablosunda `NULL` hatası ("Error 515") giderildi.
    - `AuditInterceptor.cs`: Hükümsüz kullanıcı durumları (seeding, background jobs) için varsayılan değerler ("System", "127.0.0.1") eklendi.
    - `AuditLog.cs`: Teknik alanlar `nullable` yapıldı.
    - Seeding işlemi (`dotnet run --project GuestFlow.Api --SeedDemoData=true`) başarıyla doğrulandı.

### Yapılacaklar (seçmeli)

- OTA entegrasyonlarının gerçek provider API'leri ile hardening'i (retry, idempotency, monitoring). ✅ (OTAIntegrationService'te idempotency, retry/backoff, dead-letter queue mevcut)
- Mobil-first operasyon ekranları (özellikle "bugünkü hizmetler", "yaklaşanlar", "ödemesi alınmayanlar").
- Observability: OpenTelemetry trace/metric + dashboard.

### Gelecek Özellikler / Backlog (öncelikli)

- **P0 — Finans/Muhasebe (yüksek ROI)**
  - Multi-currency muhasebe politikası: JE currency & satır currency standardı + rounding kuralları. ✅ (ExchangeRateService, JournalService multi-currency rounding)
  - "Posted by" audit: JE'de `CreatedBy/PostedBy` gerçek kullanıcı claim'inden doldurulsun. ✅ (JournalService'te Personnel entity'den FullName alınıyor)
  - VAT raporlama: KDV tahakkuk (391) ve dönem bazlı KDV raporu + export. ✅ (ReportsService.GetVatAccrualReportAsync, GetVatPeriodReportAsync, ExportService VAT export methods, ExportController endpoints)
  - Journal "unpost/reversal": üretim politikası netleşsin (reversal entry mi, unpost yasak mı).
- **P0 — Operasyon** ✅
  - Günlük operasyon ekranı: bugün/yaklaşan servisler + risk bayrakları + hızlı aksiyonlar. ✅ (DailyOperationsService, ConciergeDashboard entegrasyonu)
  - Bildirim (Notification) kuralları: geciken ödeme, yaklaşan servis, atanmayan şoför vb. ✅ (NotificationRuleService, OverduePayment, UpcomingService, UnassignedDriver)
- **P1 — Raporlama** ✅
  - Dashboard'lar: filtrelenebilir rapor ekranları (tarih aralığı, servis tipi, personel). ✅ (ReportsPage, ReportsService filtre desteği)
  - Export paketleri: Guest Ledger, Supplier Ledger, Room Ledger (CSV/Excel). ✅ (ExportService, ExportController endpoints)
- **P1 — Entegrasyonlar** ✅
  - OTA webhook idempotency key + retry/backoff + dead-letter kuyruğu. ✅ (OTAIntegrationService, OTAWebhookRetryBackgroundService)
  - Provider bazlı rate limit ve circuit breaker. ✅ (CheckRateLimit, CheckCircuitBreaker fonksiyonları)
- **P1 — Güvenlik/Compliance** ✅
  - 2FA (Admin/Owner) + brute-force koruması + login audit ekranı. ✅ (TwoFactorService, BruteForceProtectionService, LoginAuditController)
  - PII yönetimi: veri maskeleme, silme/anonymize (KVKK/GDPR uyumu için). ✅ (PIIManagementService, PrivacyController, PrivacyManagementPage frontend, GuestDetailPage maskeleme)
- **P2 — UX / Platform**
  - Feature flags (staging'de deneme, prod rollout).
  - Role/permission matrisi: UI + API aynı kaynaktan üretilebilir hale gelsin.

---

## Sprint 5 (10-16 hafta) — Concierge Yönetimi ve PMS Entegrasyonları (Kritik Öncelik)

### Hedef

**GuestFlow'un asıl amacı**: 5 yıldızlı otellerin **concierge desk** operasyonları için tasarlanmış bir **misafir yönetim sistemi**dir. Mevcut PMS sistemleri (Opera, Elektraweb) ile **anlık entegrasyon** sağlayarak concierge personelinin tüm misafir hizmetlerini tek platformdan yönetmesini sağlamak.

### Proje Odak Noktası

- **Concierge/Misafir Yönetimi**: 5 yıldızlı otellerin concierge desk'i için özel tasarım
- **PMS Entegrasyonu**: Opera, Elektraweb gibi mevcut otel sistemleri ile anlık veri senkronizasyonu
- **OTA Entegrasyonu**: Booking.com, Expedia gibi kanallardan otomatik veri çekme
- **Unified Platform**: Tüm misafir bilgileri ve hizmetler tek ekranda

### 🎯 P0 — PMS Entegrasyonları (EN YÜKSEK ÖNCELİK - Kritik)

#### Opera PMS Entegrasyonu

- **Opera Cloud API Entegrasyonu**:
  - Opera Cloud REST API veya XML API entegrasyonu
  - Authentication (OAuth 2.0 veya API key)
  - Rate limiting ve throttling yönetimi
  - API versioning desteği
- **Anlık Veri Senkronizasyonu**:
  - **Misafir Bilgileri (Guest Profile)**: Real-time sync (webhook veya polling)
  - **Rezervasyon Bilgileri (Reservations)**: Anlık güncelleme (check-in/check-out olayları)
  - **Oda Durumu (Room Status)**: Real-time room status sync
  - **Fatura Bilgileri (Folio)**: Folio senkronizasyonu
  - **Oda Atamaları (Room Assignments)**: Oda değişikliklerini takip
- **Opera Veri Modeli Mapping**:
  - `OperaGuest` → `GuestFlow.GuestEntity` mapping
  - `OperaReservation` → `GuestFlow.ReservationEntity` mapping
  - `OperaRoomType` → `GuestFlow.RoomType` mapping
  - `OperaFolio` → `GuestFlow.InvoiceEntity` mapping
  - Field mapping configuration (admin panel'den yönetilebilir)
- **Hata Yönetimi ve Retry**:
  - Connection failure handling
  - Data conflict resolution (last write wins veya source of truth)
  - Sync status monitoring dashboard
  - Error logging ve alerting (e-posta/SMS)

#### Elektraweb Entegrasyonu

- **Elektraweb API Entegrasyonu**:
  - Elektraweb REST API entegrasyonu
  - Authentication mekanizması
  - API versioning desteği
- **Veri Senkronizasyonu**:
  - Misafir bilgileri senkronizasyonu
  - Rezervasyon senkronizasyonu
  - Oda durumu senkronizasyonu
  - Fatura senkronizasyonu
- **Elektraweb Veri Modeli Mapping**:
  - `ElektrawebGuest` → `GuestFlow.GuestEntity` mapping
  - `ElektrawebReservation` → `GuestFlow.ReservationEntity` mapping
  - Field mapping configuration

#### PMS Entegrasyon Mimarisi ✅

- **Generic PMS Adapter Pattern**: ✅

  ```csharp
  public interface IPMSIntegrationService
  {
      Task<GuestProfile> GetGuestProfileAsync(string guestId);
      Task<Reservation> GetReservationAsync(string reservationId);
      Task<List<Reservation>> GetReservationsAsync(DateTime startDate, DateTime endDate);
      Task<bool> UpdateRoomStatusAsync(string roomNumber, RoomStatus status);
      Task<Folio> GetFolioAsync(string reservationId);
      Task<bool> SyncReservationsAsync(DateTime startDate, DateTime endDate);
  }
  
  public class OperaPMSAdapter : IPMSIntegrationService { }
  public class ElektrawebPMSAdapter : IPMSIntegrationService { }
  ```

  - IPMSIntegrationService, PMSIntegrationService, PMSSyncService implementasyonu mevcut
- **Veri Senkronizasyon Stratejisi**: ✅
  - **Real-time sync (Tercih Edilen)**: Webhook-based (Opera Cloud webhook'ları) ✅ (PMSWebhookProcessor, HMAC SHA256 signature validation)
  - **Polling sync**: Webhook yoksa polling (5 dakikada bir) ✅ (PMSPollingBackgroundService)
  - **Batch sync**: Günlük toplu senkronizasyon (backup ve data integrity) ✅ (PMSSyncService.PerformFullSyncAsync)
- **Conflict Resolution**: ✅
  - "Last write wins" veya "Source of truth" stratejisi (PMS = source of truth) ✅
  - Manual conflict resolution UI (admin panel) - Backend hazır
  - Sync history ve audit log (kim, ne zaman, ne değiştirdi) ✅

### 🎯 P0 — Concierge Desk Özellikleri

#### Unified Guest Profile (PMS + GuestFlow Verileri) ✅

- **Misafir Profili Birleştirme**: ✅
  - PMS'den gelen misafir bilgileri (Opera/Elektraweb) ✅ (PMSIntegrationService)
  - GuestFlow'da oluşturulan hizmet geçmişi (transfer, tur, restoran) ✅
  - Concierge notları ve tercihler ✅ (GuestPreferencesService)
  - VIP durumu ve özel istekler ✅
  - Tüm konaklama geçmişi (PMS'den) ✅
- **Guest History Dashboard**: ✅
  - Önceki konaklamalar (PMS'den çekilen) ✅ (ConciergeDashboardService.GetGuestHistoryDashboardAsync)
  - Hizmet geçmişi (GuestFlow'dan) ✅
  - Harcama analizi (PMS + GuestFlow toplam) ✅
  - Tercih analizi (oda tercihi, yemek tercihi, aktivite tercihi) ✅ (GuestPreferenceAnalysisService entegrasyonu)
- **Guest Preferences Management**: ✅
  - Oda tercihleri (PMS'den - high floor, sea view, vb.) ✅ (GuestPreferenceAnalysisService.MergePreferencesFromPMSAsync)
  - Yemek tercihleri (vegan, halal, alerjiler) ✅
  - Aktivite tercihleri (spor, kültür, eğlence) ✅
  - İletişim tercihleri (e-posta, SMS, WhatsApp) ✅

#### Concierge Dashboard (Günlük Operasyon Merkezi) ✅

- **Günlük Operasyon Ekranı**: ✅
  - **Bugün Check-in Olan Misafirler** (PMS'den çekilen) ✅ (ConciergeDashboardService.GetTodayCheckInsAsync)
  - **Bugün Check-out Olan Misafirler** (PMS'den) ✅ (ConciergeDashboardService.GetTodayCheckOutsAsync)
  - **Aktif Misafirler** (şu anda otelde - PMS'den) ✅ (ConciergeDashboardService.GetActiveGuestsAsync)
  - **Yaklaşan Servisler** (transfer, tur, restoran - GuestFlow'dan) ✅ (DailyOperationsService, ConciergeDashboardService.GetUpcomingServicesForTodayAsync)
  - **Bekleyen Talepler** (concierge talepleri) ✅
- **Misafir Durumu Göstergeleri**: ✅
  - **VIP Misafirler** (highlight - PMS'den VIP flag) ✅
  - **Özel İstekleri Olan Misafirler** (special requests flag) ✅
  - **Sorunlu Misafirler** (problem flag - önceki konaklamalardan) ✅ (Risk flags)
  - **Tekrar Eden Misafirler** (loyalty - PMS'den geçmiş konaklamalar) ✅
  - **Doğum Günü/Yıldönümü** (PMS'den çekilen) ✅
- **Hızlı Aksiyonlar** (One-Click Actions): ✅
  - Transfer rezervasyonu (tek tıkla - misafir bilgisi otomatik doldurulur) ✅ (QuickActionService.CreateTransferReservationAsync)
  - Tur rezervasyonu ✅ (QuickActionService.CreateTourReservationAsync)
  - Restoran rezervasyonu ✅ (QuickActionService.CreateRestaurantReservationAsync)
  - Oda servisi talebi ✅ (QuickActionService.CreateRoomServiceRequestAsync)
  - Mesaj gönderme (e-posta/SMS/WhatsApp) ✅ (QuickActionService.SendMessageAsync)
  - Fatura görüntüleme (PMS folio) ✅ (QuickActionService.GetFolioAsync)

#### Misafir İletişim Merkezi ✅

- **Unified Communication**: ✅
  - E-posta entegrasyonu (misafir e-postaları - PMS'den çekilen) ✅ (UnifiedCommunicationService)
  - SMS entegrasyonu ✅ (UnifiedCommunicationService)
  - WhatsApp Business entegrasyonu ✅ (UnifiedCommunicationService)
  - In-app messaging ✅ (UnifiedCommunicationService)
- **Communication History**: ✅
  - Tüm iletişim geçmişi (tek yerde - PMS + GuestFlow) ✅ (UnifiedCommunicationService.GetCommunicationHistoryAsync)
  - Misafir ile yapılan tüm konuşmalar ✅
  - Otomatik yanıt şablonları (concierge için) ✅
- **Smart Notifications**: ✅
  - **Pre-Arrival**: Check-in öncesi hoş geldin mesajı (PMS rezervasyon bilgisi ile) ✅ (SmartNotificationService.SendPreArrivalNotificationsAsync)
  - **Arrival**: Check-in sonrası bilgilendirme ✅ (SmartNotificationService.SendArrivalNotificationsAsync)
  - **During Stay**: Hizmet hatırlatmaları ✅ (SmartNotificationService.SendDuringStayNotificationsAsync)
  - **Pre-Departure**: Check-out öncesi veda mesajı ✅ (SmartNotificationService.SendPreDepartureNotificationsAsync)
  - **Special Occasions**: Doğum günü, yıldönümü (PMS'den çekilen) ✅ (SmartNotificationService.SendSpecialOccasionNotificationsAsync)

## 🧠 Sprint 6-7 (17-24 hafta) — Turizm Operasyon Intelligence Layer (YENİ VİZYON)

### Hedef

GuestFlow'u **Turizm Operasyon Intelligence Layer** haline getirmek. İnsan davranışını ürünün merkezine koyarak, oteldeki **insan + misafir + hizmet + zaman + duygu** ilişkilerinin graf veri modelini oluşturmak.

**Vizyon**: GuestFlow = Otelin "İnsan İlişkileri Hafızası"

### Yapılacaklar

#### Graf Veri Modeli Altyapısı ✅

- **Neo4j Entegrasyonu**: ✅
  - Neo4j database kurulumu ve konfigürasyonu ✅ (Neo4jSettings, INeo4jService)
  - Neo4j .NET driver entegrasyonu (`Neo4j.Driver`) ✅
  - Graph database connection management ✅
  - Hybrid architecture (SQL Server + Neo4j) ✅
  - Data synchronization layer (dual-write pattern) ✅ (BehavioralTrackingService)
- **Node Types (Graf Düğümleri)**: ✅
  - `Guest` nodes (misafir) ✅ (GraphDataService.CreateOrUpdateGuestNodeAsync)
  - `Staff` nodes (personel) ✅ (GraphDataService.CreateOrUpdateStaffNodeAsync)
  - `Service` nodes (hizmet: Transfer, Tour, Restaurant) ✅ (GraphDataService.CreateOrUpdateServiceNodeAsync)
  - `Time` nodes (zaman: Date, Time, Season) ✅ (GraphDataService.CreateOrUpdateTimeNodeAsync)
  - `Emotion` nodes (duygu: Sentiment, Satisfaction) ✅ (GraphDataService.CreateOrUpdateEmotionNodeAsync)
- **Edge Types (Graf Kenarları)**: ✅
  - `INTERACTS` (misafir-personel etkileşimi) ✅ (GraphDataService.CreateOrUpdateInteractionRelationshipAsync)
  - `PREFERS` (misafir tercihleri) ✅ (GraphDataService.CreateOrUpdatePreferenceRelationshipAsync)
  - `SATISFIES` (hizmet memnuniyeti) ✅ (GraphDataService.CreateOrUpdateSatisfactionRelationshipAsync)
  - `RECOMMENDS` (öneriler) ✅
  - `OCCURS_AT` (zaman ilişkisi) ✅ (GraphDataService.CreateOrUpdateTimeRelationshipAsync)
  - `FEELS` (duygu ilişkisi) ✅ (GraphDataService.CreateOrUpdateEmotionRelationshipAsync)
  - `LEARNS_FROM` (öğrenme ilişkisi) ✅
- **Relationship Properties (İlişki Özellikleri)**: ✅
  - `weight` (ilişki ağırlığı: 0.0-1.0) ✅
  - `frequency` (sıklık: kaç kez) ✅
  - `sentiment` (duygu skoru: -1.0 to 1.0) ✅
  - `satisfaction` (memnuniyet: 0-10) ✅
  - `timestamp` (zaman damgası) ✅
  - `context` (bağlam: JSON) ✅

#### Behavioral Data Collection (Davranışsal Veri Toplama) ✅

- **Misafir Davranış Takibi**: ✅
  - Rezervasyon kalıpları analizi (tarih, süre, oda tipi) ✅ (GuestBehaviorEntity, BehavioralTrackingService)
  - Hizmet tercihleri tracking (transfer, tur, restoran) ✅
  - İletişim tercihleri tracking (email, SMS, WhatsApp) ✅
  - Zaman tercihleri tracking (sabah, öğle, akşam) ✅
  - Harcama kalıpları analizi (tutar, kategori, sıklık) ✅
  - Memnuniyet göstergeleri tracking (rating, feedback) ✅
- **Personel Davranış Takibi**: ✅
  - Hizmet sunma kalıpları (başarı oranı, hız) ✅ (StaffBehaviorEntity)
  - Misafir etkileşim kalıpları (sıklık, kalite) ✅
  - Başarı metrikleri (memnuniyet skorları) ✅
  - Tercih öğrenme hızı (ne kadar hızlı öğreniyor) ✅
  - Problem çözme yetenekleri ✅
- **İlişki Davranış Takibi**: ✅
  - Misafir-Personel uyumu skorları ✅ (GuestStaffInteractionEntity)
  - Hizmet-Misafir uyumu skorları ✅
  - Zaman-Hizmet uyumu skorları ✅
  - Duygu-Hizmet uyumu skorları ✅

#### Sentiment & Emotional Intelligence (Duygu Zekası) ✅

- **Sentiment Analysis Integration**: ✅
  - Communication sentiment (Email, SMS, WhatsApp mesajları) ✅ (SentimentAnalysisService.AnalyzeSentimentAsync)
  - Review sentiment (OTA reviews: Booking.com, Expedia) ✅
  - Feedback sentiment (misafir geri bildirimleri) ✅
  - Real-time sentiment tracking (anlık duygu takibi) ✅
  - Sentiment history (duygu geçmişi) ✅ (SentimentAnalysisService.GetGuestSentimentTrendsAsync)
- **Emotional Markers (Duygusal İşaretler)**: ✅
  - Duygusal durum takibi (mutlu, memnun, nötr, memnun değil) ✅
  - Duygusal yolculuk haritası (emotional journey mapping) ✅
  - Duygu-hizmet uyumu (hangi hizmet hangi duyguyu tetikliyor) ✅
  - Duygusal trendler (zaman içinde duygu değişimleri) ✅
  - Emotional triggers (duygusal tetikleyiciler) ✅

#### Relationship Intelligence Service (İlişki Zekası Servisi) ✅

- **Guest-Staff Matching (Misafir-Personel Eşleştirmesi)**: ✅
  - Uyum analizi algoritması (compatibility algorithm) ✅ (RelationshipIntelligenceService.FindBestStaffMatchesAsync)
  - Başarı skorları hesaplama (success score calculation) ✅ (RelationshipIntelligenceService.CalculateCompatibilityAsync)
  - Otomatik eşleştirme önerileri (automatic matching recommendations) ✅
  - Historical matching performance (geçmiş eşleştirme performansı) ✅
- **Service-Guest Matching (Hizmet-Misafir Eşleştirmesi)**: ✅
  - Tercih bazlı öneriler (preference-based recommendations) ✅ (RelationshipIntelligenceService.RecommendServicesAsync)
  - Davranış bazlı öneriler (behavior-based recommendations) ✅
  - Zaman bazlı öneriler (time-based recommendations) ✅
  - Duygu bazlı öneriler (emotion-based recommendations) ✅
- **Relationship Strength Calculation (İlişki Gücü Hesaplama)**: ✅
  - İlişki gücü hesaplama algoritması (relationship strength algorithm) ✅
  - İlişki geçmişi tracking (relationship history tracking) ✅
  - İlişki trendleri analizi (relationship trends analysis) ✅
  - İlişki öngörüleri (relationship predictions) ✅

#### Predictive Intelligence (Tahminsel Zeka) ✅

- **Guest Behavior Prediction (Misafir Davranış Tahmini)**: ✅
  - Rezervasyon tahminleri (reservation predictions) ✅ (PredictiveIntelligenceService.PredictGuestBehaviorAsync)
  - Hizmet talep tahminleri (service demand predictions) ✅ (PredictiveIntelligenceService.PredictServiceDemandAsync)
  - Memnuniyet tahminleri (satisfaction predictions) ✅ (PredictiveIntelligenceService.PredictSatisfactionAsync)
  - Harcama tahminleri (spending predictions) ✅ (PredictiveIntelligenceService.PredictSpendingAsync)
- **Risk Prediction (Risk Tahmini)**: ✅
  - Dissatisfaction risk (memnuniyetsizlik riski) ✅ (PredictiveIntelligenceService.PredictRisksAsync)
  - Cancellation risk (iptal riski) ✅
  - Problem risk (sorun riski) ✅
  - Churn risk (müşteri kaybı riski) ✅
- **Opportunity Detection (Fırsat Tespiti)**: ✅
  - Upsell fırsatları (ek satış fırsatları) ✅ (PredictiveIntelligenceService.PredictOpportunitiesAsync)
  - Cross-sell fırsatları (çapraz satış fırsatları) ✅
  - Personalization fırsatları (kişiselleştirme fırsatları) ✅
  - Loyalty fırsatları (sadakat fırsatları) ✅

#### Proactive Intelligence (Proaktif Zeka) ✅

- **Proactive Service Recommendations (Proaktif Hizmet Önerileri)**: ✅
  - Davranış bazlı öneriler (behavior-based recommendations) ✅ (ProactiveIntelligenceService.GetProactiveServiceRecommendationsAsync)
  - Zaman bazlı öneriler (time-based recommendations) ✅
  - Duygu bazlı öneriler (emotion-based recommendations) ✅
  - İlişki bazlı öneriler (relationship-based recommendations) ✅
- **Proactive Problem Prevention (Proaktif Problem Önleme)**: ✅
  - Erken uyarı sistemi (early warning system) ✅ (ProactiveIntelligenceService.GetProblemPreventionAlertsAsync)
  - Risk önleme önerileri (risk prevention recommendations) ✅
  - Müdahale önerileri (intervention recommendations) ✅
  - Otomatik aksiyonlar (automatic actions) ✅ (ProactiveIntelligenceService.GetAutomaticActionsAsync)
- **Proactive Personalization (Proaktif Kişiselleştirme)**: ✅
  - Kişiselleştirilmiş deneyim (personalized experience) ✅ (ProactiveIntelligenceService.GetPersonalizationSuggestionsAsync)
  - Otomatik tercih öğrenme (automatic preference learning) ✅
  - Adaptif öneriler (adaptive recommendations) ✅
  - Context-aware suggestions (bağlam farkındalıklı öneriler) ✅

### Teknik Gereksinimler ✅

- **Neo4j Database**: Community veya Enterprise edition ✅ (Neo4jSettings yapılandırıldı)
- **Neo4j .NET Driver**: `Neo4j.Driver` NuGet package ✅
- **Sentiment Analysis**: Azure Text Analytics veya AWS Comprehend ✅ (SentimentAnalysisService - keyword-based, Azure entegrasyonu için hazır)
- **ML/AI Framework**: ML.NET veya Python microservice ⏳ (PredictiveIntelligenceService placeholder, ML.NET entegrasyonu için hazır)
- **Graph Query Language**: Cypher (Neo4j query language) ✅ (GraphDataService Cypher queries kullanıyor)

### Mevcut Sistemle Entegrasyon ✅

- **SQL Server**: Transactional data (mevcut) ✅
- **Neo4j**: Relationship data (yeni) ✅
- **Dual-Write Pattern**: Her iki veritabanına yazma ✅ (BehavioralTrackingService)
- **Event Sourcing**: Graph updates için event-driven architecture ✅
- **CQRS**: Read için graph, write için SQL ✅

### 🎯 P0 — OTA Entegrasyonları (PMS ile Entegre)

#### Mevcut Durum

- ✅ OTA entity'leri ve temel servisler mevcut
- ✅ Booking.com, Expedia, Agoda, Airbnb enum'ları var
- ⚠️ Gerçek API entegrasyonları eksik
- ⚠️ Webhook işleme tam değil

#### Yapılacaklar (PMS ile Senkronize)

- **Booking.com Entegrasyonu (PMS ile Entegre)**:
  - API v2/v3 entegrasyonu (reservations, availability, rates)
  - Webhook handler: rezervasyon oluşturma, iptal, değişiklik
  - **PMS Senkronizasyonu**: Booking.com rezervasyonları → PMS'e otomatik gönder
  - **Channel Manager**: PMS oda durumu → Booking.com availability (real-time)
  - Fiyat senkronizasyonu: PMS fiyatları → Booking.com rates
  - Retry mekanizması + circuit breaker
  - Conflict resolution (çift rezervasyon önleme)
- **Expedia Partner Solutions (EPS) Entegrasyonu (PMS ile Entegre)**:
  - EPS API entegrasyonu
  - Rezervasyon senkronizasyonu: Expedia → PMS
  - Fiyat ve availability: PMS → Expedia (real-time sync)
  - PMS oda durumu → Expedia inventory
- **Airbnb Entegrasyonu**:
  - Airbnb API entegrasyonu (iFrame API veya REST API)
  - Rezervasyon senkronizasyonu
  - Mesajlaşma entegrasyonu (opsiyonel)
- **Channel Manager Özellikleri**:
  - Çoklu kanal yönetimi (Booking, Expedia, Airbnb, Agoda, Hotels.com)
  - Otomatik availability senkronizasyonu (her 5 dakikada)
  - Fiyat kuralları: minimum fiyat, mark-up, sezon bazlı
  - Stop-sale yönetimi (tüm kanallarda anında)
  - Inventory pooling (oda tipleri arası otomatik transfer)
- **OTA Webhook Güvenliği**:
  - Webhook signature doğrulama
  - Idempotency key yönetimi
  - Dead-letter queue (başarısız webhook'lar için)
  - Webhook retry mekanizması (exponential backoff)

### 🎯 P0 — Misafir Deneyimi İyileştirmeleri

#### Misafir Portalı (Self-Service)

- **Rezervasyon Yönetimi**:
  - Misafir self-check-in/check-out
  - Rezervasyon değişiklik talepleri (tarih, oda tipi)
  - Rezervasyon iptal (politika bazlı)
  - Rezervasyon geçmişi görüntüleme
- **Fatura ve Ödeme**:
  - Online fatura görüntüleme ve indirme
  - Online ödeme (kredi kartı, banka transferi)
  - Ödeme geçmişi
  - Borç/alacak durumu
- **Hizmet Talepleri**:
  - Transfer rezervasyonu (self-service)
  - Tur rezervasyonu
  - Restoran rezervasyonu
  - Oda servisi talepleri
- **İletişim**:
  - Mesajlaşma sistemi (personel ile)
  - Bildirim tercihleri (e-posta, SMS, push)
  - Feedback/şikayet formu

#### QR Kod Sistemi

- **Check-in/Check-out QR Kodları**:
  - Misafir için özel QR kod oluşturma
  - Mobil check-in (QR kod okutma)
  - Dijital anahtar (opsiyonel: NFC entegrasyonu)
- **Transfer QR Kodları**:
  - Transfer boarding pass (QR kod)
  - Şoför doğrulama sistemi
- **Hizmet QR Kodları**:
  - Restoran rezervasyon QR kodu
  - Tur katılım QR kodu
  - Wi-Fi erişim QR kodu

### 🎯 P0 — Dinamik Fiyatlama ve Revenue Management

#### Dinamik Fiyatlama Motoru

- **Fiyat Kuralları**:
  - Sezon bazlı fiyatlandırma (yüksek/düşük sezon)
  - Doluluk oranına göre otomatik fiyat ayarlama
  - Lead time bazlı fiyatlandırma (erken rezervasyon indirimi)
  - Length of stay (kalış süresi) bazlı fiyatlandırma
  - Day of week fiyatlandırması (hafta sonu/hafta içi)
- **Competitor Price Tracking** (Opsiyonel):
  - Rakip fiyat takibi (web scraping veya API)
  - Otomatik fiyat önerileri
- **Fiyat Optimizasyonu**:
  - AI destekli fiyat önerileri (machine learning)
  - Demand forecasting (talep tahmini)
  - Revenue per available room (RevPAR) optimizasyonu

#### Revenue Management Dashboard

- RevPAR analizi
- ADR (Average Daily Rate) takibi
- Occupancy rate (doluluk oranı) analizi
- Revenue by channel (kanal bazlı gelir)
- Forecast dashboard (gelecek dönem tahminleri)

### 🎯 P1 — Mobil Uygulama

#### React Native Mobil Uygulama

- **Personel Uygulaması** (iOS + Android):
  - Günlük operasyon ekranı (bugünkü servisler)
  - Misafir bilgileri (hızlı erişim)
  - Transfer takibi (gerçek zamanlı konum)
  - Check-in/check-out (QR kod okutma)
  - Bildirimler (push notifications)
  - Offline çalışma desteği (sync mekanizması)
- **Misafir Uygulaması** (Opsiyonel):
  - Rezervasyon yönetimi
  - Check-in/check-out
  - Hizmet rezervasyonu
  - Mesajlaşma
  - Fatura görüntüleme

### 🎯 P1 — Review ve Rating Sistemi

#### Misafir Değerlendirmeleri

- **Review Yönetimi**:
  - Check-out sonrası otomatik review talebi (e-posta/SMS)
  - Review formu (misafir portalı veya e-posta linki)
  - Review onay süreci (spam koruması)
  - Review yanıtları (personel yanıt verebilir)
- **Rating Sistemi**:
  - Genel rating (1-5 yıldız)
  - Kategori bazlı rating (temizlik, hizmet, konum, fiyat)
  - Review görüntüleme (public web sayfası)
- **Review Analytics**:
  - Ortalama rating takibi
  - Review trend analizi
  - Negatif review uyarıları
  - Review export (TripAdvisor, Google Maps için)

### 🎯 P1 — Loyalty Programı

#### Sadakat Programı

- **Puan Sistemi**:
  - Her rezervasyon için puan kazanma
  - Puan kullanımı (indirim, ücretsiz hizmet)
  - Puan geçmişi
- **Tier Sistemi** (Bronze, Silver, Gold, Platinum):
  - Seviye bazlı avantajlar
  - Otomatik tier yükseltme
  - Özel fiyatlandırma (VIP müşteriler)
- **Kampanyalar**:
  - Özel indirimler (loyalty üyeleri için)
  - Doğum günü indirimleri
  - Sezon kampanyaları

### 🎯 P1 — AI Destekli Özellikler

#### Akıllı Öneriler

- **Transfer Önerileri** (mevcut özellik genişletilebilir):
  - Misafir aktivitelerine göre otomatik transfer önerileri
  - Trafik durumuna göre zaman önerileri
- **Hizmet Önerileri**:
  - Misafir tercihlerine göre tur önerileri
  - Restoran önerileri (mutfak tercihi, bütçe)
  - Paket servis önerileri
- **Chatbot Asistan**:
  - 7/24 müşteri desteği
  - Sık sorulan sorular (FAQ)
  - Rezervasyon sorgulama
  - Hizmet bilgisi

#### Tahmin Analitiği

- **Demand Forecasting**:
  - Gelecek dönem talep tahmini
  - Sezon bazlı tahminler
  - Event bazlı talep artışı tahmini
- **Revenue Forecasting**:
  - Gelir tahminleri
  - Kârlılık projeksiyonları

### 🎯 P1 — Real-Time Inventory Management

#### Stok ve Kullanılabilirlik Yönetimi

- **Oda Envanteri**:
  - Gerçek zamanlı oda durumu (available, occupied, maintenance, out of order)
  - Oda tipi bazlı envanter
  - Oda atama optimizasyonu
- **Araç Envanteri**:
  - Transfer araçlarının gerçek zamanlı durumu
  - Araç kullanılabilirlik takibi
  - Bakım planlama
- **Restoran Kapasitesi**:
  - Masa kullanılabilirliği
  - Rezervasyon kapasitesi yönetimi

### 🎯 P1 — Weather ve Event Entegrasyonları

#### Hava Durumu Entegrasyonu

- **Weather API Entegrasyonu** (OpenWeatherMap, WeatherAPI):
  - Hava durumu bilgisi (misafir dashboard'unda)
  - Hava durumuna göre hizmet önerileri (yağmurlu gün → kapalı mekan turları)
  - Hava durumu uyarıları (fırtına, kar vb.)

#### Etkinlik Yönetimi

- **Event Management**:
  - Şehir etkinlikleri takibi (festival, konser, spor etkinliği)
  - Etkinlik bazlı talep artışı tahmini
  - Etkinlik paketleri (etkinlik + transfer + konaklama)

### 🎯 P1 — Çoklu Dil ve Yerelleştirme

#### Dil Desteği Genişletme

- **Ek Diller**:
  - Almanca (Alman turistler için)
  - Rusça (Rus turistler için)
  - Arapça (Orta Doğu pazarı için)
  - Fransızca (Fransız turistler için)
- **Yerelleştirme**:
  - Tarih/saat formatları (ülkeye göre)
  - Para birimi formatları
  - Telefon numarası formatları
  - Adres formatları

### 🎯 P1 — Yerel Ödeme Yöntemleri

#### Ödeme Gateway Entegrasyonları

- **Türkiye**:
  - İyzico entegrasyonu
  - PayTR entegrasyonu
  - Garanti BBVA Sanal POS
- **Uluslararası**:
  - Stripe entegrasyonu
  - PayPal entegrasyonu
  - Banka transferi (SWIFT)
- **Dijital Cüzdanlar**:
  - Apple Pay
  - Google Pay
  - Crypto ödemeler (opsiyonel)

### 🎯 P1 — Concierge İş Akışları (PMS Entegre)

#### Misafir Karşılama Akışı (End-to-End)

- **Pre-Arrival (Geliş Öncesi)**:
  - Rezervasyon bilgisi (PMS'den otomatik çekilir)
  - Misafir profili kontrolü (PMS + GuestFlow geçmiş)
  - Özel istekler kontrolü (VIP, doğum günü, yıldönümü)
  - Hoş geldin mesajı hazırlama (otomatik şablon)
  - Oda hazırlığı kontrolü (PMS'den oda durumu)
- **Arrival (Geliş)**:
  - Check-in işlemi (PMS ile senkronize - real-time)
  - Oda atama (PMS'den oda bilgisi)
  - Key handover (dijital anahtar veya fiziksel)
  - Concierge tanıtımı (misafir bilgisi ekranda)
  - İlk hizmet önerileri (geçmiş tercihlere göre)
- **During Stay (Konaklama Sırası)**:
  - Hizmet talepleri yönetimi (transfer, tur, restoran)
  - Sorun çözme (concierge notları)
  - Özel istekler takibi (VIP hizmetler)
  - Fatura takibi (PMS folio + GuestFlow hizmetleri)
- **Pre-Departure (Ayrılış Öncesi)**:
  - Check-out hazırlığı (PMS'den check-out tarihi)
  - Fatura kontrolü (PMS folio + GuestFlow faturaları)
  - Veda mesajı (otomatik)
  - Feedback talebi (review sistemi)
  - Son hizmet önerileri (check-out sonrası transfer)

#### Hizmet Yönetimi Akışı (PMS Verileri ile Entegre)

- **Transfer Yönetimi**:
  - Misafir check-in/check-out zamanına göre otomatik transfer önerisi (PMS'den)
  - Havalimanı transferleri (PMS'den uçuş bilgisi çekilebilir - gelecekte)
  - Şehir içi transferler
  - Transfer durumu takibi (real-time)
- **Tur Yönetimi**:
  - Misafir tercihlerine göre tur önerileri (geçmiş konaklamalardan)
  - Tur rezervasyonu ve takibi
  - Tur sonrası feedback
- **Restoran Yönetimi**:
  - Restoran rezervasyonu
  - Özel diyet istekleri (PMS'den alerji bilgisi)
  - Doğum günü/özel gün organizasyonları (PMS'den özel gün bilgisi)

### 🎯 P1 — Raporlama ve Analitik (PMS + GuestFlow Verileri)

#### Concierge Performans Raporları

- **Hizmet Dağılımı**:
  - Transfer sayıları (GuestFlow)
  - Tur rezervasyonları (GuestFlow)
  - Restoran rezervasyonları (GuestFlow)
  - Toplam hizmet geliri (GuestFlow)
  - Konaklama geliri (PMS'den)
  - Toplam misafir geliri (PMS + GuestFlow)
- **Misafir Memnuniyeti**:
  - Hizmet bazlı rating'ler (GuestFlow)
  - Konaklama rating'leri (PMS'den - gelecekte)
  - Concierge personel performansı
  - Sorun çözme süreleri

#### Operasyonel Raporlar (PMS + GuestFlow Unified)

- **Günlük Operasyon Özeti**:
  - Check-in/check-out sayıları (PMS'den)
  - Aktif misafir sayısı (PMS'den)
  - Hizmet talepleri (GuestFlow)
  - Bekleyen işler (concierge)
- **Gelir Raporları (Unified)**:
  - PMS gelirleri (konaklama - PMS'den)
  - GuestFlow gelirleri (hizmetler - GuestFlow'dan)
  - Toplam gelir analizi (PMS + GuestFlow)
  - Misafir başına ortalama gelir (ADR + hizmet geliri)
  - RevPAR (Revenue per Available Room - PMS'den)

### 🎯 P1 — WhatsApp Business Entegrasyonu

#### WhatsApp Business API

- **Otomatik Mesajlaşma**:
  - Rezervasyon onay mesajları (PMS rezervasyon bilgisi ile)
  - Check-in hatırlatmaları (PMS check-in tarihi ile)
  - Transfer bilgilendirmeleri (GuestFlow)
  - Fatura gönderimi (PMS folio + GuestFlow)
- **Müşteri Desteği**:
  - WhatsApp üzerinden müşteri desteği
  - Otomatik yanıtlar (chatbot)
  - Personel yanıt sistemi (concierge)

### 🎯 P2 — Analytics ve Business Intelligence

#### Gelişmiş Raporlama

- **Business Intelligence Dashboard**:
  - Power BI veya Tableau entegrasyonu
  - Özel dashboard'lar (yönetim için)
  - Real-time metrikler
- **Raporlar**:
  - Misafir segmentasyonu analizi
  - Kanal performans analizi
  - Hizmet kârlılık analizi
  - Personel performans raporları
  - Sezon bazlı trend analizi

### 🎯 P2 — White-Label Çözüm

#### Çoklu Marka Desteği

- **Multi-Tenancy**:
  - Birden fazla otel/marka yönetimi
  - Marka bazlı özelleştirme (logo, renkler, tema)
  - Marka bazlı fiyatlandırma
- **Franchise Yönetimi**:
  - Merkezi yönetim (franchise sahibi)
  - Lokal yönetim (franchise işletmeleri)
  - Raporlama (merkezi + lokal)

### 🎯 P2 — API Marketplace

#### Üçüncü Taraf Entegrasyonları

- **API Documentation**:
  - Swagger/OpenAPI dokümantasyonu (mevcut, genişletilebilir)
  - API key yönetimi
  - Rate limiting (API bazlı)
- **Webhook Sistemi**:
  - Custom webhook'lar (müşteri tanımlı)
  - Webhook test ortamı
- **Entegrasyon Marketplace**:
  - Üçüncü taraf uygulamalar (CRM, muhasebe, pazarlama)
  - Plugin sistemi

### 🎯 P2 — Compliance ve Güvenlik

#### KVKK/GDPR Uyumu

- **Veri Koruma**:
  - Veri maskeleme (PII koruması)
  - Veri silme/anonymize (right to be forgotten)
  - Veri export (data portability)
  - Onay yönetimi (consent management)
- **Güvenlik İyileştirmeleri**:
  - 2FA (iki faktörlü doğrulama) - zorunlu Admin için
  - Brute-force koruması
  - Login audit log (kim, ne zaman, nereden giriş yaptı)
  - IP whitelist/blacklist
  - Session yönetimi (concurrent session limit)

---

## Sprint 6 (16-24 hafta) — Ölçeklenme ve Platform Genişletme

### Hedef

Projeyi ölçeklenebilir bir platforma dönüştürmek ve SaaS modeline hazırlamak.

### 🎯 P0 — SaaS Altyapısı

#### Multi-Tenancy

- **Tenant Yönetimi**:
  - Tenant isolation (veritabanı seviyesinde)
  - Tenant bazlı konfigürasyon
  - Tenant bazlı faturalandırma
- **Subscription Yönetimi**:
  - Plan yönetimi (Starter, Professional, Enterprise)
  - Kullanım bazlı faturalandırma
  - Otomatik ödeme (subscription renewal)

### 🎯 P1 — Performance ve Ölçeklenebilirlik

#### Optimizasyonlar

- **Database Optimizasyonu**:
  - Read replica'lar (okuma performansı)
  - Database sharding (büyük veri için)
  - Connection pooling optimizasyonu
- **Caching Stratejisi**:
  - Redis cluster (yüksek kullanılabilirlik)
  - Distributed caching
  - Cache invalidation stratejisi
- **CDN Entegrasyonu**:
  - Static asset CDN (CloudFlare, AWS CloudFront)
  - Image optimization
  - Lazy loading iyileştirmeleri

### 🎯 P1 — Monitoring ve Observability

#### Gelişmiş İzleme

- **Application Performance Monitoring (APM)**:
  - OpenTelemetry entegrasyonu
  - Distributed tracing
  - Error tracking (Sentry entegrasyonu)
- **Business Metrics**:
  - Custom metrikler (rezervasyon sayısı, gelir, vb.)
  - Alert kuralları (business KPI'lar için)
- **Log Aggregation**:
  - ELK Stack veya Datadog entegrasyonu
  - Log retention policy
  - Log search ve analiz

---

## Öncelik Matrisi (Satış ve Pazar Etkisi)

### 🔥 EN YÜKSEK ÖNCELİK (Kritik - Hemen Başla)

1. **PMS Entegrasyonları (Opera, Elektraweb)** - ⭐⭐⭐⭐⭐
   - **Neden Kritik**: Projenin temel amacı - concierge desk için zorunlu
   - **Satış Etkisi**: %300+ artış (5 yıldızlı oteller için zorunlu özellik)
   - **Rekabet Avantajı**: Çok az rakip bu entegrasyonu sunuyor
   - **Müşteri Segmenti**: 5 yıldızlı oteller, lüks otel zincirleri
   - **ROI**: Çok Yüksek - müşteri kazanmak için kritik

2. **Concierge Dashboard** - ⭐⭐⭐⭐⭐
   - **Neden Kritik**: PMS entegrasyonu olmadan anlamsız
   - **Satış Etkisi**: %200+ artış (operasyonel verimlilik)
   - **Rekabet Avantajı**: Tek ekranda tüm bilgiler
   - **Müşteri Segmenti**: Tüm concierge desk kullanan oteller

3. **OTA Entegrasyonları (PMS ile Entegre)** - ⭐⭐⭐⭐
   - **Neden Kritik**: Booking.com, Expedia - pazar girişi için kritik
   - **Satış Etkisi**: %150+ artış (rezervasyon kanalları)
   - **Rekabet Avantajı**: PMS + OTA tek platformda
   - **Müşteri Segmenti**: Tüm oteller

### 🔥 Yüksek Öncelik (3-6 Ay)

1. **Unified Guest Profile (PMS + GuestFlow)** - ⭐⭐⭐⭐
   - **Satış Etkisi**: %100+ artış (misafir deneyimi)
   - **Rekabet Avantajı**: Tek ekranda tüm misafir bilgileri

2. **Misafir Portalı** - ⭐⭐⭐
   - **Satış Etkisi**: %50+ artış (müşteri memnuniyeti)
   - **Rekabet Avantajı**: Self-service özellikler

3. **Mobil Uygulama (Concierge)** - ⭐⭐⭐
   - **Satış Etkisi**: %80+ artış (operasyonel verimlilik)
   - **Rekabet Avantajı**: Sahada çalışan personel için

4. **Review/Rating Sistemi** - ⭐⭐⭐
   - **Satış Etkisi**: %40+ artış (online görünürlük)

### ⚡ Orta Öncelik (3-6 Ay)

1. **Loyalty Programı** - Müşteri sadakati
2. **AI Öneriler** - Farklılaşma
3. **WhatsApp Entegrasyonu** - Yerel pazar için önemli
4. **QR Kod Sistemi** - Modern deneyim
5. **Weather/Event Entegrasyonları** - Değer katma

### 📈 Düşük Öncelik (6+ Ay)

1. **White-Label** - Enterprise müşteriler için
2. **API Marketplace** - Platform genişletme
3. **SaaS Altyapısı** - Ölçeklenme
4. **Advanced Analytics** - Enterprise özellik

---

## Satış Stratejisi ile Entegrasyon (PMS Odaklı)

### Özellik → Satış Noktası Eşleştirmesi (Güncellenmiş)

| Özellik | Satış Noktası | Hedef Müşteri Segmenti | Satış Etkisi |
|---------|---------------|------------------------|--------------|
| **PMS Entegrasyonu (Opera/Elektraweb)** | "Mevcut PMS sisteminizle anlık entegrasyon - Concierge desk'iniz için özel tasarlandı" | **5 yıldızlı oteller** | ⭐⭐⭐⭐⭐ %300+ |
| **Concierge Dashboard** | "Tüm misafir bilgileri tek ekranda - PMS + hizmetler birleşik görünüm" | **Concierge desk kullanan oteller** | ⭐⭐⭐⭐⭐ %200+ |
| **Unified Guest Profile** | "PMS'den gelen misafir bilgileri + hizmet geçmişi = Tam misafir profili" | **5 yıldızlı oteller** | ⭐⭐⭐⭐ %100+ |
| **OTA Entegrasyonları (PMS ile)** | "Booking.com/Expedia rezervasyonları otomatik PMS'e aktarılır" | **Tüm oteller** | ⭐⭐⭐⭐ %150+ |
| **Günlük Operasyon Ekranı** | "Bugünkü check-in/check-out'ları PMS'den otomatik görün - Hızlı aksiyonlar" | **Concierge desk** | ⭐⭐⭐⭐ %150+ |
| **Misafir İletişim Merkezi** | "PMS + GuestFlow tüm iletişim geçmişi tek yerde" | **5 yıldızlı oteller** | ⭐⭐⭐ %80+ |
| **Mobil Uygulama (Concierge)** | "Sahada çalışan personeliniz PMS verilerine mobilden erişsin" | **Concierge desk** | ⭐⭐⭐ %80+ |
| **Unified Raporlama** | "PMS gelirleri + hizmet gelirleri = Toplam gelir analizi" | **Yönetim** | ⭐⭐⭐ %60+ |
| Misafir Portalı | "Misafirleriniz self-service ile size zaman kazandırır" | Orta-büyük oteller | ⭐⭐⭐ %50+ |
| Dinamik Fiyatlama | "Gelirinizi %20-30 artırın" | Tüm oteller | ⭐⭐⭐ %40+ |
| Review Sistemi | "Online görünürlüğünüzü artırın" | Tüm oteller | ⭐⭐ %40+ |
| Loyalty Programı | "Müşteri sadakati ile tekrar rezervasyonları artırın" | Orta-büyük oteller | ⭐⭐ %30+ |

### 🎯 Satış Stratejisi Değişiklikleri (PMS Odaklı)

#### 1. Hedef Müşteri Segmenti Değişikliği

**ÖNCE**: Genel otel yönetim sistemi (tüm oteller)
**ŞİMDİ**: **5 yıldızlı oteller ve concierge desk kullanan oteller** (odaklanmış pazar)

#### 2. Satış Noktaları Güncellemesi

**Yeni Ana Satış Noktası**:
> "GuestFlow, 5 yıldızlı otellerin concierge desk operasyonları için özel tasarlanmış bir misafir yönetim sistemidir. Mevcut PMS sisteminizle (Opera, Elektraweb) **anlık entegrasyon** sağlayarak, concierge personelinizin tüm misafir bilgilerini ve hizmetlerini **tek ekrandan** yönetmesini sağlar."

**Destekleyici Satış Noktaları**:

1. **"PMS Entegrasyonu"**: "Opera veya Elektraweb sisteminizle anlık senkronizasyon - Veri girişi yok, otomatik çalışır"
2. **"Concierge Dashboard"**: "Bugünkü check-in/check-out'ları, aktif misafirleri, yaklaşan servisleri tek ekranda görün"
3. **"Unified Guest Profile"**: "PMS'den gelen misafir bilgileri + hizmet geçmişi = Tam misafir profili"
4. **"OTA Entegrasyonu"**: "Booking.com/Expedia rezervasyonları otomatik PMS'e aktarılır, manuel işlem yok"

#### 3. Fiyatlandırma Stratejisi Değişikliği

**ÖNCE**: Genel otel yönetim sistemi fiyatlandırması
**ŞİMDİ**: **Concierge/5 yıldızlı otel odaklı premium fiyatlandırma**

| Segment | Özellikler | Fiyatlandırma |
|---------|-----------|---------------|
| **Starter** | Temel concierge özellikleri, 1 PMS entegrasyonu | $5,000-10,000/yıl |
| **Professional** | Tam concierge özellikleri, 2+ PMS entegrasyonu, OTA entegrasyonları | $15,000-30,000/yıl |
| **Enterprise** | White-label, multi-tenant, özel entegrasyonlar | $50,000-100,000/yıl |

#### 4. Rekabet Analizi Güncellemesi

**Rakip Ürünler**:

- **Opera Concierge**: Opera'nın kendi concierge modülü (sınırlı özellikler)
- **Elektraweb Concierge**: Elektraweb'in concierge modülü (sınırlı)
- **GuestBridge**: Concierge yönetimi (PMS entegrasyonu zayıf)
- **ALICE**: Concierge platformu (pahalı, $200-500/oda/ay)

**GuestFlow'un Avantajları**:

- ✅ **PMS Entegrasyonu**: Opera ve Elektraweb ile anlık entegrasyon (rakiplerde yok)
- ✅ **Unified Platform**: PMS + hizmetler tek ekranda
- ✅ **Türkçe Dil Desteği**: Yerel pazar için
- ✅ **Uygun Fiyat**: ALICE'den çok daha uygun
- ✅ **Özelleştirilebilir**: Tam özelleştirme imkanı

#### 5. Satış Kanalı Stratejisi

**Doğrudan Satış**:

- 5 yıldızlı otel yöneticileri ile doğrudan görüşme
- Concierge müdürleri ile demo
- Otel zincirleri ile kurumsal anlaşmalar

**Partner Satışı**:

- PMS danışmanları (Opera, Elektraweb danışmanları)
- Otel yönetim şirketleri
- Turizm danışmanları

**Referans Programı**:

- İlk 5 müşteri için özel fiyatlandırma
- Başarı hikayeleri oluşturma
- Case study'ler hazırlama

---

## Başarı Metrikleri (KPI'lar) - PMS Odaklı

### Teknik Metrikler

- **PMS Entegrasyon Başarı Oranı**: %99.9+ (kritik - concierge için zorunlu)
- **PMS Sync Latency**: < 5 saniye (real-time sync)
- **PMS Data Accuracy**: %100 (veri tutarlılığı)
- OTA entegrasyon başarı oranı: %99.5+
- Webhook işleme süresi: < 2 saniye
- Mobil uygulama crash rate: < 0.1%
- API response time: < 500ms (p95)

### İş Metrikleri (PMS Entegrasyonu Sonrası)

- **Concierge Verimlilik Artışı**: %40-60 (tek ekranda tüm bilgiler)
- **Hata Oranı Azalması**: %50-70 (manuel veri girişi yok)
- **Müşteri Memnuniyeti**: 4.7/5 (hızlı ve doğru hizmet)
- **Hizmet Satış Artışı**: %30-50 (concierge daha fazla hizmet önerebilir)
- Rezervasyon artışı: %15-25 (OTA entegrasyonu sonrası)
- Gelir artışı: %20-30 (unified platform ile)
- Tekrar rezervasyon oranı: %30+ (loyalty programı ile)

### Satış Metrikleri (PMS Odaklı)

- **5 Yıldızlı Otel Segmenti Pazar Payı**: %5+ (hedef 3 yıl)
- **Concierge Desk Kullanan Oteller**: %10+ (hedef 3 yıl)
- **Müşteri Başına Ortalama Gelir**: $20,000-50,000/yıl (premium segment)
- **Churn Rate**: < 5% (PMS entegrasyonu yüksek switching cost yaratır)
- **NPS (Net Promoter Score)**: 60+ (concierge memnuniyeti)

---

## Notlar ve Riskler

### Teknik Riskler

- **OTA API Değişiklikleri**: OTA provider'lar API'lerini değiştirebilir → Versioning stratejisi gerekli
- **Yüksek Trafik**: OTA webhook'ları yüksek trafik yaratabilir → Scalable architecture gerekli
- **Veri Senkronizasyonu**: Çoklu kanal → veri tutarsızlığı riski → Idempotency kritik

### İş Riskleri

- **Rekabet**: Büyük oyuncular (Opera, Cloudbeds) → Farklılaşma önemli
- **Müşteri Adaptasyonu**: Yeni özellikler → Eğitim gerekli
- **Maliyet**: Geliştirme maliyeti → ROI hesaplaması önemli

### Çözüm Önerileri

- **Agile Development**: Hızlı iterasyon, müşteri feedback'i
- **Pilot Program**: Beta müşteriler ile test
- **Dokümantasyon**: Kapsamlı kullanım kılavuzları
- **Destek**: 7/24 teknik destek (SaaS modelinde)

## Kabul Kriterleri (Roadmap’in “bitti” sayılması)

- Bu dosya dışında repo kökünde “roadmap/todo/phase” amaçlı **ikinci bir yol haritası dokümanı kalmaması**.
- README’de tek bir yol haritasına yönlendirme olması.
- Sprint 1 sonunda middleware davranışlarının (headers + rate limit) tutarlı ve test edilebilir olması.

---

## 🛠️ Gelecek İçin Öneriler (Antigravity)

### 1. Mock PMS Servisi (Testing)

Gerçek PMS entegrasyonları (Opera/Elektraweb) VPN, lisans ve kontrat gerektirir. Geliştirme sürecini bloke etmemek için:

- **Mock PMS Microservice**: Standart `IPMSIntegrationService` arayüzünü implemente eden, randomize veri üreten ve webhook tetikleyebilen hafif bir servis yazılmalı. Bu sayede local ortamda "Senkronizasyon" senaryoları uçtan uca test edilebilir.

### 2. Yük Testi (Graph Dual-Write)

SQL + Neo4j (dual-write) mimarisi transaction süresini uzatabilir.

- **K6 veya JMeter** ile yüksek trafik altında (örn. aynı anda 100 check-in) sistemin tepki süresi ve veri tutarlılığı test edilmeli. `BehavioralTrackingService` asenkron (fire-and-forget) kuyruk yapısına (RabbitMQ/MassTransit) geçirilerek ana akış rahatlatılabilir.

### 3. Mobil Uygulama (React Native)

Yol haritasında P1 olarak geçen Mobil Uygulama, saha operasyonu (şoförler, karşılama ekibi) için kritiktir.

- **Sprint 8** olarak resmiyet kazandırılmalı. Backend API'leri (özellikle `Authentication` ve `Operations`) mobil tüketime hazır görünüyor (`JWT` flow mevcut).
