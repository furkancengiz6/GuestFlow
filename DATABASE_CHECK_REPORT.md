# Veritabanı Kontrol Raporu

> ⚠️ Not (Güncellik): Bu rapor tarihsel analiz içerebilir. Güncel QA durumu için `QA_TEST_REPORT.md` ve mevcut migration’lar için `GuestFlow.Persistence/Migrations/` referans alın.

## ✅ Bağlantı Yapılandırması

### Connection String
- **Durum**: ✅ Yapılandırılmış
- **Konum**: `GuestFlow.Api/appsettings.json`
- **Connection String**: 
  ```json
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=GuestFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  ```
- **Not**: SQL Server Express kullanılıyor. Veritabanı adı: `GuestFlowDb`

### DbContext Yapılandırması
- **Durum**: ✅ Doğru yapılandırılmış
- **Konum**: `GuestFlow.Api/Program.cs` (Satır 157-158)
- **Kod**:
  ```csharp
  builder.Services.AddDbContext<GuestFlowDbContext>(options =>
      options.UseSqlServer(cs, x => x.MigrationsAssembly("GuestFlow.Persistence")));
  ```
- **Migrations Assembly**: ✅ Doğru belirtilmiş (`GuestFlow.Persistence`)

---

## ⚠️ Migration Dosyaları Durumu

### Mevcut Migration'lar
1. ✅ `20250408150045_InitialC` - İlk migration (temel tablolar)
2. ✅ `20250408150355_UpdatePasswordLength` - Password uzunluğu güncellemesi

### Migration Dosyaları İçeriği
- ✅ `InitialC.cs` - Temel tablolar oluşturulmuş
- ✅ `UpdatePasswordLength.cs` - Password kolonu güncellenmiş
- ✅ `GuestFlowDbContextModelSnapshot.cs` - Model snapshot mevcut

---

## ❌ KRİTİK SORUN: Eksik Tablolar

### DbContext'te Tanımlı Ama Migration'da Olmayan Tablolar

Aşağıdaki entity'ler `GuestFlowDbContext` içinde `DbSet` olarak tanımlı ancak **hiçbir migration dosyasında tablo oluşturulmamış**:

1. ❌ **RefreshTokens** (`RefreshTokenEntity`)
   - DbContext'te: ✅ Var (Satır 27)
   - Migration'da: ❌ Yok
   - Configuration: ✅ Var (`RefreshTokenConfiguration`)

