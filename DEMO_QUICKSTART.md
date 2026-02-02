# GuestFlow Demo - Hızlı Başlangıç

## ⚠️ Önemli Not

Proje migration dosyalarında bazı foreign key constraint sorunları bulunmaktadır. Demo için en kolay yol frontend'i standalone modda çalıştırmaktır.

## Seçenek 1: Frontend Standalone Demo (Önerilen)

Frontend mock data ile çalışabilir:

```bash
cd GuestFlow.Frontend
npm install
npm run dev
```

Frontend: <http://localhost:5173>

## Seçenek 2: Backend ile Tam Demo

Eğer veritabanı problemlerini çözmek istiyorsanız:

### Manuel Veritabanı Hazırlama

1. **SQL Server Management Studio (SSMS)** ile bağlanın
2. `GuestFlowDb` veritabanını manuel oluşturun
3. Migration dosyalarını düzeltin veya tabloları manuel oluşturun

### Migration Sorunlarını Çözme

```bash
# Tüm migration dosyalarını sil
Remove-Item -Recurse -Force GuestFlow.Persistence\Migrations\*

# Model'den yeni migration oluştur
dotnet ef migrations add InitialCreate --project GuestFlow.Persistence --startup-project GuestFlow.Api

# Veritabanını sil (varsa)
dotnet ef database drop --force --project GuestFlow.Persistence --startup-project GuestFlow.Api

# Yeni migration'ı uygula
dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
```

### Demo Verilerini Yükle

```bash
cd GuestFlow.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:SeedDemoData="true"
dotnet run
```

**konsol çıktısındaki demo kullanıcı şifrelerini kaydedin!**

## Seçenek 3: Docker ile Demo

```bash
docker-compose up -d
```

- Frontend: <http://localhost>
- Backend API: <http://localhost:5000>
- Swagger: <http://localhost:5000/swagger>

## Sorun Giderme

### "Cannot find the object" hatası

Migration dosyalarında sıralama problemi var. Tüm migration'ları silip yeniden oluşturun.

### "Could not create constraint" hatası  

Foreign key bağımlılıkları çakışıyor. Tabloları manuel oluşturmayı deneyin.

### Frontend API'ye bağlanamıyor

- Backend'in çalıştığını kontrol edin: <http://localhost:5146/health>
- CORS ayarlarını kontrol edin
- `.env` dosyasında `VITE_API_BASE_URL` doğru olmalı

## API Test (Swagger)

Backend çalıştıktan sonra:

1. <http://localhost:5146/swagger>
2. Login endpoint ile token alın
3. "Authorize" butonuna token'ı yapıştırın
4. Endpoint'leri test edin

## Önerilen Workflow

**İlk Kez Kullanıcılar için:**

1. Frontend standalone modda başlatın (`npm run dev`)
2. UI'yi keşfedin
3. İhtiyaç duyarsanız backend'i ekleyin

**Geliştiriciler için:  

1. Migration sorunlarını çözün (yukarıdaki adımlar)
2. Backend + Frontend birlikte çalıştırın
3. Database seed ile demo verilerini yükleyin

---

**Not:** Migration sorunları, projedeki karmaşık entity ilişkilerinden kaynaklanmaktadır. Production ortamında migration dosyaları manuel gözden geçirilmelidir.

**Güncelleme:** 2026-01-24
