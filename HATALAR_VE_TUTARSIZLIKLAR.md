# GuestFlow Projesi - Hatalar ve Tutarsızlıklar Raporu

**Oluşturulma Tarihi:** 2025-01-08  
**Proje:** GuestFlow  
**Kapsam:** Backend (GuestFlow.Api, GuestFlow.Application, GuestFlow.Domain)

---

## 📋 İçindekiler

1. [AutoMapper Mapping Eksiklikleri](#1-automapper-mapping-eksiklikleri)
2. [Manuel Mapping Kullanımları](#2-manuel-mapping-kullanımları)
3. [Tutarsızlıklar](#3-tutarsızlıklar)
4. [Eksik Update DTO Mapping'leri](#4-eksik-update-dto-mappingleri)
5. [Öncelik Sırası ve Öneriler](#5-öncelik-sırası-ve-öneriler)

---

## 1. AutoMapper Mapping Eksiklikleri

### 1.1. AirportEntity → GetAirportDto
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/Airport/AirportManager.cs` (satır 181-188)
- `GuestFlow.Application/Operations/Airport/AirportManager.cs` (satır 207, 233) - List mapping'ler AutoMapper kullanıyor ama GetById manuel

**Sorun:** `GetAirportById` metodu manuel mapping kullanıyor, ancak `GetAirports` ve `GetAirportsPaged` AutoMapper kullanıyor. Bu tutarsızlık yaratıyor.

**Çözüm:**
```csharp
// MappingProfile.cs'ye eklenecek:
CreateMap<AirportEntity, GetAirportDto>();
CreateMap<AddAirportDto, AirportEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.City, opt => opt.Ignore());
CreateMap<UpdateAirportDto, AirportEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.City, opt => opt.Ignore());
```

---

### 1.2. CityEntity → GetCityDto
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/City/CityManager.cs` (satır 164-169)
- `GuestFlow.Application/Operations/City/CityManager.cs` (satır 185, 210) - List mapping'ler AutoMapper kullanıyor

**Sorun:** `GetCityById` metodu manuel mapping kullanıyor, ancak `GetCities` ve `GetCitiesPaged` AutoMapper kullanıyor.

**Çözüm:**
```csharp
// MappingProfile.cs'ye eklenecek:
CreateMap<CityEntity, GetCityDto>();
CreateMap<AddCityDto, CityEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.Airports, opt => opt.Ignore());
CreateMap<UpdateCityDto, CityEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.Airports, opt => opt.Ignore());
```

---

### 1.3. InvoicesEntity → GetInvoiceDto
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/Invoice/InvoiceManager.cs` (satır 83-98, 120, 158, 356)

**Sorun:** Tüm `GetInvoice*` metodları manuel mapping kullanıyor. `InvoiceDetailDto` için mapping var ama `GetInvoiceDto` için yok.

**Çözüm:**
```csharp
// MappingProfile.cs'ye eklenecek:
CreateMap<InvoicesEntity, GetInvoiceDto>();
// InvoiceDetailDto zaten var (satır 155)
```

---

### 1.4. DailyRevenueEntity → GetDailyRevenueDto
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/DailyRevenue/DailyRevenueManager.cs` (satır 166-172, 189)

**Sorun:** Tüm `GetDailyRevenue*` metodları manuel mapping kullanıyor.

**Çözüm:**
```csharp
// MappingProfile.cs'ye eklenecek:
CreateMap<DailyRevenueEntity, GetDailyRevenueDto>();
CreateMap<AddDailyRevenueDto, DailyRevenueEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
CreateMap<UpdateDailyRevenueDto, DailyRevenueEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
```

---

### 1.5. DailyNoteEntity → GetDailyNoteDto
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/DailyNote/DailyNoteManager.cs` (satır 171-179, 194)

**Sorun:** Tüm `GetDailyNote*` metodları manuel mapping kullanıyor.

**Çözüm:**
```csharp
// MappingProfile.cs'ye eklenecek:
CreateMap<DailyNoteEntity, GetDailyNoteDto>();
CreateMap<AddDailyNoteDto, DailyNoteEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.Personnel, opt => opt.Ignore());
CreateMap<UpdateDailyNoteDto, DailyNoteEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.Personnel, opt => opt.Ignore());
```

---

### 1.6. GuestEntity → GetGuestDto (Kısmi)
**Durum:** ⚠️ Kısmi  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/Guest/GuestManager.cs` (satır 243-253, 269, 303)

**Sorun:** `GetGuestById` ve `GetGuests` metodları manuel mapping kullanıyor, ancak mapping profile'da `CreateMap<GuestEntity, GetGuestDto>()` tanımlı (satır 29). Bu tutarsızlık yaratıyor.

**Çözüm:** Manuel mapping'leri kaldırıp AutoMapper kullanılmalı.

---

### 1.7. VehicleEntity → GetVehicleDto (Kısmi)
**Durum:** ⚠️ Kısmi (Yeni düzeltildi)  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/Vehicle/VehicleManager.cs` (satır 157-165)

**Sorun:** `GetVehicleById` metodu manuel mapping kullanıyor, ancak `GetVehicles` ve `GetVehiclesPaged` AutoMapper kullanıyor. Mapping profile'a eklendi ama `GetVehicleById` hala manuel.

**Çözüm:** `GetVehicleById` metodundaki manuel mapping'i kaldırıp AutoMapper kullanılmalı.

---

### 1.8. CityTourEntity → GetCityTourDto (Kısmi)
**Durum:** ⚠️ Kısmi  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/CityTour/CityTourManager.cs` (satır 407-418)

**Sorun:** `GetCityTourById` metodu manuel mapping kullanıyor, ancak `GetCityTours` ve `GetCityToursPaged` AutoMapper kullanıyor.

**Çözüm:** `GetCityTourById` metodundaki manuel mapping'i kaldırıp AutoMapper kullanılmalı.

---

### 1.9. YachtTourEntity → GetYachtTourDto (Kısmi)
**Durum:** ⚠️ Kısmi  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosyalar:**
- `GuestFlow.Application/Operations/YachtTour/YachtTourManager.cs` (satır 364-376)

**Sorun:** `GetYachtTourById` metodu manuel mapping kullanıyor, ancak `GetYachtTours` ve `GetYachtToursPaged` AutoMapper kullanıyor.

**Çözüm:** `GetYachtTourById` metodundaki manuel mapping'i kaldırıp AutoMapper kullanılmalı.

---

## 2. Manuel Mapping Kullanımları

### 2.1. PaymentService - GetPaymentsPaged
**Dosya:** `GuestFlow.Application/Operations/Payment/PaymentService.cs` (satır 289-306)  
**Sorun:** `GetPaymentsPaged` metodu manuel mapping kullanıyor, ancak `GetPaymentById` ve `GetPaymentDetail` AutoMapper kullanıyor.

**Mevcut Kod:**
```csharp
var dtos = payments.Select(p => new GetPaymentDto
{
    Id = p.Id,
    PaymentNumber = p.PaymentNumber,
    // ... manuel mapping
}).ToList();
```

**Çözüm:** AutoMapper kullanılmalı:
```csharp
var dtos = _mapper.Map<List<GetPaymentDto>>(payments);
```

**Not:** `GetPaymentDto` içinde `GuestName` ve `InvoiceNumber` gibi navigation property'ler varsa, bunlar için özel mapping gerekebilir.

---

### 2.2. SmsService - GetSmsHistoryPaged
**Dosya:** `GuestFlow.Application/Operations/Sms/SmsService.cs` (satır 470-490)  
**Sorun:** `GetSmsHistoryPaged` metodu manuel mapping kullanıyor, ancak `GetSmsHistoryById` AutoMapper kullanıyor.

**Mevcut Kod:**
```csharp
var dtos = smsList.Select(s => new GetSmsHistoryDto
{
    Id = s.Id,
    // ... manuel mapping
}).ToList();
```

**Çözüm:** AutoMapper kullanılmalı, ancak `GuestName` ve `PersonnelName` gibi navigation property'ler için özel mapping gerekebilir.

---

### 2.3. SmsService - Diğer Metodlar
**Dosya:** `GuestFlow.Application/Operations/Sms/SmsService.cs`  
**Etkilenen Metodlar:**
- `GetSmsHistoryByPhoneNumber` (satır 523)
- `GetSmsHistoryByGuestId` (satır 566)

**Sorun:** Bu metodlar da manuel mapping kullanıyor.

---

## 3. Tutarsızlıklar

### 3.1. GetById vs GetList Metodları
**Sorun:** Birçok Manager'da `GetById` metodları manuel mapping kullanırken, `GetList` ve `GetPaged` metodları AutoMapper kullanıyor.

**Etkilenen Manager'lar:**
- `AirportManager` - GetById manuel, GetList AutoMapper
- `CityManager` - GetById manuel, GetList AutoMapper
- `VehicleManager` - GetById manuel, GetList AutoMapper
- `CityTourManager` - GetById manuel, GetList AutoMapper
- `YachtTourManager` - GetById manuel, GetList AutoMapper
- `GuestManager` - GetById manuel, GetList manuel (her ikisi de manuel)
- `InvoiceManager` - Tüm metodlar manuel
- `DailyRevenueManager` - Tüm metodlar manuel
- `DailyNoteManager` - Tüm metodlar manuel

**Öneri:** Tüm Manager'larda tutarlı bir yaklaşım kullanılmalı. AutoMapper tercih edilmeli.

---

### 3.2. PaymentService - GetPayments Metodları
**Dosya:** `GuestFlow.Application/Operations/Payment/PaymentService.cs`  
**Sorun:** 
- `GetPaymentById` ve `GetPaymentDetail` AutoMapper kullanıyor (satır 221, 246)
- `GetPaymentsPaged` manuel mapping kullanıyor (satır 289)
- `GetPaymentsByInvoiceId`, `GetPaymentsByGuestId`, `GetPaymentsByStatus`, `GetPaymentsByDateRange` manuel mapping kullanıyor (satır 478, 514, 554)

**Öneri:** Tüm metodlarda AutoMapper kullanılmalı.

---

## 4. Eksik Update DTO Mapping'leri

### 4.1. UpdateCityTourDto → CityTourEntity
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosya:** `GuestFlow.Application/Operations/CityTour/CityTourManager.cs` (satır 305)

**Çözüm:**
```csharp
CreateMap<UpdateCityTourDto, CityTourEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.OwnerGuest, opt => opt.Ignore())
    .ForMember(dest => dest.Personnel, opt => opt.Ignore())
    .ForMember(dest => dest.City, opt => opt.Ignore())
    .ForMember(dest => dest.GuestCityTours, opt => opt.Ignore())
    .ForMember(dest => dest.Invoices, opt => opt.Ignore());
```

---

### 4.2. UpdateYachtTourDto → YachtTourEntity
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosya:** `GuestFlow.Application/Operations/YachtTour/YachtTourManager.cs` (satır 264)

**Çözüm:**
```csharp
CreateMap<UpdateYachtTourDto, YachtTourEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.OwnerGuest, opt => opt.Ignore())
    .ForMember(dest => dest.Personnel, opt => opt.Ignore())
    .ForMember(dest => dest.City, opt => opt.Ignore())
    .ForMember(dest => dest.GuestYachtTours, opt => opt.Ignore())
    .ForMember(dest => dest.Invoices, opt => opt.Ignore());
```

---

### 4.3. UpdateTransferDto → TransferEntity
**Durum:** ❌ Eksik  
**Dosya:** `GuestFlow.Application/Mappings/MappingProfile.cs`  
**Etkilenen Dosya:** `GuestFlow.Application/Operations/Transfer/TransferManager.cs` (satır 285)

**Çözüm:**
```csharp
CreateMap<UpdateTransferDto, TransferEntity>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.Guest, opt => opt.Ignore())
    .ForMember(dest => dest.Personnel, opt => opt.Ignore())
    .ForMember(dest => dest.Airport, opt => opt.Ignore())
    .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
    .ForMember(dest => dest.Invoices, opt => opt.Ignore())
    .ForMember(dest => dest.PickupCity, opt => opt.Ignore())
    .ForMember(dest => dest.DropoffCity, opt => opt.Ignore());
```

---

### 4.4. UpdateAirportDto → AirportEntity
**Durum:** ❌ Eksik (Yukarıda belirtildi, 1.1'de)

---

### 4.5. UpdateCityDto → CityEntity
**Durum:** ❌ Eksik (Yukarıda belirtildi, 1.2'de)

---

## 5. Öncelik Sırası ve Öneriler

### 🔴 Yüksek Öncelik (Kritik)

1. **VehicleEntity → GetVehicleDto** - ✅ Düzeltildi (MappingProfile'a eklendi)
   - ⚠️ `GetVehicleById` metodundaki manuel mapping kaldırılmalı

2. **AirportEntity → GetAirportDto** - Eksik mapping eklenmeli
3. **CityEntity → GetCityDto** - Eksik mapping eklenmeli
4. **InvoicesEntity → GetInvoiceDto** - Eksik mapping eklenmeli

### 🟡 Orta Öncelik

5. **DailyRevenueEntity → GetDailyRevenueDto** - Eksik mapping eklenmeli
6. **DailyNoteEntity → GetDailyNoteDto** - Eksik mapping eklenmeli
7. **UpdateCityTourDto, UpdateYachtTourDto, UpdateTransferDto** - Eksik mapping'ler eklenmeli
8. **UpdateAirportDto, UpdateCityDto** - Eksik mapping'ler eklenmeli

### 🟢 Düşük Öncelik (İyileştirme)

9. **PaymentService** - Manuel mapping'ler AutoMapper'a çevrilmeli
10. **SmsService** - Manuel mapping'ler AutoMapper'a çevrilmeli
11. **GuestManager** - Manuel mapping'ler AutoMapper'a çevrilmeli (mapping zaten var)
12. **CityTourManager, YachtTourManager** - GetById metodlarındaki manuel mapping'ler kaldırılmalı

---

## 📝 Genel Öneriler

1. **Tutarlılık:** Tüm Manager'larda aynı mapping yaklaşımı kullanılmalı (AutoMapper tercih edilmeli).

2. **Navigation Properties:** Eğer DTO'larda navigation property'lerden gelen veriler varsa (örn: `GuestName`, `InvoiceNumber`), bunlar için özel mapping tanımlanmalı:
   ```csharp
   .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : null))
   ```

3. **Test:** Her mapping değişikliğinden sonra ilgili endpoint'ler test edilmeli.

4. **Dokümantasyon:** MappingProfile.cs dosyasına yorum satırları eklenerek hangi mapping'lerin ne için kullanıldığı belirtilmeli.

---

## 🔍 Kontrol Listesi

- [ ] AirportEntity → GetAirportDto mapping eklendi
- [ ] CityEntity → GetCityDto mapping eklendi
- [ ] InvoicesEntity → GetInvoiceDto mapping eklendi
- [ ] DailyRevenueEntity → GetDailyRevenueDto mapping eklendi
- [ ] DailyNoteEntity → GetDailyNoteDto mapping eklendi
- [ ] UpdateCityTourDto → CityTourEntity mapping eklendi
- [ ] UpdateYachtTourDto → YachtTourEntity mapping eklendi
- [ ] UpdateTransferDto → TransferEntity mapping eklendi
- [ ] UpdateAirportDto → AirportEntity mapping eklendi
- [ ] UpdateCityDto → CityEntity mapping eklendi
- [ ] VehicleManager.GetVehicleById manuel mapping kaldırıldı
- [ ] CityTourManager.GetCityTourById manuel mapping kaldırıldı
- [ ] YachtTourManager.GetYachtTourById manuel mapping kaldırıldı
- [ ] AirportManager.GetAirportById manuel mapping kaldırıldı
- [ ] CityManager.GetCityById manuel mapping kaldırıldı
- [ ] GuestManager manuel mapping'ler AutoMapper'a çevrildi
- [ ] InvoiceManager manuel mapping'ler AutoMapper'a çevrildi
- [ ] DailyRevenueManager manuel mapping'ler AutoMapper'a çevrildi
- [ ] DailyNoteManager manuel mapping'ler AutoMapper'a çevrildi
- [ ] PaymentService manuel mapping'ler AutoMapper'a çevrildi
- [ ] SmsService manuel mapping'ler AutoMapper'a çevrildi

---

**Not:** Bu rapor, mevcut kod tabanının analizi sonucunda oluşturulmuştur. Yeni mapping'ler eklendikçe veya mevcut mapping'ler değiştikçe bu rapor güncellenmelidir.

