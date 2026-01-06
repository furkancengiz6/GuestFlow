# GuestFlow - Gelecek Özellik Fikirleri

> Bu dosya, mevcut mimari ve iş akışlarını bozmadan projeye eklenebilecek orta / uzun vadeli geliştirme fikirlerini toplar.  
> Uygulanan fikirler CHANGELOG ve ilgili TODO dosyalarına taşınmalıdır.

---

## 1. Tedarikçi (Supplier) Modülü ve Kârlılık

- **1.1 Tedarikçi Yönetimi**
  - [ ] `Suppliers` tablosu: `Id, Name, Type (Yacht, Transfer, Activity, General), DefaultCurrency, DefaultCost, ContactName, Phone, Email, Notes, IsActive`.
  - [ ] Backend: Supplier CRUD (repository, service, controller).
  - [ ] Frontend: Tedarikçi liste/form sayfası + transfer/tur formlarında `SupplierId` dropdown.
  - [ ] Supplier bazlı raporlar: “Bu ay X tedarikçisine ne ödedik, ne kazandık?”.

- **1.2 Hizmet Bazlı Kâr / Marj Hesapları**
  - [ ] Transfer / CityTour / YachtTour için:
    - Müşteri fiyatı: `FinalPrice` (zaten var).
    - Tedarikçi maliyeti: `SupplierCost` + `SupplierCurrency`.
  - [ ] Raporlama servisi:
    - [ ] Tarih / şehir / tedarikçi / hizmet tipi filtrelerine göre: Toplam satış, toplam tedarikçi maliyeti, net kâr, marj %.
    - [ ] Döviz dönüşümü (mevcut `ICurrencyService` kullanılarak).
  - [ ] Dashboard kartları:
    - [ ] “Bugünkü Net Kâr”, “Bu Ay Net Kâr”, “Top 5 En Karlı Tedarikçi”.

---

## 2. Ödeme ve Tahsilat Takibi İyileştirmeleri

- **2.1 Ödemesi Alınmamış Hizmetler Paneli (Detaylı)**
  - [ ] Mevcut `Dashboard / unpaid-services` endpoint’ini genişlet:
    - Tüm hizmet tipi için: misafir adı, oda numarası, şehir, hizmet tarihi, tutar, tedarikçi maliyeti, net kâr.
    - Ödeme durumu (müşteri + tedarikçi ayrı).
  - [ ] Frontend:
    - [ ] Dashboard’da filtrelenebilir tablo (tarih aralığı, şehir, personel, tedarikçi).
    - [ ] Her satırdan ilgili hizmet detay sayfasına link.

- **2.2 Oda Bazlı Hesap Ekstresi**
  - [ ] Misafir için “Room Ledger” görünümü:
    - Oda numarası + check-in/out aralığı içinde tüm hizmetler (transfer, tur, rezervasyon, ekstra).
    - Toplam borç, ödenen, kalan.
  - [ ] Otel resepsiyonuna özel panel: “Bugün çıkış yapacak odaların hesap özeti”.

---

## 3. Otomatik Uyarılar ve Hatırlatmalar

- **3.1 Zaman Bazlı Hatırlatmalar**
  - [ ] Transfer için:
    - Hizmetten 3 saat önce operatör panelinde “Acil” renkle vurgulama (mevcut UpcomingServices endpoint’iyle uyumlu).
    - Opsiyonel SMS / WhatsApp hatırlatma (şoföre / misafire).
  - [ ] Şehir ve Yat Turları için:
    - 24 saat kala “Yaklaşan Tur” listesinde vurgulama.
    - Rehber / kaptan için bilgilendirme mesajı.

- **3.2 Otomatik Görevler (Background Jobs)**
  - [ ] Hangfire / Quartz ile:
    - Günlük “Ödemesi alınmayan hizmetler” raporu (mail / dashboard).
    - Günlük “Bugün yapılacak hizmetler” özeti.
    - Tedarikçi ödemeleri yaklaşan kayıtlar için uyarı.

---

## 4. Misafir ve Personel Deneyimi

- **4.1 Misafir Portalı (Light Sürüm)**
  - [ ] Sadece okuma amaçlı mini portal:
    - Misafirin kendi rezervasyonlarını, transferlerini, turlarını görmesi.
    - PDF faturalarını indirmesi.
  - [ ] İleride: online ödeme veya ek hizmet talebi (örn. ekstra transfer, city tour satın alma).

- **4.2 Personel için Mobil Operasyon Paneli**
  - [ ] Mobil uyumlu sade sayfa:
    - “Bugünkü transferlerim”, “Bugünkü turlarım”, “Ödemesi alınmamışlar”.
  - [ ] Harita entegrasyonu (Google Maps):
    - Toplanma noktaları, iskeleler, oteller için hızlı yönlendirme linkleri.

