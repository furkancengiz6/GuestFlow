# GuestFlow Demo Rehberi

## 🎯 Demo Ortamını Çalıştırma

Bu rehber, GuestFlow'un tüm özelliklerini test edebileceğiniz demo ortamını hazırlamanız için adım adım talimatlar içerir.

## 📋 Ön Gereksinimler

- .NET 8.0 SDK
- Node.js 18+ (LTS)
- SQL Server (Local veya Docker)

## 🚀 Hızlı Başlangıç (Local Development)

### 1. Veritabanını Hazırlama

```bash
# Migration'ları uygula
cd GuestFlow
dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
```

### 2. Backend'i Demo Mod ile Başlatma

```bash
# Development modunda, demo veri ile başlat
cd GuestFlow.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:SeedDemoData="true"
dotnet run
```

**⚠️ Önemli:** İlk çalıştırmada konsol çıktısındaki demo kullanıcı şifrelerini kaydedin!

```
🚨 DEMO USERS CREATED - SAVE THESE CREDENTIALS! 🚨
Admin: demo.admin@guestflow.local / [random-password]
Staff1: demo.staff.1@guestflow.local / [random-password]
...
```

### 3. Frontend'i Başlatma

```bash
# Yeni terminal penceresi
cd GuestFlow.Frontend
npm install
npm run dev
```

Frontend: <http://localhost:5173>

## 🐳 Docker Compose ile Hızlı Demo

Tüm servisleri tek komutla başlatın:

```bash
# Tüm servisleri başlat (DB, Redis, API, Frontend, Monitoring)
docker-compose up -d

# Logları izle
docker-compose logs -f api
```

**Erişim:**

- Frontend: <http://localhost>
- API: <http://localhost:5000>
- Swagger: <http://localhost:5000/swagger>
- Grafana: <http://localhost:3000> (admin/admin123!)
- Seq Logs: <http://localhost:5341>

## 👥 Demo Kullanıcılar

Seeding sonrası oluşturulan kullanıcılar:

| Email | Rol | Açıklama |
|-------|-----|----------|
| <demo.admin@guestflow.local> | Admin | Tüm yetkilere sahip |
| <demo.staff.1@guestflow.local> | Staff | Personel 1 (Driver) |
| <demo.staff.2@guestflow.local> | Staff | Personel 2 (Guide) |
| <demo.staff.3@guestflow.local> | Staff | Personel 3 (Supervisor) |

**Not:** Şifreler rastgele üretilir, konsol çıktısından kopyalayın.

## 🎨 Demo Veriler

Otomatik olarak oluşturulan örnek veriler:

### Misafirler

- 50+ demo misafir
- Çeşitli uyruklar (US, UK, DE, FR, TR, RU, etc.)
- VIP ve özel misafirler
- Gerçekçi iletişim bilgileri (@guestflow.local)

### Operasyonlar

- **Transferler:** Havalimanı, otel, restoran transferleri
- **Şehir Turları:** İstanbul, Antalya, Kapadokya
- **Yat Turları:** Bodrum, Marmaris, Fethiye
- **Rezervasyonlar:** Restoran rezervasyonları

### Lokasyonlar

- 10+ Şehir (İstanbul, Ankara, İzmir, Antalya...)
- 20+ Havalimanı (IST, SAW, AYT, ESB...)
- 15+ Otel (5 yıldızlı oteller)
- 10+ Restoran

### Finansal

- Demo faturalar (ödenmemiş/ödenmiş)
- Çoklu para birimi desteği (USD, EUR, GBP, TRY)
- Muhasebe kayıtları

## 🧪 Test Senaryoları

### Senaryo 1: Yeni Misafir ve Transfer Oluşturma

1. Login olun (Staff veya Admin)
2. **Guests** → **Add New Guest**
3. Misafir bilgilerini doldurun
4. **Transfers** → **New Transfer**
5. Transfer detaylarını girin
6. **Assign Vehicle** ve **Assign Driver**

### Senaryo 2: Tur Rezervasyonu

1. **Tours** → **City Tours** → **New City Tour**
2. Tarih, şehir, dil seçimi
3. Misafir ekle
4. Fiyat ve indirim bilgileri
5. **Generate Invoice** → **Send Email**

