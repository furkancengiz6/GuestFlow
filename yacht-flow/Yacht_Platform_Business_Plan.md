# Türkiye Lüks Yat Turu ve Kiralama Platformu - Başlangıç Planı

Bu belge, Türkiye'ye (özellikle Bodrum, Göcek, Marmaris, Fethiye, Antalya) yurtdışından gelen turistler ile yat/tekne sahiplerini buluşturacak premium platformun A'dan Z'ye yol haritasıdır.

## 1. Vizyon ve Strateji (Temel Konumlandırma)
* **Değer Önerisi:** Uluslararası misafirler için güvenilir, şeffaf fiyatlandırmalı, ultra-lüks ve kolay kullanılabilir bir yat kiralama "pazar yeri" (Airbnb tarzı).
* **Hedef Kitle:** Türkiye'ye seyahat eden, premium deneyim arayan ve döviz bazlı harcama yapan yabancı turistler (İngiltere, Rusya, Almanya, Orta Doğu ülkeleri vb.).
* **İş Modeli:** Gerçekleşen rezervasyonlar üzerinden komisyon (Guest service fee + Host service fee) veya yat sahiplerinden alınan sezonluk listeleme ücreti.

---

## 2. Teknoloji Yığını (Tech Stack) Seçimi
Platformu önce Web, sonra iOS ve Android olarak ölçeklendireceğimiz için baştan "Monorepo" (çoklu proje) mantığıyla ilerlemeliyiz. Bu sayede web ve mobil arasında kod paylaşımı yapabiliriz.

* **Frontend (Web):** Next.js (SEO için mükemmel, çoklu dil desteği kolay, inanılmaz hızlı).
* **Mobil (Gelecek Faz):** React Native (Expo) - Web'deki iş mantığının %70'ini mobilde de kullanabilmemizi sağlar.
* **Backend & API:** NestJS (Node.js) - Güvenli, ölçeklenebilir ve kurumsal mimari.
* **Veritabanı:** PostgreSQL (İlişkisel veriler, rezervasyonlar) & Redis (Hızlı arama ve önbellekleme).
* **Tasarım Dili:** Koyu tema ağırlıklı (Dark Mode), modern tipografi (Inter veya Outfit), altın/bronz/deniz mavisi detaylarla bezenmiş "Ultra-Premium" lüks hissiyatı.

---

## 3. Faz 1: Web MVP (Minimum Viable Product) Geliştirme (0-3 Ay)
İlk amacımız çalışan, harika görünen ve ödeme alabilen bir web sitesi çıkarmaktır.

### Misafir (Guest) Arayüzü:
* **Gelişmiş Arama:** Lokasyon (Bodrum, Göcek vb.), Tarih, Kişi Sayısı, Yat Tipi (Gulet, Motoryat, Katamaran, Yelkenli) filtreleri.
* **Çoklu Dil ve Para Birimi:** İngilizce, Rusça, Arapça vb. diller ile USD, EUR, GBP gösterimi.
* **Yat Detay Sayfası:** Yüksek çözünürlüklü fotoğraflar/videolar, teknik özellikler, mürettebat bilgisi, fiyata dahil olan/olmayan hizmetler (Yemek, yakıt vb.).
* **Rezervasyon ve Ödeme:** Kredi kartı ile (Stripe veya Iyzico) güvenli depozito ödemesi. Akıllı takvim ile müsaitlik kontrolü.

### Ev Sahibi / Kaptan (Host) Paneli:
* Yat profili oluşturma ve fotoğraf yükleme.
* Dinamik fiyatlandırma (Yüksek sezon / düşük sezon fiyatları).
* Müsaitlik takvimi yönetimi (Kendi özel turlarını kapatabilme).
* Mesajlaşma (Misafirlerle doğrudan site üzerinden çeviri destekli iletişim).

---

## 4. Faz 2: Operasyon ve "Tavuk-Yumurta" Problemini Çözme (3-6 Ay)
Bir pazar yerinde en zor kısım hem yatları hem de müşteriyi aynı anda bulmaktır.
* **Tedarik (Supply) Toplama:** İlk olarak web sitesi bitmeden yat sahipleri, acenteler ve kaptanlarla görüşüp sisteme manuel olarak en az 50-100 adet yüksek kaliteli yat eklenmelidir. (Bodrum ve Göcek marinadan başlanabilir).
* **SEO ve İçerik Pazarlaması:** "Luxury yacht charter Bodrum", "Gulet holiday Turkey" gibi anahtar kelimelerde blog içerikleri ve bölge rehberleri oluşturularak Google'da organik trafik hedeflenir.
* **B2B Ortaklıklar:** Lüks otellerin (Mandarin Oriental, Macakizi vb.) concierge departmanlarıyla anlaşılarak müşterilerine sizin platformunuz üzerinden tekne ayarlanması.

---

## 5. Faz 3: Mobil Uygulama Geliştirme - iOS & Android (6-9 Ay)
Web tarafı gelir üretmeye ve sistemin hataları giderilmeye başlandığında mobil uygulamalar devreye girer.

* **Neden Mobil?** Özellikle tatil için gelmiş turistler sahildeyken bilgisayar açmaz, her şeyi cepten yapar.
* **Anlık Bildirimler (Push Notifications):** Kaptanlar için yeni rezervasyon talebi anında cebe düşer, misafirler için "Yatınız hazır, şu marinada sizi bekliyor" bildirimleri gider.
* **Harita ve Navigasyon Entegrasyonu:** (Apple Maps/Google Maps) Misafirlerin yatın kalkacağı marinaya yol tarifi alması veya yatın anlık konumunu haritada görmesi.
* **Gemi İçi (On-board) Concierge:** Misafir yattayken mobil uygulama üzerinden ek içki, şef, jetski veya transfer talebinde bulunabilir.

---

## 6. Faz 4: Ekosistem Büyütme ve Yeni Özellikler (9+ Ay)
* **AI Destekli Rota Planlayıcı:** Misafirlere "Yapay Zeka" ile 7 günlük özel Göcek-Marmaris rotası oluşturma.
* **Deneyimler:** Sadece yat değil; "Gün batımında yatta özel keman dinletisi", "Koyda dalış eğitimi" gibi yata entegre alt-deneyimlerin satışı.
* **Sadakat Programı:** Müşterileri her yıl geri getirmek için özel "Yacht Club" statüleri.

---

## Özetle İlk Adımlarınız Ne Olmalı?
1. Platformun ismini (markayı) netleştirin ve domainleri alın.
2. Hedef bölgedeki (örn: Bodrum) 5-10 yat sahibiyle konuşup böyle bir sisteme komisyon verip vermeyeceklerini doğrulayın.
3. Tasarım (UI/UX) aşamasına geçip premium bir marka kimliği oluşturalım.
4. Yazılım altyapısını kurmaya başlayalım (Web MVP).