---

## 5. Raporlama ve Analitik

- **5.1 Zaman Serisi ve Segmentasyon Raporları**
  - [ ] Gelir ve kâr raporları:
    - Günlük / haftalık / aylık gelir ve net kâr.
    - Hizmet türüne göre dağılım (Transfer, CityTour, YachtTour, Restaurant, Package).
    - Pazaryeri / kaynak bazlı segmentasyon (Booking, Direkt, Acenta vs. – ileride eklenebilir).

- **5.2 Operasyonel KPI’lar**
  - [ ] KPI örnekleri:
    - Zamanında başlayan transfer yüzdesi.
    - Son dakika iptal oranı.
    - Tedarikçi bazlı iptal / sorun sayısı.
    - Kişi başı ortalama gelir.

---

## 6. Entegrasyonlar

- **6.1 Kanal / PMS Entegrasyonları (Gelecek Aşama)**
  - [ ] PMS (otel sistemleri) ile temel entegrasyon:
    - Konaklama rezervasyonlarını otomatik çekip GuestFlow içindeki hizmetlerle eşleştirme.
  - [ ] Kanal yöneticileri / OTA (Booking.com, Expedia vb.) için:
    - Kaynak alanı (`BookingSource`) ekleyip raporlarda kullanmak.

- **6.2 İletişim Entegrasyonları**
  - [ ] WhatsApp Business API veya benzeri:
    - Rezervasyon onayı, hatırlatma, fatura linki gönderimi.
  - [ ] Mevcut Email & SMS servisleri ile “şablon bazlı” misafir iletişimi akışları.

---

## 7. Teknik ve Altyapı İyileştirmeleri (Fikir Seviyesinde)

- **7.1 Gelişmiş Observability**
  - [ ] OpenTelemetry ile trace + metric’lerin toplanması.
  - [ ] Prometheus + Grafana dashboard’ları.

- **7.2 Çoklu Otel / Çoklu Marka (Multi-tenancy)**
  - [ ] Aynı sistemde birden fazla otel / acenta yönetimi için tenant altyapısı.
  - [ ] Tenant bazlı veri izolasyonu ve raporlama.

---

## 8. Bu Dosya Nasıl Kullanılmalı?

- Buradaki maddeler **resmî roadmap değil**, üretim kodunu etkilemeyen fikir havuzu niteliğindedir.
- Bir fikir geliştirme aşamasına taşındığında:
  - İlgili madde `BACKEND_TODO.md` veya `FRONTEND_TODO.md` içine somut task’ler olarak eklenmeli.
  - Uygulandığında CHANGELOG ve ilgili dokümantasyon güncellenmelidir.

---

## 9. Dinamik Fiyatlama ve Kampanya Motoru

- **9.1 Dinamik Fiyatlama (Yield Management)**
  - [ ] Doluluk, tarih (yüksek/ölü sezon), saat (gece/gündüz) gibi parametrelere göre otomatik fiyat önerileri.
  - [ ] “Kurallı fiyatlama” desteği:
    - Örnek: “Eğer tarih = hafta sonu VE kişi sayısı > 4 ise fiyatı %10 artır”.
    - Örnek: “Kapanışa 24 saatten az kala kalan kontenjan için %15 indirim”.
  - [ ] Turlar ve transferler için minimum/maksimum marj koruma (zararına satış engeli).

- **9.2 Kampanya / Kupon Sistemi**
  - [ ] Kupon ve kampanya tanımları:
    - Yüzde indirim, sabit indirim, belirli hizmet tipi / şehir / tarih aralığına özel kampanya.
  - [ ] Misafir bazlı kampanya:
    - Sık gelen misafire otomatik indirim.
    - Özel günlerde (doğum günü vb.) otomatik avantaj.
  - [ ] Raporlama:
    - Kampanyanın getirdiği ek gelir / kayıp marj analizi.

---

## 10. İş Akışı (Workflow) ve Otomasyon Kuralları

- **10.1 Workflow Tanımları**
  - [ ] Basit bir “iş akışı motoru” fikri:
    - Olay → Koşul → Aksiyon zinciri (ör. “Yeni transfer eklendiğinde ve tarih bugün ise → ilgili personele bildirim gönder”).
  - [ ] Hazır şablonlar:
    - “Yeni rezervasyon → E-posta + SMS onayı”.
    - “Ödemesi alınmamış 24 saati geçmiş hizmet → operatöre uyarı”.
    - “Misafir check-out’tan 1 gün sonra → memnuniyet e-postası”.

- **10.2 Görsel Workflow Editörü (uzun vadede)**
  - [ ] Backend tarafında JSON tabanlı workflow tanımları.
  - [ ] Frontend’te drag&drop ile iş akışı tasarımı (Node-benzeri).

---

## 11. Misafir Deneyimi ve CRM Özellikleri

