# Phase 1: CRITICAL FIXES - GuestFlow İyileştirme Programı

## 📅 Başlangıç Tarihi: 2025-01-13
## 🎯 Hedef: Test Coverage %70+, Production Ready Code

---

## 🔥 KRİTİK SORUNLAR (HAFTA 1-2)

### 1. **TEST COVERAGE KRİZİ** - #1 ÖNCELİK
**Mevcut Durum:** İlk adımlar atıldı - Coverage çalışıyor!

#### ✅ Tamamlanan (Week 1 - 13-17 Ocak)
- [x] Jest konfigürasyonu kontrol edildi (%70 threshold ayarlı)
- [x] Test dosyaları mevcut (20+ test dosyası)
- [x] Coverage raporu üretiliyor ✅
- [x] i18n mock sorunu çözüldü ✅
- [x] collectCoverageFrom ayarları optimize edildi ✅
- [x] **formatters.test.ts**: 7/7 tests passing ✅
- [x] **validation.test.ts**: 6/6 tests passing ✅
- [x] **Coverage hesaplama çalışıyor** ✅

#### 🔍 Sorun Analizi (Güncellendi - 17 Ocak)
```bash
# Başarı hikayesi:
✅ formatters.ts: Statements 94.11%, Lines 92.85%
✅ validation.ts: Tüm testler geçiyor
✅ 13+ test çalışıyor

# Kalan sorunlar:
❌ transferService.test.ts: API mock sorunları
❌ useNotification.test.ts: Hook mock sorunları
❌ Component tests: Store/React Query mock'ları
❌ E2E tests: Environment sorunları
❌ Memory leak: Büyük test suitlerinde heap overflow

# Genel coverage hala düşük çünkü çoğu test çalışmıyor
```

#### 🛠️ Çözüm Adımları (Güncellendi - 17 Ocak)
- [x] **Jest konfigürasyonu düzeltme** ✅
  - [x] `collectCoverageFrom` ayarları optimize edildi
  - [x] `testMatch` pattern'leri doğrulandı
  - [x] Transform ayarları kontrol edildi

- [x] **Temel mock'lar oluşturuldu** ✅
  - [x] i18n mock'u çalışıyor
  - [x] Axios mock altyapısı hazır
  - [x] React Router mock'u çalışıyor

