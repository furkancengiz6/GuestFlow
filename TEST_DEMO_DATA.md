# Demo Veri Test Rehberi

Bu rehber, oluşturulan demo verilerin nasıl test edileceğini adım adım açıklar.

---

## 📋 Ön Hazırlık

### 1. Veritabanı Bağlantısını Kontrol Edin

`GuestFlow.Api/appsettings.json` dosyasında connection string'in doğru olduğundan emin olun:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=GuestFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Not**: SQL Server Express'in çalıştığından emin olun.

---

## 🚀 Adım 1: Uygulamayı Çalıştırın

### Terminal'den Çalıştırma

```bash
cd GuestFlow.Api
dotnet run --launch-profile http
```

veya

```bash
dotnet run --project GuestFlow.Api --launch-profile http
```

### Visual Studio'dan Çalıştırma

1. `GuestFlow.Api` projesini startup project olarak ayarlayın
2. `http` profilini seçin
3. F5 ile çalıştırın

### Beklenen Çıktı

Uygulama başladığında konsolda şu logları görmelisiniz:

```
Demo veri oluşturma başlatılıyor...
8 şehir eklendi.
4 havaalanı eklendi.
7 araç eklendi.
4 personel eklendi.
12 misafir eklendi.
25 transfer eklendi.
15 şehir turu eklendi.
12 yat turu eklendi.
42 fatura eklendi.
35 ödeme eklendi.
60 günlük gelir eklendi.
30 günlük not eklendi.
Demo veri başarıyla oluşturuldu!
```

**Not**: Eğer veriler zaten varsa, "zaten mevcut, atlanıyor" mesajları görünecektir.

---

## 🔍 Adım 2: Veritabanında Verileri Kontrol Edin

### SQL Server Management Studio (SSMS) ile

1. SSMS'i açın
2. `localhost\SQLEXPRESS` sunucusuna bağlanın
3. `GuestFlowDb` veritabanını genişletin
4. Tabloları kontrol edin:

```sql
-- Şehir sayısını kontrol et
SELECT COUNT(*) FROM Cities;
-- Beklenen: 8

-- Havaalanı sayısını kontrol et
SELECT COUNT(*) FROM Airports;
-- Beklenen: 4

-- Araç sayısını kontrol et
SELECT COUNT(*) FROM Vehicles;
-- Beklenen: 7

-- Personel sayısını kontrol et
SELECT COUNT(*) FROM Personnels;
-- Beklenen: 4

-- Misafir sayısını kontrol et
SELECT COUNT(*) FROM Guests;
-- Beklenen: 12

-- Transfer sayısını kontrol et
SELECT COUNT(*) FROM Transfers;
-- Beklenen: 25

-- Şehir turu sayısını kontrol et
SELECT COUNT(*) FROM CityTours;
-- Beklenen: 15

-- Yat turu sayısını kontrol et
SELECT COUNT(*) FROM YachtTours;
-- Beklenen: 12

-- Fatura sayısını kontrol et
SELECT COUNT(*) FROM Invoices;
-- Beklenen: ~42

-- Ödeme sayısını kontrol et
SELECT COUNT(*) FROM Payments;
-- Beklenen: ~35

-- Günlük gelir sayısını kontrol et
SELECT COUNT(*) FROM DailyRevenues;
-- Beklenen: 60

-- Günlük not sayısını kontrol et
SELECT COUNT(*) FROM DailyNotes;
-- Beklenen: 30
```

### Örnek Veri Sorguları

```sql
-- Personel bilgilerini görüntüle
SELECT Id, FullName, Email, UserType FROM Personnels;

-- Misafir bilgilerini görüntüle
SELECT Id, FullName, Email, Nationality, IsSpecialGuest FROM Guests;

-- Transfer detaylarını görüntüle
SELECT 
    t.Id,
    g.FullName AS GuestName,
    v.PlateNumber AS VehiclePlate,
    t.PickupAddress,
    t.DropoffAddress,
    t.Status,
    t.FinalPrice,
    t.Currency
FROM Transfers t
INNER JOIN Guests g ON t.GuestId = g.Id
INNER JOIN Vehicles v ON t.VehicleId = v.Id;

-- Fatura ve ödeme bilgilerini görüntüle
SELECT 
    i.InvoiceNumber,
    g.FullName AS GuestName,
    i.TotalAmount,
    i.Currency,
    p.PaymentNumber,
    p.Status AS PaymentStatus,
    p.PaymentMethod
FROM Invoices i
INNER JOIN Guests g ON i.GuestId = g.Id
LEFT JOIN Payments p ON p.InvoiceId = i.Id;
```

---

## 🌐 Adım 3: Swagger UI'dan Test Edin

### 1. Swagger UI'ı Açın

Uygulama çalıştıktan sonra tarayıcıda şu adresi açın:

```
http://localhost:5146/swagger
```

### 2. Login Yapın

1. **`POST /api/v1/auth/login`** endpoint'ini bulun
2. "Try it out" butonuna tıklayın
3. Şu bilgileri girin:

```json
{
  "email": "ahmet@guestflow.com",
  "password": "Admin123!"
}
```

4. "Execute" butonuna tıklayın
5. Response'dan `token` değerini kopyalayın

### 3. Token ile Yetkilendirme

1. Swagger UI'ın sağ üst köşesindeki **"Authorize"** butonuna tıklayın
2. `Bearer {token}` formatında token'ı yapıştırın (örnek: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`)
3. "Authorize" butonuna tıklayın
4. "Close" butonuna tıklayın

### 4. Endpoint'leri Test Edin

#### Misafirleri Listele

1. **`GET /api/v1/guests`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da 12 misafir görmelisiniz

#### Araçları Listele

