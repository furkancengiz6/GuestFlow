# GuestFlow: Teknik Spesifikasyonlar ve Mimari

Bu belge, GuestFlow platformunun teknolojik altyapısını, mimari kararlarını ve kullanılan kütüphaneleri detaylandırır.

## 1. Teknoloji Yığını (Tech Stack)

### Arka Uç (Backend)

- **Framework**: .NET 8 (LTSP)
- **Dil**: C# 12
- **Veritabanı (RDBMS)**: Microsoft SQL Server
- **ORM**: Entity Framework Core 8.0
- **Veritabanı (Graph)**: Neo4j (Intelligence Layer için ilişki haritalama)
- **Önbellekleme**: Redis (Performans ve Rate Limiting için)
- **Gerçek Zamanlı İletişim**: ASP.NET Core SignalR (Canlı operasyon akışı)
- **Loglama**: Serilog (Console, File ve Seq entegrasyonu)

### Ön Uç (Frontend)

- **Kütüphane**: React 18
- **Build Tool**: Vite 5
- **Dil**: TypeScript
- **UI Framework**: Material-UI (MUI) v5
- **Durum Yönetimi**: Zustand (Persist middleware ile)
- **Veri Çekme**: Axios (İnterseptör tabanlı yetkilendirme yönetimi)
- **Form Yönetimi**: React Hook Form & FluentValidation

---

## 2. Mimari Yapı

GuestFlow, **N-Layered Clean Architecture** prensiplerine göre yapılandırılmıştır:

- **GuestFlow.Api**: RESTful uç noktalar, Middleware'ler ve API yapılandırması.
- **GuestFlow.Application**: İş mantığı (Business Logic), servisler, DTO'lar ve AutoMapper eşleşmeleri.
- **GuestFlow.Domain**: Varlıklar (Entities), arayüz tanımları ve temel iş kuralları.
- **GuestFlow.Persistence**: EF Core DbContext, konfigürasyonlar, Repository ve UnitOfWork implementasyonları.
- **GuestFlow.Frontend**: Modüler React bileşenleri ve modern UI katmanı.

---

## 3. Güvenlik ve Uyumluluk

Platform, kurumsal düzeyde güvenlik standartlarını destekler:

- **Yetkilendirme**: JWT (JSON Web Token) tabanlı stateless authentication.
- **Yetki Kontrolü**: Role-Based Access Control (RBAC) ve granüler izin sistemi.
- **Veri Koruması**:
  - Hassas veriler için **PII (Personally Identifiable Information)** koruma servisleri.
  - KVKK/GDPR uyumlu veri anonimleştirme desteği.
- **Güvenlik Katmanları**:
  - Rate Limiting (IP ve Kullanıcı tabanlı).
  - Security Headers (CSP, HSTS).
  - Brute-force koruması ve Audit Logging.

---

## 4. Akıllı Özellikler (Intelligence Layer)

GuestFlow, standart bir CRM'den farklı olarak ileri seviye analitik yeteneklere sahiptir:

- **Behavioral Data Collection**: Misafir ve personel davranışlarının takibi.
- **Graph Database**: Neo4j kullanarak misafirler arası ilişkilerin ve tercihler haritasının çıkarılması.
- **Predictive Analytics**: Geçmiş verilerden yola çıkarak gelecek operasyonel yük tahmini.

---

## 5. Dağıtım ve DevOps

- **Containerization**: Docker & Docker-Compose desteği.
- **Orkestrasyon**: Kubernetes (k8s) konfigürasyonları hazırdır.
- **CI/CD**: Azure DevOps hatları mevcuttur.
