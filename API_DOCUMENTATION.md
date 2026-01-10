# GuestFlow API Documentation

## 📋 Genel Bilgiler

GuestFlow API, otel ve seyahat operasyonlarını yönetmek için geliştirilmiş kapsamlı bir REST API'dir.

- **Base URL**: `https://api.guestflow.com/api/v1.0`
- **Authentication**: JWT Bearer Token
- **Rate Limit**: 100 requests/minute (authenticated), 10 requests/minute (unauthenticated)
- **Version**: API Versioning with URL versioning

## 🔐 Authentication

### Login
```http
POST /api/v1.0/auth/login
Content-Type: application/json

{
  "email": "admin@guestflow.com",
  "password": "your-password"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh-token-here",
    "expiresIn": 2700,
    "user": {
      "id": 1,
      "email": "admin@guestflow.com",
      "firstName": "Admin",
      "lastName": "User",
      "role": "Admin"
    }
  }
}
```

### Token Refresh
```http
POST /api/v1.0/auth/refresh-token
Authorization: Bearer <your-token>
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

## 👥 Misafir Yönetimi (Guests)

### Misafir Listesi
```http
GET /api/guests?page=1&pageSize=10&search=john&sortBy=firstName&sortOrder=asc
Authorization: Bearer <token>
```

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `pageSize`: Sayfa boyutu (default: 10, max: 100)
- `search`: Arama metni (isim, email, telefon)
- `sortBy`: Sıralama alanı
- `sortOrder`: Sıralama yönü (asc/desc)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@example.com",
        "phoneNumber": "+1234567890",
        "nationality": "US",
        "createdDate": "2024-01-15T10:30:00Z",
        "isActive": true
      }
    ],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Misafir Ekleme
```http
POST /api/guests
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "phoneNumber": "+1987654321",
  "nationality": "US",
  "dateOfBirth": "1990-05-15",
  "passportNumber": "P123456789",
  "specialRequests": "Vegetarian meals",
  "emergencyContactName": "John Smith",
  "emergencyContactPhone": "+1234567890"
}
```

### Misafir Güncelleme
```http
PUT /api/guests/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Johnson",
  "email": "jane.johnson@example.com",
  "phoneNumber": "+1987654321",
  "isActive": true
}
```

### Misafir Detayı
```http
GET /api/guests/{id}
Authorization: Bearer <token>
```

## 🚗 Transfer Yönetimi

### Transfer Listesi
```http
GET /api/transfers?date=2024-01-15&status=confirmed
Authorization: Bearer <token>
```

**Query Parameters:**
- `date`: Tarih (YYYY-MM-DD)
- `status`: Durum (pending, confirmed, completed, cancelled)
- `guestId`: Misafir ID
- `vehicleId`: Araç ID

### Transfer Ekleme
```http
POST /api/transfers
Authorization: Bearer <token>
Content-Type: application/json

{
  "guestId": 1,
  "transferType": "AirportToHotel",
  "pickupLocation": "Istanbul Airport (IST)",
  "dropoffLocation": "Hilton Istanbul",
  "scheduledDate": "2024-01-20T14:30:00Z",
  "vehicleId": 1,
  "passengerCount": 2,
  "specialRequests": "Child seat required",
  "estimatedPrice": 45.00,
  "currency": "USD"
}
```

### Transfer Durumu Güncelleme
```http
PATCH /api/transfers/{id}/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "status": "completed",
  "actualPickupTime": "2024-01-20T14:35:00Z",
  "actualDropoffTime": "2024-01-20T15:15:00Z",
  "finalPrice": 50.00,
  "notes": "Traffic was heavier than expected"
}
```

## 🏨 Otel Yönetimi

### Otel Listesi
```http
GET /api/hotels?page=1&pageSize=10&city=Istanbul&minStars=4
Authorization: Bearer <token>
```

### Otel Ekleme
```http
POST /api/hotels
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Hilton Istanbul",
  "address": "Cumhuriyet Cd. No:50, 34367 Şişli/İstanbul",
  "cityId": 1,
  "starRating": 5,
  "phoneNumber": "+90 212 315 6000",
  "email": "info@hilton.com",
  "website": "https://www.hilton.com",
  "amenities": ["WiFi", "Pool", "Spa", "Fitness Center"],
  "checkInTime": "14:00",
  "checkOutTime": "12:00",
  "description": "Luxury hotel in the heart of Istanbul"
}
```

## 📧 Bildirim Yönetimi

### Email Gönderme
```http
POST /api/emails/send
Authorization: Bearer <token>
Content-Type: application/json