- [ ] **Test dosyalarını onarma**
  - [ ] API service testlerini düzeltme (transferService.test.ts)
  - [ ] Hook testlerini düzeltme (useNotification.test.ts)
  - [ ] Component testlerini düzeltme (store/React Query mock'ları)
  - [ ] Integration testlerini düzeltme

- [ ] **Performance optimizasyonu**
  - [ ] Memory leak sorununu çözme
  - [ ] Test parallelization
  - [ ] Coverage calculation optimizasyonu

### 2. **TODO LİSTELERİNİ GÜNCELLEME**

#### Backend TODO Güncellemeleri
- [x] **AutoMapper Mapping** - ASLINDA ÇOK İYİ (521 satır kod)
- [ ] Entity Framework warnings giderilmesi
- [ ] Security headers iyileştirilmesi
- [ ] Input sanitization implementasyonu

#### Frontend TODO Güncellemeleri
- [x] **Çoğu özellik tamamlanmış** - TODO'ları işaretle
- [ ] Test coverage sorunu çözümü
- [ ] Performance monitoring ekleme

### 3. **ENTITY FRAMEWORK WARNINGS**

#### Mevcut Uyarılar:
```
- The property 'GuestYachtTour.CreatedDate' was first mapped explicitly and then ignored
- Multiple relationships without configured foreign key properties
- Decimal properties without explicit SQL column type
- Query filter interaction warnings
```

#### Çözüm:
- [ ] Fluent API yapılandırmasını iyileştirme
- [ ] Model builder'da ilişki yapılandırması
- [ ] Decimal property'ler için column type belirleme

---

## 📋 HAFTA 1 PLANI (13-19 Ocak)

### Gün 1-2: Test Coverage Sorunu
- [ ] Jest konfigürasyonunu derinlemesine analiz
- [ ] Test dosyalarını tek tek çalıştırma
- [ ] Coverage hesaplama mantığını anlama
- [ ] İlk düzeltmeleri uygulama

### Gün 3-4: TODO Güncellemeleri
- [ ] BACKEND_TODO.md dosyasını güncelleme
- [ ] FRONTEND_TODO.md dosyasını güncelleme
- [ ] PHASE1_CRITICAL_FIXES.md oluşturma

### Gün 5-7: EF Warnings & Güvenlik
- [ ] Entity Framework uyarılarını giderme
- [ ] Security headers ekleme
- [ ] Input sanitization implementasyonu

---

## 📊 İLERLEME TAKİBİ

### Hafta 1 İlerleme (13-17 Ocak) ✅
- [x] Test Coverage altyapısı kuruldu (%0.1 → coverage hesaplanıyor)
- [x] İlk testler düzeltildi (formatters, validation)
- [x] Mock altyapısı geliştirildi (i18n, axios, react-router)
- [x] Coverage raporları çalışır hale getirildi
- [ ] TODO Güncellemeleri: 0/2 (**Bu hafta yapılacak**)
- [ ] EF Warnings: 0/4 (**Bu hafta yapılacak**)

### Hafta 2 İlerleme (20-24 Ocak - Devam Eden)
- [x] Security headers eklendi (CSP, HSTS, X-Frame-Options)
- [x] Entity Framework warnings giderildi
- [ ] Input validation derinlemesine kontrol
- [ ] Kalan test dosyaları düzeltme (Jest konfigürasyon sorunları devam ediyor)
- [ ] Coverage %70+ hedefi (şu anda %2.22, temel altyapı hazır)

**Güncel Durum:** Jest konfigürasyonunda babel/tsx parsing sorunları var. Çözüm için ts-jest'e geçiş veya babel konfigürasyonu gerekiyor.

---

## 🎯 BAŞARI KRİTERLERİ

### Minimum Gereksinimler (Week 2 Sonu)
- [ ] Frontend Test Coverage: %70+
- [ ] Backend TODO: Güncel ve doğru
- [ ] EF Warnings: %100 giderildi
- [ ] Security Headers: CSP, HSTS, etc.
- [ ] Input Sanitization: XSS koruması

### İdeal Hedefler (Week 2 Sonu)
- [ ] Backend Test Coverage: %60+
- [ ] Performance monitoring aktif
- [ ] Error tracking kurulmuş
- [ ] Production deployment ready

---

## 🚨 RİSKLER & ENGELLER

### Yüksek Risk
- **Test Coverage**: Jest/TypeScript konfigürasyonu karmaşık
- **Coverage Hesaplama**: Source map ve transform sorunları

### Orta Risk
- **EF Warnings**: Model değişiklikleri gerektirebilir
- **Security Headers**: CORS çakışması riski

### Düşük Risk
- **TODO Güncellemeleri**: Sadece dokümantasyon
- **Input Sanitization**: Mevcut validasyonları genişletme

---

## 📞 DESTEK & KAYNAKLAR

### Dokümantasyon
- Jest Configuration: https://jestjs.io/docs/configuration
- Istanbul Coverage: https://github.com/istanbuljs/nyc
- Entity Framework: https://docs.microsoft.com/en-us/ef/core/

### Test Dosyaları
- `__tests__/` klasöründe 20+ test dosyası mevcut
- `setupTests.ts` konfigüre edilmiş
- `jest.config.js` %70 threshold ayarlı

---

## 🎉 MİLENSTONE'LAR

### Milestone 1: Test Coverage Düzeltildi
- Frontend coverage %70+
- Tüm testler geçiyor
- Coverage raporu doğru çalışıyor

### Milestone 2: Code Quality İyileştirildi
- EF warnings giderildi
- Security headers eklendi
- Input sanitization aktif

### Milestone 3: Production Ready
- Tüm kritik sorunlar çözüldü
- Monitoring aktif
- Deployment hazır