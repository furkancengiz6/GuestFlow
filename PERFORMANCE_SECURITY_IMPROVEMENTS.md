# GuestFlow - Performance & Güvenlik İyileştirmeleri

## 📊 Analiz Raporu (Detaylı İnceleme Sonucu)

### 🔍 Mevcut Durum Değerlendirmesi

**Performance Sorunları:**
- Frontend bundle size: ~2.5MB (Material-UI, Recharts büyük etkisi)
- Database N+1 query potansiyeli (Repository'de Include kullanımı)
- Image optimizasyonu eksik
- Caching stratejisi temel seviyede

**Güvenlik Notu (Güncel Durum)**

Bu dokümanın bazı kısımları tarihsel analiz niteliğindedir. Kod tabanında artık:
- ✅ Input sanitization (XSS koruması) mevcut (`HtmlSanitization` middleware)
- ✅ Audit logging mevcut (EF interceptor)
- ✅ Security headers mevcut (CSP dahil)
- ✅ Rate limiting mevcut (dev/test bypass olabilir)

Güncel “source of truth” için: `ROADMAP.md`, `QA_TEST_REPORT.md` ve `IMPLEMENTATION_SUMMARY.md`.

---

## 🚀 Performance İyileştirmeleri

### 1. 🎯 Frontend Bundle Size Optimizasyonu

#### **Mevcut Durum:**
- Material-UI: ~1.2MB
- Recharts: ~350KB
- date-fns: ~150KB
- Toplam: ~2.5MB

#### **İyileştirme Önerileri:**

```typescript
// 1. Dynamic imports ile code splitting
const DashboardPage = lazy(() => import('./pages/Dashboard/DashboardPage'))
const GuestsPage = lazy(() => import('./pages/Guests/GuestsPage'))

// 2. Tree shaking için selective imports
import { Button, TextField } from '@mui/material' // ❌ Tüm MUI
import Button from '@mui/material/Button'         // ✅ Sadece Button
import TextField from '@mui/material/TextField'

// 3. Ağır kütüphaneleri lazy load
const ChartsPage = lazy(() => import('./pages/ChartsPage')) // Recharts sadece gerektiğinde
```

#### **Beklenen Kazanç:**
- İlk yükleme: %40-50 azaltma
- Time to Interactive: %30-40 iyileşme

### 2. 🗄️ Database Query Optimizasyonu

#### **Mevcut Sorunlar:**
```csharp
// N+1 Query Örneği (Repository'de potansiyel)
var guests = await _guestRepository.GetAllAsync();
foreach (var guest in guests)
{
    var transfers = await _transferRepository.GetAllAsync(t => t.GuestId == guest.Id); // ❌ N+1
}
```

#### **İyileştirme Önerileri:**

```csharp
// 1. Eager Loading ile Include kullanımı
public async Task<List<Guest>> GetGuestsWithTransfersAsync()
{
    return await _context.Guests
        .Include(g => g.Transfers)
        .ThenInclude(t => t.Vehicle)
        .Where(g => !g.IsDeleted)
        .ToListAsync();
}

// 2. Specification Pattern ile optimize sorgular
public class GuestWithTransfersSpec : Specification<Guest>
{
    public GuestWithTransfersSpec()
    {
        AddInclude(g => g.Transfers);
        AddInclude("Transfers.Vehicle");
    }
}

// 3. Database Index'leri
[Index(nameof(GuestId), nameof(CreatedDate))] // Transfer tablosu için
public class Transfer : BaseEntity
```

#### **Beklenen Kazanç:**
- Database sorgu sayısı: %60-80 azaltma
- Response time: %50-70 iyileşme

### 3. 🖼️ Image & Asset Optimizasyonu

```typescript
// 1. Lazy loading images
<img
  loading="lazy"
  src={image.src}
  alt={image.alt}
  onLoad={() => setImageLoaded(true)}
/>

// 2. WebP formatı ve responsive images
<picture>
  <source srcset="image.webp" type="image/webp">
  <img src="image.jpg" alt="Responsive image">
</picture>

// 3. CDN kullanımı
const imageUrl = process.env.REACT_APP_CDN_URL + image.path
```

### 4. ⚡ Caching Stratejisi İyileştirme

```csharp
// 1. Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 2. Output caching
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
public async Task<IActionResult> GetDashboardData()

// 3. Application level caching
public async Task<List<City>> GetCitiesAsync()
{
    const string cacheKey = "cities";
    var cities = await _cache.GetAsync<List<City>>(cacheKey);

    if (cities == null)
    {
        cities = await _cityRepository.GetAllAsync();
        await _cache.SetAsync(cacheKey, cities, TimeSpan.FromHours(1));
    }

    return cities;
}
```

---

## 🔒 Güvenlik İyileştirmeleri

### 1. 🛡️ Input Sanitization & XSS Koruması

#### **Mevcut Durum:** ❌ YOK

#### **İyileştirme:**

```csharp
// 1. HTML sanitization middleware
public class HtmlSanitizationMiddleware
{
    private readonly RequestDelegate _next;

    public HtmlSanitizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var sanitizedBody = SanitizeHtml(body);

            var buffer = Encoding.UTF8.GetBytes(sanitizedBody);
            context.Request.Body = new MemoryStream(buffer);
            context.Request.Body.Position = 0;
        }

        await _next(context);
    }

    private string SanitizeHtml(string html)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear(); // Sadece güvenli tag'ları izin ver
        sanitizer.AllowedTags.Add("p", "br", "strong", "em");
        return sanitizer.Sanitize(html);
    }
}

// 2. Model validation attributes
public class CreateGuestRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Only letters and spaces allowed")]
    public string FirstName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; }
}
```

### 2. 📝 Audit Logging Sistemi

#### **Mevcut Durum:** ❌ YOK

#### **İyileştirme:**

```csharp
// 1. Audit entity
public class AuditLog : BaseEntity
{
    public string TableName { get; set; }
    public string Action { get; set; } // INSERT, UPDATE, DELETE
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}

// 2. Audit interceptor
public class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry);
            auditEntry.TableName = entry.Metadata.GetTableName();
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                    continue;

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            var auditLog = new AuditLog
            {
                TableName = auditEntry.TableName,
                Action = auditEntry.Action.ToString(),
                OldValues = JsonSerializer.Serialize(auditEntry.OldValues),
                NewValues = JsonSerializer.Serialize(auditEntry.NewValues),
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                IpAddress = GetClientIpAddress(),
                Timestamp = DateTime.UtcNow
            };

            context.AuditLogs.Add(auditLog);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

// 3. Program.cs'e kaydet
builder.Services.AddScoped<AuditInterceptor>();
```

### 3. 🔐 Security Headers İyileştirme

#### **Mevcut Durum:** ⚠️ Temel CSP var

#### **İyileştirme:**

```csharp
// 1. SecurityHeadersMiddleware
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Content Security Policy
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' https://api.guestflow.com; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self';";

        // Security Headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // HSTS (sadece HTTPS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        await _next(context);
    }
}

// 2. Program.cs'e ekle
app.UseMiddleware<SecurityHeadersMiddleware>();
```

### 4. 🛡️ Rate Limiting İyileştirme

#### **Mevcut Durum:** ⚠️ Temel rate limiting var

#### **İyileştirme:**

```csharp
// 1. Gelişmiş rate limiting policies
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("Strict", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("Moderate", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));
});

// 2. Controller'da kullanım
[EnableRateLimiting("Strict")]
public class AuthController : BaseController
{
    [AllowAnonymous]
    [EnableRateLimiting("Moderate")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Login logic
    }
}
```

### 5. 🔍 SQL Injection & Parameterized Queries

#### **Mevcut Durum:** ✅ EF Core parametreli sorgular (iyi)

#### **Ek İyileştirmeler:**

```csharp
// 1. Raw SQL kullanırken parameterized queries
public async Task<List<Guest>> GetGuestsByDateRange(DateTime startDate, DateTime endDate)
{
    var sql = "SELECT * FROM Guests WHERE CreatedDate BETWEEN @startDate AND @endDate";
    return await _context.Guests
        .FromSqlRaw(sql, new SqlParameter("@startDate", startDate), new SqlParameter("@endDate", endDate))
        .ToListAsync();
}

// 2. Dynamic query builder
public IQueryable<Guest> BuildGuestQuery(GuestFilter filter)
{
    var query = _context.Guests.AsQueryable();

    if (!string.IsNullOrEmpty(filter.FirstName))
        query = query.Where(g => EF.Functions.Like(g.FirstName, $"%{filter.FirstName}%"));

    if (filter.CreatedAfter.HasValue)
        query = query.Where(g => g.CreatedDate >= filter.CreatedAfter.Value);

    return query;
}
```

---

## 📋 Uygulama Planı

### **Aşama 1: Kritik Güvenlik (1-2 Hafta)**
- [ ] Input sanitization middleware implementasyonu
- [ ] Security headers iyileştirme
- [ ] XSS koruması ekleme

### **Aşama 2: Audit & Monitoring (2-3 Hafta)**
- [ ] Audit logging sistemi
- [ ] Error tracking iyileştirme
- [ ] Security event logging

### **Aşama 3: Performance Optimizasyonu (3-4 Hafta)**
- [ ] Database query optimizasyonu
- [ ] Frontend bundle size azaltma
- [ ] Image optimizasyonu

### **Aşama 4: Advanced Security (2-3 Hafta)**
- [ ] Rate limiting gelişmiş kurallar
- [ ] API key management
- [ ] Multi-factor authentication hazırlığı

---

## 🎯 Beklenen Kazançlar

### **Performance İyileştirmeleri:**
- **İlk yükleme hızı:** %40-50 artış
- **Database sorgu sayısı:** %60-80 azalma
- **Memory kullanımı:** %30-40 azalma
- **Time to Interactive:** %30-40 iyileşme

### **Güvenlik İyileştirmeleri:**
- **OWASP Top 10 compliance:** %90+ karşılanma
- **Audit coverage:** %100 (tüm kritik işlemler)
- **XSS koruması:** %100 input sanitization
- **Rate limiting:** Gelişmiş DDoS koruması

---

## 🛠️ Gereken Tools & Dependencies

```json
// Package references
"Microsoft.AspNetCore.Cors": "8.0.0",
"Microsoft.AspNetCore.RateLimiting": "8.0.0",
"Ganss.Xss": "4.0.1",  // HTML sanitization
"Serilog.AspNetCore": "8.0.0",  // Advanced logging
"Serilog.Sinks.Seq": "8.0.0",  // Log aggregation
```

```xml
<!-- NuGet packages -->
<PackageReference Include="Microsoft.AspNetCore.Cors" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="8.0.0" />
<PackageReference Include="Ganss.Xss" Version="4.0.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
```

---

**Not:** Bu iyileştirmeler GuestFlow'u enterprise-level güvenlik ve performance standartlarına çıkaracaktır. Her aşama bağımsız olarak implement edilebilir ve production'a güvenli geçiş sağlar.