{
  "toEmail": "guest@example.com",
  "subject": "Welcome to GuestFlow",
  "template": "welcome_email",
  "templateData": {
    "guestName": "John Doe",
    "checkInDate": "2024-01-20",
    "hotelName": "Hilton Istanbul"
  }
}
```

### SMS Gönderme
```http
POST /api/sms/send
Authorization: Bearer <token>
Content-Type: application/json

{
  "phoneNumber": "+1234567890",
  "message": "Your transfer is confirmed for tomorrow at 14:30",
  "priority": "normal"
}
```

## 📊 Raporlama

### Dashboard İstatistikleri
```http
GET /api/reports/dashboard?date=2024-01-15
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "todayTransfers": 12,
    "todayRevenue": 1250.50,
    "monthlyRevenue": 45250.75,
    "pendingPayments": 5,
    "activeGuests": 45,
    "occupancyRate": 78.5,
    "topDestinations": [
      {"city": "Istanbul", "count": 15},
      {"city": "Antalya", "count": 12}
    ]
  }
}
```

### Detaylı Raporlar
```http
GET /api/reports/transfers?startDate=2024-01-01&endDate=2024-01-31&groupBy=month
Authorization: Bearer <token>
```

## 📁 Dosya Yönetimi

### Dosya Yükleme
```http
POST /api/files/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data

# Form data:
# file: [binary file data]
# entityType: "guest" | "transfer" | "invoice"
# entityId: 123
# fileType: "image" | "document" | "pdf"
```

### Dosya İndirme
```http
GET /api/files/{id}/download
Authorization: Bearer <token>
```

## ⚙️ Sistem Ayarları

### Bakım Modu
```http
PATCH /api/v1.0/settings/maintenance
Authorization: Bearer <token> (Admin only)
Content-Type: application/json

{
  "enabled": true,
  "message": "System maintenance in progress"
}
```

### Sistem Durumu
```http
GET /api/health
```

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0",
  "services": {
    "database": "Healthy",
    "redis": "Healthy",
    "email": "Healthy"
  }
}
```

## 🔒 Güvenlik Özellikleri

### Rate Limiting
- **Authenticated users**: 100 requests/minute
- **Unauthenticated users**: 10 requests/minute
- **Login endpoint**: 5 attempts/minute

### Input Validation
- XSS protection aktif
- SQL injection prevention
- Input sanitization
- Comprehensive validation rules

### Audit Logging
- Tüm CRUD işlemler loglanır
- User activity tracking
- Security event monitoring
- IP address ve session tracking

## 📋 Hata Kodları

### Genel HTTP Status Codes
- `200`: Başarılı
- `201`: Oluşturuldu
- `400`: Geçersiz istek
- `401`: Yetkilendirme gerekli
- `403`: Yetki yok
- `404`: Bulunamadı
- `429`: Çok fazla istek (Rate limit)
- `500`: Sunucu hatası

### Özel Hata Kodları
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "email",
        "message": "Email format is invalid"
      }
    ]
  }
}
```

## 🔄 Webhook'lar (Gelecek Özellik)

```http
POST https://your-app.com/webhooks/guestflow
X-Webhook-Signature: sha256=...
Content-Type: application/json

{
  "event": "transfer.completed",
  "data": {
    "transferId": 123,
    "guestId": 456,
    "completedAt": "2024-01-20T15:15:00Z"
  }
}
```

## 📞 Destek

API ile ilgili sorunlar için:
- **Email**: api-support@guestflow.com
- **Docs**: https://docs.guestflow.com
- **Status Page**: https://status.guestflow.com

---

**Son Güncelleme**: 15 Ocak 2026
**API Version**: v1.0