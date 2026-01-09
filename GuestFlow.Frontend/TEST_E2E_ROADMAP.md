# E2E Testing Roadmap — GuestFlow.Frontend

Bu doküman, `GuestFlow.Frontend` için uçtan uca (E2E) test stratejisi ve uygulanması için genel yol haritasını sunar. Amaç: güvenilir, tekrarlanabilir ve bakımı kolay E2E testleri oluşturmak ve CI'ye entegre etmektir.

- **Kapsam**
  - Playwright tabanlı E2E testleri (UI + temel akışlar)
  - Test veri yönetimi ve kimlik doğrulama akışları
  - CI çalıştırmaları, raporlama ve hata teşhisi

- **Hedefler**
  - Kritik kullanıcı akışlarını test etmek (login, dashboard, transfers, guests, raporlar)
  - Minimal flakiness (stabil beklemeler, test izolasyonu)
  - Hızlı yerel geri bildirim + güvenilir CI çalıştırmaları

- **Önkoşullar**
  - `npm install` çalıştırılmış olmalı (özellikle `@playwright/test` ve `@types/node`)
  - `playwright.config.ts` doğru `baseURL` ve `webServer` ile yapılandırılmış
  - `tsconfig` testleri kapsayacak şekilde (`types` ve `include`) güncel

- **Yapı / Konvansiyonlar**
  - Test dosya yolu: `tests/e2e/*.spec.ts` veya proje içindeki `tests/**`
  - Her spec dosyası için kısa açıklama, gerekli fixture'lar ve temiz başlangıç durumu
  - Test isimlendirme: `feature - should do X` şeklinde okunabilir test başlıkları
  - Ortak helper/fonksiyonlar: `tests/playwright/*` veya `tests/helpers/*`

- **Çevresel Değişkenler**
  - `E2E_BASE_URL` — testlerin çalışacağı uygulama URL'si (fallback: `http://localhost:5175`)
  - `E2E_USER_EMAIL`, `E2E_USER_PASSWORD` — test kullanıcı kimlik bilgileri
  - `PLAYWRIGHT_STORAGE` — varsa önceden oturum bilgisi için storageState yolu

- **Geliştirme Akışı (Yerel)**
  1. `npm run dev` ile uygulamayı başlat
  2. `npm run test:e2e` veya `npx playwright test --project=chromium`
  3. Hata durumunda: `npx playwright show-report` veya `playwright-report` çıktısını incele

- **CI Entegrasyonu**
  - Adımlar:
    1. Ortam kurulumu (`npm ci`)
    2. Uygulamayı servis olarak başlat (ör. `npm run build && npm run preview` veya docker)
    3. `npx playwright install --with-deps`
    4. `npx playwright test --reporter=list,html,json`
    5. Raporları sakla/artifakt olarak ekle (`playwright-report`, ekran görüntüleri, videolar)
  - Hedef: Her PR'da en az bir Playwright smoke testi; ana dalda tam test kümesi

- **Stabilite & İyileştirme Kuralları**
  - Beklemelerde explicit selector bazlı beklemeler kullan (örn. `toBeVisible`, `waitForSelector`)
  - Rastgele zaman gecikmelerinden kaçın; network bağımlılıklarını mock etme seçeneklerini değerlendir
  - Flaky test tespit edildiğinde: izolasyon, test verisi reset, ve testin yeniden yazılması

- **Bakım**
  - Bağımlılık güncellemeleri: `@playwright/test`, TypeScript ve ilgili `@types` paketleri için aylık kontrol
  - Test veri seti ve fixture'ları güncelle ve dokümante et
  - CI test sürelerini takip et; eğer artıyorsa testleri paralel/etiket bazlı ayır

- **Hata Teşhisi Kılavuzu**
  - Hızlı kontrol listesi:
    - Uygulama gerçekten başlıyor mu? (`curl $E2E_BASE_URL`)
    - Test kullanıcısı doğru mu? (credential env)
    - Playwright report/trace/video/log'larını incele
  - Özel durumlar: Auth token süresi, CORS, third-party API gecikmeleri — mock veya stub düşün

- **Acceptance Criteria**
  - Temel akışlar (login, dashboard, transfers list/create) flaky olmadan CI'de geçmeli
  - Her PR için en az bir hızlı smoke testi koşuyor olmalı
  - Test raporları ve artefaktlar CI içinde saklanmalı

- **İleriye Dönük / Roadmap**
  - Test kapsama alanını genişlet: ödeme/rezervasyon vb. kritik iş akışları
  - Test veri fixture yönetimi (seed/ teardown) otomasyonu
  - Performans/Load testleri veya Playwright ile API contract testleri

--- 
Not: İstersen bu dosyayı projenin köküne veya `GuestFlow.Frontend/docs/` altına taşıyabilirim ve ekip için checklist/PR şablonu oluşturabilirim.