### Senaryo 3: Fatura ve Ödeme

1. **Invoices** → Ödenmemiş fatura seç
2. **View Details** → **Download PDF**
3. **Payments** → **Record Payment**
4. Ödeme yöntemi ve tutar

### Senaryo 4: Raporlama

1. **Reports** → **Dashboard**
2. Gelir grafikleri, istatistikler
3. **Revenue Summary** (tarih filtresi)
4. **Export to Excel/CSV**

### Senaryo 5: PMS Entegrasyonu (Mock)

1. **Settings** → **PMS Integration**
2. Opera Cloud / Elektraweb konfigürasyonu
3. **Sync Guests** (manuel sync)
4. **Webhook Test**

## 📊 Dashboard Özellikleri

Demo verileri ile test edebileceğiniz dashboard özellikleri:

- **Today's Overview:** Günlük check-in/out, aktif transferler
- **Revenue Charts:** Günlük/haftalık/aylık gelir
- **Guest Statistics:** Misafir dağılımı, VIP oranı
- **Service Status:** Pending/confirmed/completed
- **Top Destinations:** En popüler turlar ve şehirler

## 🔐 Güvenlik Test

### Rate Limiting

```bash
# Login endpoint'e 10 kez yanlış şifre
curl -X POST http://localhost:5000/api/v1.0/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"wrong"}'
```

### JWT Expiration

- Access token: 45 dakika
- Refresh token: 7 gün
- Token'ın süresini doldurup refresh mekanizmasını test edin

## 🧹 Demo Verileri Temizleme

```bash
# Veritabanını sıfırla
dotnet ef database drop --project GuestFlow.Persistence --startup-project GuestFlow.Api --force

# Yeniden oluştur ve seed et
dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
$env:SeedDemoData="true"
dotnet run --project GuestFlow.Api
```

## 🐛 Sorun Giderme

### Demo veriler oluşmadı

- `ASPNETCORE_ENVIRONMENT=Development` olduğundan emin olun
- `SeedDemoData=true` ayarlandığından emin olun
- Konsol loglarını kontrol edin

### Database bağlantı hatası

- SQL Server'ın çalıştığından emin olun
- Connection string'i kontrol edin (`appsettings.json`)

### Frontend API'ye bağlanamıyor

- Backend'in çalıştığını doğrulayın: <http://localhost:5146/health>
- CORS ayarlarını kontrol edin
- `.env` dosyasında `VITE_API_BASE_URL` doğru olmalı

## 📱 API Testi (Swagger)

1. Backend çalıştıktan sonra: <http://localhost:5146/swagger>
2. **Authorize** butonuna tıklayın
3. Login endpoint'i ile token alın
4. Token'ı Authorize kısmına yapıştırın
5. Tüm endpoint'leri test edin

## 🎓 Önerilen Test Sırası

1. ✅ **Login & Authentication** - Demo kullanıcı ile giriş
2. ✅ **Guest Management** - CRUD işlemleri
3. ✅ **Transfer Operations** - Transfer oluşturma ve yönetimi
4. ✅ **Tour Bookings** - Tur rezervasyonları
5. ✅ **Invoice Generation** - Fatura oluşturma ve PDF
6. ✅ **Payment Processing** - Ödeme kaydetme
7. ✅ **Reporting** - Dashboard ve raporlar
8. ✅ **File Management** - Dosya yükleme/indirme
9. ✅ **Notifications** - Email/SMS gönderimi (test modu)
10. ✅ **PMS Integration** - Mock PMS sync

## 💡 İpuçları

- **Gerçekçi test için:** Demo verileri gerçek senaryo akışlarını takip eder
- **Multi-currency:** Farklı para birimleri ile faturalar oluşturun
- **Role testing:** Hem Admin hem Staff rolleri ile test edin
- **Mobile test:** Responsive tasarımı mobil ekranlarda test edin
- **Performance:** Chrome DevTools ile network ve performans analizi

## 📞 Destek

Sorun yaşarsanız:

- `docs/` klasöründeki detaylı dokümantasyonu inceleyin
- GitHub Issues'e rapor edin
- Logları kontrol edin (`/health` endpoint'i)

---

**Demo Hazırlayan:** GuestFlow Team  
**Güncellenme:** 2026-01-24