1. **`GET /api/v1/vehicles`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da 7 araç görmelisiniz

#### Transferleri Listele

1. **`GET /api/v1/transfers`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da 25 transfer görmelisiniz

#### Şehir Turlarını Listele

1. **`GET /api/v1/citytours`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da 15 şehir turu görmelisiniz

#### Yat Turlarını Listele

1. **`GET /api/v1/yachttours`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da 12 yat turu görmelisiniz

#### Faturaları Listele

1. **`GET /api/v1/invoices`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da ~42 fatura görmelisiniz

#### Ödemeleri Listele

1. **`GET /api/v1/payments`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da ~35 ödeme görmelisiniz

#### Dashboard Verilerini Görüntüle

1. **`GET /api/v1/dashboard/overview`** endpoint'ini bulun
2. "Try it out" → "Execute"
3. Response'da dashboard istatistikleri görmelisiniz

---

## 🧪 Adım 4: Postman/Insomnia ile Test Edin

### 1. Login Request

```http
POST http://localhost:5146/api/v1/auth/login
Content-Type: application/json

{
  "email": "ahmet@guestflow.com",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Giriş başarılı",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresIn": 2700
  }
}
```

### 2. Token ile İstek Gönderme

```http
GET http://localhost:5146/api/v1/guests
Authorization: Bearer {token}
```

### 3. Sayfalama ile İstek

```http
GET http://localhost:5146/api/v1/guests?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

---

## 🎯 Adım 5: Frontend'den Test Edin

### 1. Frontend'i Çalıştırın

```bash
cd GuestFlow.Frontend
npm install
npm run dev
```

### 2. Login Yapın

1. Tarayıcıda `http://localhost:5173` (veya frontend portu) açın
2. Login sayfasında:
   - Email: `ahmet@guestflow.com`
   - Password: `Admin123!`
3. Giriş yapın

### 3. Dashboard'u Kontrol Edin

- Dashboard'da istatistikler görünmeli
- Grafikler veri göstermeli
- Son rezervasyonlar listelenmeli

### 4. Modülleri Test Edin

- **Misafirler**: 12 misafir listelenmeli
- **Araçlar**: 7 araç listelenmeli
- **Transferler**: 25 transfer listelenmeli
- **Turlar**: Şehir ve yat turları görünmeli
- **Faturalar**: Faturalar listelenmeli
- **Ödemeler**: Ödemeler görünmeli

---

## 🔐 Test Kullanıcıları

### Admin Kullanıcı
- **Email**: `ahmet@guestflow.com`
- **Password**: `Admin123!`
- **Rol**: Admin

### Staff Kullanıcılar
- **Email**: `ayse@guestflow.com`
- **Password**: `Staff123!`
- **Rol**: Staff

- **Email**: `mehmet@guestflow.com`
- **Password**: `Staff123!`
- **Rol**: Staff

- **Email**: `zeynep@guestflow.com`
- **Password**: `Staff123!`
- **Rol**: Staff

---

## ✅ Kontrol Listesi

### Veritabanı Kontrolleri
- [ ] Uygulama başarıyla çalıştı
- [ ] Seed logları konsolda göründü
- [ ] Veritabanında tablolar oluşturuldu
- [ ] Tüm tablolarda veri var

### API Kontrolleri
- [ ] Login endpoint çalışıyor
- [ ] Token alınabiliyor
- [ ] Token ile protected endpoint'lere erişilebiliyor
- [ ] GET endpoint'leri veri döndürüyor
- [ ] Sayfalama çalışıyor

### Frontend Kontrolleri
- [ ] Login yapılabiliyor
- [ ] Dashboard verileri görünüyor
- [ ] Listeler dolu görünüyor
- [ ] Detay sayfaları açılıyor

---

## 🐛 Sorun Giderme

### Veriler Oluşmadı

1. **Konsol loglarını kontrol edin**
   - Hata mesajları var mı?
   - Seed işlemi başladı mı?

2. **Veritabanı bağlantısını kontrol edin**
   - SQL Server çalışıyor mu?
   - Connection string doğru mu?

3. **Migration'ları kontrol edin**
   ```bash
   dotnet ef database update --project GuestFlow.Persistence --startup-project GuestFlow.Api
   ```

### Token Alınamıyor

1. **Kullanıcı bilgilerini kontrol edin**
   - Email ve password doğru mu?
   - Kullanıcı veritabanında var mı?

2. **JWT ayarlarını kontrol edin**
   - `appsettings.json`'da JWT ayarları doğru mu?

### Endpoint'ler 401 Unauthorized Döndürüyor

1. **Token'ı kontrol edin**
   - Token geçerli mi?
   - Token süresi dolmuş mu?

2. **Authorization header'ı kontrol edin**
   - `Bearer {token}` formatında mı?
   - Boşluk var mı?

---

## 📊 Beklenen Veri Sayıları

| Tablo | Beklenen Kayıt Sayısı |
|-------|----------------------|
| Cities | 8 |
| Airports | 4 |
| Vehicles | 7 |
| Personnels | 4 |
| Guests | 12 |
| Transfers | 25 |
| CityTours | 15 |
| YachtTours | 12 |
| Invoices | ~42 |
| Payments | ~35 |
| DailyRevenues | 60 |
| DailyNotes | 30 |

---

## 🎉 Başarılı Test Sonucu

Eğer tüm adımlar başarılı olduysa:

✅ Veritabanında tüm tablolar oluşturuldu  
✅ Demo veriler başarıyla eklendi  
✅ API endpoint'leri çalışıyor  
✅ Frontend verileri görüntülüyor  
✅ Login ve yetkilendirme çalışıyor  

Artık projenizi demo verilerle test edebilirsiniz! 🚀