- **11.1 Misafir Profili Zenginleştirme**
  - [ ] Misafir kartında:
    - Harcama geçmişi (toplam, hizmet türüne göre).
    - Tercihler (favori restoran, sevdiği oda tipi, sevdiği turlar).
    - Notlar (alerji, özel istekler, önemli kişiler).
  - [ ] “VIP misafir” segmentleri:
    - Kriterlere göre otomatik VIP etiketleme (toplam harcama, ziyaret sayısı vs.).
  
- **11.2 Sonrası İletişim ve Memnuniyet Ölçümü**
  - [ ] Tur/transfer sonrası kısa memnuniyet anketi (link veya QR).
  - [ ] NPS / skor takibi:
    - Misafir bazlı memnuniyet skoru.
    - Tedarikçi / kaptan / rehber bazlı skor.

---

## 12. Personel ve Operasyon Yönetimi

- **12.1 Personel Performans ve Görev Yönetimi**
  - [ ] Personel için görev listesi:
    - Günlük yapılacaklar (misafir karşılama, transfer sorumluluğu, tur rehberliği).
  - [ ] Performans göstergeleri:
    - Yapılan hizmet sayısı, memnuniyet skoru, gecikme sayısı vs.
  - [ ] Vardiya planlama:
    - Hangi personel hangi saat/dönem hangi hizmetten sorumlu?

- **12.2 Eğitim ve Check-list’ler**
  - [ ] Hizmet öncesi/sonrası yapılacaklar için check-list:
    - Örn: Transfer öncesi: “Araç temizliği”, “Karşılama tabelası hazır mı?”.
  - [ ] Uygulama içinde “read-only” onboarding / eğitim modülü:
    - Temel prosedürler, video/link desteği.

---

## 13. Gelişmiş Dashboard ve Görselleştirme

- **13.1 Operasyonel Harita Görünümü**
  - [ ] Canlı harita:
    - Bugünkü transfer ve tur lokasyonlarını (pickup/dropoff şehir/otel/iskelesi) görsel olarak gösterme.
    - Renklerle: yaklaşan, devam eden, tamamlanan hizmetler.
  
- **13.2 Çoklu Dashboard Setleri**
  - [ ] Rol bazlı dashboard:
    - Yönetici: kârlılık, toplam gelir, tedarikçi bazlı rapor.
    - Operasyon: yaklaşan hizmetler, ödenmemişler, iptal riskleri.
    - Finans: tahsilat, tedarikçi ödemeleri, alacak/borç listeleri.

---

## 14. Kalite ve Güvenlik Geliştirmeleri (Öneri Bazlı)

- **14.1 Güvenlik Derinleştirme**
  - [ ] Detaylı audit log (kim, ne zaman, hangi alanı neye çevirdi).
  - [ ] IP / cihaz bazlı oturum izi.
  - [ ] Basit bir “risk skoru” (çok kısa zaman aralığında aşırı istek, şüpheli aktiviteler).

- **14.2 Gelişmiş Test ve Simülasyon Ortamı**
  - [ ] “Demo data scenario” setleri:
    - Yüksek sezon senaryosu, düşük sezon senaryosu, sadece tedarikçi ağırlıklı çalışma senaryosu.
  - [ ] Tek komutla ortamı bu senaryolardan biriyle dolduran script.

---

## 15. Yapay Zekâ Destekli Öneriler (Uzun Vadede)

- **15.1 Fiyat / Paket Önerileri**
  - [ ] Geçmiş verilere bakarak:
    - Belirli dönemlerde hangi tur/transfer kombinasyonlarının daha çok satıldığını analiz etmek.
    - “Bu misafir için şu paketler uygun olabilir” şeklinde öneri motoru.

- **15.2 Anomali ve Hata Tespiti**
  - [ ] Garip kayıtları işaretlemek:
    - Çok düşük / çok yüksek fiyatlar.
    - Aynı anda iki yerde görünen hizmetler (çakışan rezervasyon).
    - Normalin çok dışında maliyet veya gelir.

---

## 16. Dokümantasyon ve Kullanılabilirlik

- **16.1 Operasyonel El Kitabı (In-App Docs)**
  - [ ] Uygulama içine gömülü kısa bilgi kutucukları:
    - “Bu alan ne için kullanılır?”, “Hangi durumlarda doldurmalıyım?” açıklamaları.
  - [ ] Özellikle yeni eklenen alanlar (tedarikçi, kârlılık, oda ekstresi gibi) için mini rehber.

- **16.2 API Entegrasyon Rehberleri**
  - [ ] 3. parti sistemlerle entegre olmak isteyenler için:
    - Örnek istek/yanıt gövdeleri.
    - Senaryo bazlı kullanım (ör. “sadece misafir verilerini çekmek için şu endpointleri kullan”).

