# GuestFlow API - Test Port Bilgileri

## 🚀 Localhost Test Portları

### Ana API Portları (launchSettings.json)

#### HTTP Profili
- **URL**: `http://localhost:5145`
- **Swagger**: `http://localhost:5145/swagger`
- **Kullanım**: Development test için

#### HTTPS Profili
- **HTTPS URL**: `https://localhost:7020`
- **HTTP URL**: `http://localhost:5145`
- **Swagger**: `https://localhost:7020/swagger`
- **Kullanım**: Production benzeri test için (SSL sertifikası gerekir)

#### IIS Express Profili
- **HTTP URL**: `http://localhost:27752`
- **HTTPS URL**: `https://localhost:44309`
- **Swagger**: `http://localhost:27752/swagger`
- **Kullanım**: IIS Express ile test için

### Yapılandırma Dosyalarındaki Portlar

#### appsettings.json
- **JWT Audience**: `http://localhost:5145` ✅ (güncellendi)
- **EmailSettings BaseUrl**: `http://localhost:5145` ✅ (güncellendi)
- **FileSettings BaseUrl**: `http://localhost:5145` ✅ (güncellendi)

### Test Senaryoları

#### 1. Swagger UI ile Test
```bash
# HTTP ile çalıştır
dotnet run --launch-profile http

# Tarayıcıda aç
http://localhost:5145/swagger
```

#### 2. Postman/Insomnia ile Test
```
Base URL: http://localhost:5145
Swagger JSON: http://localhost:5145/swagger/v1/swagger.json
```

#### 3. cURL ile Test
```bash
# Health check
curl http://localhost:5145/api/health

# Login
curl -X POST http://localhost:5145/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

### Önemli Notlar

1. **Port Çakışması**: Eğer port kullanılıyorsa, `launchSettings.json` dosyasındaki port numaralarını değiştirebilirsiniz.

2. **HTTPS Sertifikası**: HTTPS profilini kullanmak için development sertifikası gerekir:
   ```bash
   dotnet dev-certs https --trust
   ```

3. **CORS Ayarları**: Frontend'den test ederken CORS ayarlarını kontrol edin.

4. **Veritabanı**: SQL Server'ın `localhost\SQLEXPRESS` üzerinde çalıştığından emin olun.

### Hızlı Test Komutları

```bash
# Projeyi çalıştır (HTTP)
dotnet run --project GuestFlow.Api --launch-profile http

# Projeyi çalıştır (HTTPS)
dotnet run --project GuestFlow.Api --launch-profile https

# Swagger'ı aç
start http://localhost:5145/swagger
```

### API Endpoint Örnekleri

```
# Authentication
POST http://localhost:5145/api/auth/login
POST http://localhost:5145/api/auth/register
POST http://localhost:5145/api/auth/refresh-token
POST http://localhost:5145/api/auth/revoke-token

# Personnel
GET http://localhost:5145/api/personnel
POST http://localhost:5145/api/personnel

# Guests
GET http://localhost:5145/api/guests
POST http://localhost:5145/api/guests

# Invoices
GET http://localhost:5145/api/invoices
POST http://localhost:5145/api/invoices/{id}/generate-pdf

# Files
POST http://localhost:5145/api/files/upload
GET http://localhost:5145/api/files

# Emails
GET http://localhost:5145/api/emails/statistics
POST http://localhost:5145/api/emails/queue
```