2. ❌ **EmailQueues** (`EmailQueueEntity`)
   - DbContext'te: ✅ Var (Satır 28)
   - Migration'da: ❌ Yok
   - Configuration: ❌ Yok (Entity'de Configuration sınıfı tanımlı değil)

3. ❌ **EmailTemplates** (`EmailTemplateEntity`)
   - DbContext'te: ✅ Var (Satır 29)
   - Migration'da: ❌ Yok
   - Configuration: ❌ Yok (Entity'de Configuration sınıfı tanımlı değil)

4. ❌ **EmailHistories** (`EmailHistoryEntity`)
   - DbContext'te: ✅ Var (Satır 30)
   - Migration'da: ❌ Yok
   - Configuration: ❌ Yok (Entity'de Configuration sınıfı tanımlı değil)

5. ❌ **Reservations** (`ReservationEntity`)
   - DbContext'te: ✅ Var (Satır 31)
   - Migration'da: ❌ Yok
   - Configuration: ✅ Var (`ReservationConfiguration`)

6. ❌ **Payments** (`PaymentEntity`)
   - DbContext'te: ✅ Var (Satır 32)
   - Migration'da: ❌ Yok
   - Configuration: ✅ Var (`PaymentConfiguration`)

7. ❌ **SmsHistories** (`SmsHistoryEntity`)
   - DbContext'te: ✅ Var (Satır 33)
   - Migration'da: ❌ Yok
   - Configuration: ✅ Var (`SmsHistoryConfiguration`)

---

## ✅ Configuration Sınıfları

Tüm entity'ler için Configuration sınıfları mevcut ve doğru yapılandırılmış:

- ✅ `AirportConfiguration`
- ✅ `CityConfiguration`
- ✅ `CityTourConfiguration`
- ✅ `DailyNoteConfiguration`
- ✅ `DailyRevenueConfiguration`
- ✅ `GuestConfiguration`
- ✅ `InvoicesConfiguration`
- ✅ `PersonnelConfiguration`
- ✅ `TransferConfiguration`
- ✅ `VehicleConfiguration`
- ✅ `YachtTourConfiguration`
- ✅ `GuestYachtTourConfiguration`
- ✅ `GuestCityTourConfiguration`
- ✅ `RefreshTokenConfiguration`
- ✅ `ReservationConfiguration`
- ✅ `PaymentConfiguration`
- ✅ `SmsHistoryConfiguration`

**Not**: EmailQueue, EmailTemplate, EmailHistory için Configuration sınıfları **YOK**. Bu entity'ler için Configuration sınıfları oluşturulmalı veya migration sırasında Fluent API ile yapılandırılmalı.

---

## 🔴 Update-Database Durumu

### Mevcut Durum
**Update-Database komutu ŞU ANDA HATASIZ ÇALIŞMAZ!**

### Neden?
1. DbContext'te 7 tablo tanımlı ama migration'da yok
2. Uygulama çalıştığında bu tablolara erişmeye çalışacak
3. Tablolar olmadığı için `SqlException` hatası alınacak

### Hata Örneği
```
SqlException: Invalid object name 'RefreshTokens'
SqlException: Invalid object name 'EmailQueues'
SqlException: Invalid object name 'EmailTemplates'
SqlException: Invalid object name 'EmailHistories'
SqlException: Invalid object name 'Reservations'
SqlException: Invalid object name 'Payments'
SqlException: Invalid object name 'SmsHistories'
```

---

## ✅ Çözüm: Yeni Migration Oluşturma

### Adımlar

1. **Package Manager Console'da** (Default Project: `GuestFlow.Api`):
   ```powershell
   Add-Migration AddMissingTables -Project GuestFlow.Persistence -StartupProject GuestFlow.Api
   ```

2. **Veya .NET CLI ile**:
   ```bash
   dotnet ef migrations add AddMissingTables --project GuestFlow.Persistence --startup-project GuestFlow.Api
   ```

3. **Migration'ı kontrol edin**:
   - Oluşturulan migration dosyasında 7 tablo için `CreateTable` komutları olmalı
   - Foreign key ilişkileri doğru tanımlanmış olmalı

4. **Veritabanını güncelleyin**:
   ```powershell
   Update-Database -Project GuestFlow.Persistence -StartupProject GuestFlow.Api
   ```

   **Veya**:
   ```bash
   dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
   ```

---

## 📋 Kontrol Listesi

### Öncelikli İşlemler
- [ ] Yeni migration oluştur (`AddMissingTables`)
- [ ] Migration dosyasını kontrol et (7 tablo için CreateTable olmalı)
- [ ] Update-Database komutunu çalıştır
- [ ] Veritabanında tabloların oluştuğunu doğrula
- [ ] Uygulamayı çalıştır ve bağlantıyı test et

### İkincil Kontroller
- [ ] EmailQueue, EmailTemplate, EmailHistory için Configuration sınıfları oluştur (veya migration'da Fluent API ile yapılandır)
- [ ] Tüm foreign key ilişkilerinin doğru olduğunu kontrol et
- [ ] Index'lerin doğru oluşturulduğunu kontrol et
- [ ] EmailTemplate.Name için unique index ekle
- [ ] EmailQueue için Status ve Priority index'leri ekle

---

## 📊 Özet

| Kategori | Durum | Açıklama |
|----------|-------|----------|
| Connection String | ✅ | Doğru yapılandırılmış |
| DbContext | ✅ | Doğru yapılandırılmış |
| Mevcut Migration'lar | ✅ | İlk migration ve password update mevcut |
| Eksik Tablolar | ❌ | 7 tablo migration'da yok |
| Configuration Sınıfları | ✅ | Tümü mevcut |
| Update-Database | ❌ | Şu anda çalışmaz (eksik tablolar) |

---

## 🎯 Sonuç

**Veritabanı bağlantısı yapılandırması doğru**, ancak **migration dosyaları eksik**. 

7 yeni tablo için migration oluşturulması gerekiyor:
1. RefreshTokens
2. EmailQueues
3. EmailTemplates
4. EmailHistories
5. Reservations
6. Payments
7. SmsHistories

Bu migration oluşturulup uygulandıktan sonra `Update-Database` hatasız çalışacaktır.

