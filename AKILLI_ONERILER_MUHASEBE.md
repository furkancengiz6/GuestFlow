#### 2.3 Akıllı Öneriler
- Transfer önerileri (mevcut aktivitelere göre)
- Paket servis önerileri
- Çapraz satış fırsatları
- **Öncelik**: ORTA

--- 

# Değerlendirme — Uygulamanın sunduğu çözümlerde yanlışlık var mı?

Kısa cevap: Temel iş akışlarında **kritik iş mantığı hatası** görünmüyor, ancak muhasebe ve finans akışlarında eksiklikler ve riskler mevcut. Aşağıda tespit ettiğim önemli noktalar:

- Güvenlik/iş akışı hataları (ör. XSS sanitization için regex kullanımı, interceptor DI anti-pattern) teknik risk oluşturuyor; muhasebe verisiyle doğrudan ilişkili olmasa da veri bütünlüğünü riske atar.
- Muhasebe tarafı için kritik bir eksik: **fatura → muhasebe defteri (journal)** dönüşümünün, GL kod eşlemelerinin ve tekil/çift kayıt (double-entry) mantığının uygulamada açık ve otomatik şekilde tanımlanmış olmaması. Eğer faturalar sadece "PDF üretimi" ve "ödeme kaydı" olarak tutuluyorsa, finans departmanı için manuel çalışma gerekecektir.
- Çoklu döviz, kur farkı muhasebesi ve yuvarlama kuralları konusunda net politika ve otomasyon yoksa hatalı kayıtlar ve raporlarda tutarsızlıklar çıkar.
- Tedarikçi maliyetlerinin servise otomatik bağlanması doğru yönde; fakat "tahsilat eşleme, kısmi ödemeler, iade/geri ödeme" gibi durumlar tam kapsamlı değilse muhasebe reconciliations zorlaşır.

Genel değerlendirme: uygulama işlevsellik olarak güçlü; ancak muhasebe iş akışı için "tekil kaynak" (single source of truth) ve "otomatik journal posting" gibi eksiklikler var. Bunlar çözülmezse muhasebe çalışanı çok manuel iş yapmak zorunda kalır ve hatalar artar.

--- 

# Muhasebe (Finance) Kullanımı — Öneriler (Net, Kullanışlı ve Hızlı İş Akışı)

Aşağıdaki öneriler muhasebe personelinin işini daha net, hızlı ve doğru yapmasını sağlar. Öncelik sırasına göre kısa uygulanabilir adımlar öneriyorum.

1) Fatura → Otomatik Journal (Double-entry) Preview ve Post
 - Her fatura oluşturulduğunda (veya onaylandığında) sistemin otomatik olarak üreteceği muhasebe fişlerini (debit/credit satırları) "Önizle" modunda gösterin.
 - Muhasebeci "Preview" ekranında GL kodlarını görebilsin, gerekirse değiştirsin ve "Post to Ledger" butonuyla toplu olarak deftere aktarabilsin.
 - Toplu onay/ret ve tarihleme (posting date) seçeneği olsun.

2) GL Mapping (Esnek Hesap Eşlemesi)
 - Her servis türü (Transfer, Tour, Restaurant, Package) için varsayılan GL kod şablonları tanımlanabilsin (ör: revenue, VAT, supplier payable, commissions).
 - Kullanıcı bazlı/şube bazlı override mümkün olsun.

3) Muhasebe Dışı İşlemler İçin "Room Ledger" ve "Guest Ledger"
 - Oda bazlı tüm hareketler (transfer, yemek, extra) tek bir "Room Ledger" içinde görülebilsin; muhasebeci için export (CSV/Excel/SAF‑T) doğrudan bu görünümden alınabilsin.
 - Misafir bazlı bakiye, ödemeler, açık kalemler net şekilde gösterilsin.

4) Otomatik Eşleme (Reconciliation Assistant)
 - Banka mutabakatı ve tahsilat eşlemeleri için bir öneri motoru (match suggestions) ekleyin; eşleştirme olasılığı yüzdesi gösterilsin.
 - Kısmi ödemeleri, çoklu faturaya dağıtımı ve komisyon-ücret kesintilerini otomatik/yarı otomatik öneriyle yapabilsin.

5) Vergi (VAT) & Çoklu Döviz Desteği
 - Faturada vergi kırılımı zorunlu olsun; KDV matrahı, KDV tutarı, vergi kodu görünür ve export edilebilir olsun.
 - Döviz cinsleri için otomatik kur çekme (günlük rate) ve kur farkı muhasebesi (revaluation) raporları ekleyin.
 - Rounding (yuvarlama) kuralları (bankaya göre) tanımlanabilsin.

6) Export & Integration
 - Excel/CSV export ile birlikte **SAF‑T** veya ülke/regülatöre uygun muhasebe standardı (XML/UBL) formatları sunulsun.
 - QuickBooks / Xero / Logo / Nebim gibi yerel/uluslararası muhasebe paketlerine entegrasyon (otomatik journal push) planlayın.

7) Period Close / Posting Lock
 - Muhasebe personelinin belirlediği "fiscal period" kapatma (lock) mekanizması olsun; kapatılan dönemde değişiklik yapılamasın (sadece reversing entry ile).
 - Kapanış için checklist (VAT report, bank reconciliation, accruals) sunun.

8) Audit Trail & Attachments
 - Her posting için destekleyici belge eklenebilsin (PDF, invoice scans) ve audit log ile ilişkilendirilsin.
 - "Who posted / when / approval history" görünümü bulunsun.

9) Approval Workflow
 - Büyük tutarlı faturalar veya tedarikçi ödemeleri için çok-adımlı onay (2 veya 3 seviye) ekleyin.
 - Onay geçmişi ve reddetme sebebi log'lansın.

10) Accountant-friendly UI
 - "Accounting Dashboard" (Kısa KPI'lar): Open invoices, overdue receivables, upcoming payable dates, bank balance reconciliation status.
 - Hızlı filtreler (period, branch, GL code, supplier) ve hızlı export düğmeleri.
 - "Preview Journal" modalı, "Export to Excel/SAF-T", "Post Selected" işlemleri tek yerden.

--- 

# Önceliklendirme (Muhasebe için)
 - Immediate (1 hafta): Invoice → Journal preview & Post, simple CSV/Excel export, VAT breakdown on invoice.
 - Short (2–4 hafta): Reconciliation assistant (bank matching suggestions), GL mapping UI, multi-currency posting rules.
 - Medium (1–3 ay): Accounting package integrations (QuickBooks/Xero/Logo), period close workflow, approval flows.

--- 

# Kısa Notlar — Uygulamadaki Hatalar/Çelişkiler (Muhasebe Perspektifi)
 - Eğer şu anda faturalar sadece PDF/string olarak tutuluyor ancak journal entry otomatik oluşturulmuyorsa: muhasebe çalışanı için büyük bir eksiklik ve hata kaynağıdır (manuel giriş, kayıp kayıtlar).
 - Tedarikçi maliyetlerinin servis kayıtlarına otomatik bağlanması iyi; ancak tedarikçi ödemeleri ve banka eşlemeleri eksikse mali tablolar hatalı çıkar.
 - Çoklu döviz politikasının ve kur farkı muhasebesinin açıkça tanımlanmış olması zorunlu (aksi halde raporlar tutarsız olur).

--- 

Dosya: AKILLI_ONERILER_MUHASEBE.md

