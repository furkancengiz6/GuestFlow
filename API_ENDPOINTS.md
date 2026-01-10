# GuestFlow API Endpoints Listesi

## GET Endpoints

### AuthController (`/api/v1.0/auth`)
- `GET /api/v1.0/auth/me` - Giriş yapmış kullanıcının bilgilerini getirir

### AirportsController (`/api/airports`)
- `GET /api/airports` - Tüm havalimanlarını getirir (sayfalanmış, sıralanmış)
- `GET /api/airports/{id}` - Havalimanını ID'ye göre getirir

### CitiesController (`/api/cities`)
- `GET /api/cities` - Tüm şehirleri getirir (sayfalanmış, sıralanmış)
- `GET /api/cities/{id}` - Şehri ID'ye göre getirir

### CityToursController (`/api/citytours`)
- `GET /api/citytours` - Tüm şehir turlarını getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/citytours/{id}` - Şehir turunu ID'ye göre getirir
- `GET /api/citytours/{id}/detail` - Şehir turu detayını getirir

### CurrencyController (`/api/currency`)
- `GET /api/currency/default` - Varsayılan para birimini getirir
- `GET /api/currency/supported` - Tüm desteklenen para birimlerini getirir
- `GET /api/currency/validate/{currencyCode}` - Para birimi kodunun geçerli olup olmadığını kontrol eder
- `GET /api/currency/symbol/{currencyCode}` - Para birimi sembolünü getirir

### DailyNotesController (`/api/dailynotes`)
- `GET /api/dailynotes` - Tüm günlük notları getirir
- `GET /api/dailynotes/{id}` - Günlük notu ID'ye göre getirir

### DailyRevenuesController (`/api/dailyrevenues`)
- `GET /api/dailyrevenues` - Tüm günlük gelirleri getirir
- `GET /api/dailyrevenues/{id}` - Günlük geliri ID'ye göre getirir

### DashboardController (`/api/dashboard`)
- `GET /api/dashboard/overview` - Dashboard genel bakış bilgilerini getirir
- `GET /api/dashboard/quick-stats` - Hızlı istatistikleri getirir
- `GET /api/dashboard/recent-activities` - Son aktiviteleri getirir
- `GET /api/dashboard/revenue-chart` - Gelir grafik verilerini getirir
- `GET /api/dashboard/upcoming-bookings` - Yaklaşan rezervasyonları getirir
- `GET /api/dashboard/guest-statistics` - Misafir istatistik kartı verilerini getirir

### EmailsController (`/api/emails`)
- `GET /api/emails/queue` - E-posta kuyruğunu getirir
- `GET /api/emails/templates` - E-posta şablonlarını getirir
- `GET /api/emails/templates/{id}` - E-posta şablonunu getirir
- `GET /api/emails/history` - E-posta geçmişini getirir
- `GET /api/emails/history/{id}` - E-posta geçmişini ID ile getirir
- `GET /api/emails/statistics` - E-posta istatistiklerini getirir

### ExportController (`/api/export`)
- `GET /api/export/guests/excel` - Misafir listesini Excel formatında dışa aktarır
- `GET /api/export/guests/csv` - Misafir listesini CSV formatında dışa aktarır
- `GET /api/export/invoices/excel` - Fatura listesini Excel formatında dışa aktarır
- `GET /api/export/invoices/csv` - Fatura listesini CSV formatında dışa aktarır
- `GET /api/export/revenue/excel` - Gelir raporunu Excel formatında dışa aktarır
- `GET /api/export/revenue/csv` - Gelir raporunu CSV formatında dışa aktarır
- `GET /api/export/transfers/excel` - Transfer listesini Excel formatında dışa aktarır
- `GET /api/export/transfers/csv` - Transfer listesini CSV formatında dışa aktarır

### FilesController (`/api/files`)
- `GET /api/files` - Dosya listesini getirir (sayfalama, filtreleme, sıralama ile)
- `GET /api/files/{fileName}` - Dosya bilgisini getirir
- `GET /api/files/download/{fileName}` - Dosyayı indirir
- `GET /api/files/categories` - Dosya kategorilerini getirir
- `GET /api/files/statistics` - Dosya istatistiklerini getirir
- `GET /api/files/invoices` - Fatura PDF'lerini getirir
- `GET /api/files/guests/{guestId}` - Misafir belgelerini getirir
- `GET /api/files/tours/{tourId}` - Tur görsellerini getirir
- `GET /api/files/{fileName}/metadata` - Dosya metadata'sını getirir
- `GET /api/files/{fileName}/preview` - Dosya önizlemesini getirir (görseller için)
- `GET /api/files/share` - Aktif paylaşım linklerini getirir
- `GET /api/files/share/{shareToken}` - Paylaşım linki ile dosyayı indirir

### GuestsController (`/api/guests`)
- `GET /api/guests` - Tüm misafirleri getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/guests/{id}` - Misafiri ID'ye göre getirir
- `GET /api/guests/{id}/detail` - Misafir detayını getirir
- `GET /api/guests/{id}/invoices` - Misafir faturalarını getirir
- `GET /api/guests/{id}/timeline` - Misafir zaman çizelgesini getirir

### ImportController (`/api/import`)
- (Import controller'da GET endpoint yok)

### InvoicesController (`/api/invoices`)
- `GET /api/invoices` - Tüm faturaları getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/invoices/{id}` - Faturayı ID'ye göre getirir
- `GET /api/invoices/{id}/detail` - Fatura detayını getirir
- `GET /api/invoices/statistics` - Fatura istatistiklerini getirir
- `GET /api/invoices/by-guest/{guestId}` - Misafire ait faturaları getirir

### LocalizationController (`/api/localization`)
- `GET /api/localization/languages` - Desteklenen dilleri getirir
- `GET /api/localization/current-culture` - Mevcut kültürü getirir

### NotificationsController (`/api/notifications`)
- `GET /api/notifications` - Bildirim listesini getirir (sayfalama ile)
- `GET /api/notifications/my` - Kullanıcının bildirimlerini getirir
- `GET /api/notifications/{id}` - Bildirim detayını getirir
- `GET /api/notifications/templates` - Bildirim şablonlarını getirir
- `GET /api/notifications/templates/{templateName}` - Bildirim şablonunu getirir
- `GET /api/notifications/statistics` - Bildirim istatistiklerini getirir
- `GET /api/notifications/history` - Bildirim geçmişini getirir

### PaymentsController (`/api/payments`)
- `GET /api/payments` - Tüm ödemeleri getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/payments/{id}` - Ödemeyi ID'ye göre getirir
- `GET /api/payments/{id}/detail` - Ödeme detayını getirir
- `GET /api/payments/by-guest/{guestId}` - Misafire ait ödemeleri getirir
- `GET /api/payments/by-invoice/{invoiceId}` - Faturaya ait ödemeleri getirir
- `GET /api/payments/by-status/{status}` - Duruma göre ödemeleri getirir

### PersonnelController (`/api/personnel`)
- `GET /api/personnel` - Tüm personelleri listeler (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/personnel/{id}` - Personelin bilgilerini getirir
- `GET /api/personnel/{id}/detail` - Personel detayını getirir
- `GET /api/personnel/{id}/activities` - Personel aktivite günlüklerini getirir

### ReportsController (`/api/reports`)
- `GET /api/reports/revenue-summary` - Gelir özeti (tarih aralığına göre)
- `GET /api/reports/guest-statistics` - Misafir istatistikleri
- `GET /api/reports/tour-statistics` - Tur istatistikleri
- `GET /api/reports/transfer-statistics` - Transfer istatistikleri
- `GET /api/reports/monthly-revenue` - Aylık gelir dağılımı
- `GET /api/reports/popular-destinations` - En popüler destinasyonlar
- `GET /api/reports/dashboard-summary` - Dashboard özeti
- `GET /api/reports/daily-revenue` - Günlük gelir raporu
- `GET /api/reports/weekly-revenue` - Haftalık gelir raporu
- `GET /api/reports/yearly-revenue` - Yıllık gelir raporu
- `GET /api/reports/popular-tours` - Tur popülerlik analizi
- `GET /api/reports/personnel-performance` - Personel performans raporu

### ReservationsController (`/api/reservations`)
- `GET /api/reservations` - Tüm rezervasyonları getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/reservations/{id}` - Rezervasyonu ID'ye göre getirir
- `GET /api/reservations/{id}/detail` - Rezervasyon detayını getirir
- `GET /api/reservations/by-guest/{guestId}` - Misafire ait rezervasyonları getirir
- `GET /api/reservations/by-personnel/{personnelId}` - Personel'e ait rezervasyonları getirir
- `GET /api/reservations/by-date-range` - Tarih aralığına göre rezervasyonları getirir
- `GET /api/reservations/by-status/{status}` - Duruma göre rezervasyonları getirir

### SettingsController (`/api/v1.0/settings`)
- `GET /api/v1.0/settings/maintenance` - Bakım modunun durumunu sorgular
- `GET /api/v1.0/settings` - Tüm ayarları getirir
- `GET /api/v1.0/settings/category/{category}` - Kategoriye göre ayarları getirir
- `GET /api/v1.0/settings/key/{key}` - Ayarı anahtara göre getirir
- `GET /api/v1.0/settings/categories` - Ayar kategorilerini getirir
- `GET /api/v1.0/settings/summary` - Sistem ayarları özetini getirir

### SmsController (`/api/sms`)
- `GET /api/sms` - Tüm SMS geçmişini getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/sms/{id}` - SMS geçmişini ID'ye göre getirir
- `GET /api/sms/by-guest/{guestId}` - Misafire gönderilen SMS'leri getirir
- `GET /api/sms/by-status/{status}` - Duruma göre SMS'leri getirir
- `GET /api/sms/statistics` - SMS istatistiklerini getirir

### ToursController (`/api/tours`)
- `GET /api/tours/calendar` - Tur takvim görünümünü getirir
- `GET /api/tours/statistics` - Tur istatistiklerini getirir

### TransfersController (`/api/transfers`)
- `GET /api/transfers` - Tüm transferleri getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/transfers/{id}` - Transferi ID'ye göre getirir
- `GET /api/transfers/{id}/detail` - Transfer detayını getirir
- `GET /api/transfers/calendar` - Transfer takvim görünümünü getirir
- `GET /api/transfers/statistics` - Transfer istatistiklerini getirir

### VehiclesController (`/api/vehicles`)
- `GET /api/vehicles` - Tüm araçları getirir (sayfalanmış, sıralanmış)
- `GET /api/vehicles/{id}` - Aracı ID'ye göre getirir

### YachtToursController (`/api/yachttours`)
- `GET /api/yachttours` - Tüm yat turlarını getirir (sayfalanmış, filtrelenmiş, sıralanmış)
- `GET /api/yachttours/{id}` - Yat turunu ID'ye göre getirir
- `GET /api/yachttours/{id}/detail` - Yat turu detayını getirir

### CalendarController (`/api/calendar`)
- `GET /api/calendar/transfer/{transferId}` - Transfer için iCal/ICS formatında takvim dosyası oluşturur
- `GET /api/calendar/citytour/{cityTourId}` - Şehir turu için iCal/ICS formatında takvim dosyası oluşturur
- `GET /api/calendar/yachttour/{yachtTourId}` - Yat turu için iCal/ICS formatında takvim dosyası oluşturur
- `GET /api/calendar/reservation/{reservationId}` - Rezervasyon için iCal/ICS formatında takvim dosyası oluşturur

### ConfigurationController (`/api/configuration`)
- `GET /api/configuration` - Tüm konfigürasyon ayarlarını getirir
- `GET /api/configuration/jwt` - JWT ayarlarını getirir
- `GET /api/configuration/pdf` - PDF ayarlarını getirir
- `GET /api/configuration/email` - E-posta ayarlarını getirir
- `GET /api/configuration/file` - Dosya ayarlarını getirir
- `GET /api/configuration/currency` - Para birimi ayarlarını getirir
- `GET /api/configuration/sms` - SMS ayarlarını getirir
- `GET /api/configuration/localization` - Yerelleştirme ayarlarını getirir
- `GET /api/configuration/app` - Uygulama ayarlarını getirir

---

## POST Endpoints

### AuthController (`/api/auth`)
- `POST /api/v1.0/auth/register` - Yeni kullanıcı kaydı yapar
- `POST /api/v1.0/auth/login` - Kullanıcı girişi yapar ve JWT token üretir
- `POST /api/v1.0/auth/forgot-password` - Şifre sıfırlama talebi
- `POST /api/v1.0/auth/reset-password` - Şifre sıfırlama
- `POST /api/v1.0/auth/refresh-token` - Refresh token ile yeni access token alır
- `POST /api/v1.0/auth/revoke-token` - Refresh token'ı iptal eder (logout)
- `POST /api/v1.0/auth/change-password` - Şifre değiştirir (giriş yapmış kullanıcı için)
- `POST /api/v1.0/auth/validate-password` - Şifre güçlülüğünü kontrol eder

### AirportsController (`/api/airports`)
- `POST /api/airports` - Yeni havalimanı ekler

### CitiesController (`/api/cities`)
- `POST /api/cities` - Yeni şehir ekler

### CityToursController (`/api/citytours`)
- `POST /api/citytours` - Yeni şehir turu ekler

### DailyNotesController (`/api/dailynotes`)
- `POST /api/dailynotes` - Yeni günlük not ekler

### DailyRevenuesController (`/api/dailyrevenues`)
- `POST /api/dailyrevenues` - Yeni günlük gelir kaydı ekler

### EmailsController (`/api/emails`)
- `POST /api/emails/queue` - E-posta kuyruğa ekler
- `POST /api/emails/queue/retry` - Başarısız e-postaları tekrar dener
- `POST /api/emails/templates` - E-posta şablonu oluşturur
- `POST /api/emails/templates/{id}/render` - Şablonu render eder (test için)
- `POST /api/emails/history/{id}/opened` - E-posta açıldı olarak işaretler
- `POST /api/emails/history/{id}/click` - E-posta tıklama sayısını artırır
- `POST /api/emails/bulk` - Toplu e-posta gönderir

### FilesController (`/api/files`)
- `POST /api/files/upload` - Dosya yükler
- `POST /api/files/upload/bulk` - Birden fazla dosya yükler
- `POST /api/files/{fileName}/share` - Dosya için paylaşım linki oluşturur

### GuestsController (`/api/guests`)
- `POST /api/guests` - Yeni misafir ekler

### ImportController (`/api/import`)
- `POST /api/import/guests/excel/preview` - Excel dosyasından misafir listesini içe aktarır (preview)
- `POST /api/import/guests/csv/preview` - CSV dosyasından misafir listesini içe aktarır (preview)
- `POST /api/import/guests/excel` - Excel dosyasından misafir listesini içe aktarır ve kaydeder
- `POST /api/import/guests/csv` - CSV dosyasından misafir listesini içe aktarır ve kaydeder
- `POST /api/import/guests/save` - Önizleme sonrası seçilen misafirleri kaydeder

### InvoicesController (`/api/invoices`)
- `POST /api/invoices/{id}/generate-pdf` - Fatura için PDF oluşturur veya yeniden oluşturur
- `POST /api/invoices/{id}/send-email` - Faturayı e-posta ile gönderir

### NotificationsController (`/api/notifications`)
- `POST /api/notifications` - Bildirim oluşturur ve gönderir
- `POST /api/notifications/send-with-template` - Şablon kullanarak bildirim gönderir
- `POST /api/notifications/test-email` - Test e-postası gönderir

### PaymentsController (`/api/payments`)
- `POST /api/payments` - Yeni ödeme oluşturur
- `POST /api/payments/{id}/complete` - Ödemeyi tamamlar (gateway callback için)
- `POST /api/payments/{id}/fail` - Ödemeyi başarısız olarak işaretler
- `POST /api/payments/{id}/refund` - Ödemeyi iade eder
- `POST /api/payments/{id}/cancel` - Ödemeyi iptal eder

### PersonnelController (`/api/personnel`)
- `POST /api/personnel` - Yeni personel ekler

### ReservationsController (`/api/reservations`)
- `POST /api/reservations` - Yeni rezervasyon oluşturur
- `POST /api/reservations/{id}/confirm` - Rezervasyonu onaylar
- `POST /api/reservations/{id}/cancel` - Rezervasyonu iptal eder

### SmsController (`/api/sms`)
- `POST /api/sms/send` - SMS gönderir
- `POST /api/sms/transfer-reminder/{transferId}` - Transfer hatırlatma SMS'i gönderir
- `POST /api/sms/tour-reminder/{tourType}/{tourId}` - Tur hatırlatma SMS'i gönderir
- `POST /api/sms/reservation-confirmation/{reservationId}` - Rezervasyon onay SMS'i gönderir

### TransfersController (`/api/transfers`)
- `POST /api/transfers` - Yeni transfer kaydı ekler

### VehiclesController (`/api/vehicles`)
- `POST /api/vehicles` - Yeni araç ekler

### YachtToursController (`/api/yachttours`)
- `POST /api/yachttours` - Yeni yat turu ekler

### CalendarController (`/api/calendar`)
- `POST /api/calendar/transfers/bulk` - Birden fazla transfer için toplu takvim dosyası oluşturur
- `POST /api/calendar/tours/bulk` - Birden fazla tur için toplu takvim dosyası oluşturur

---

## PUT Endpoints

### AirportsController (`/api/airports`)
- `PUT /api/airports/{id}` - Havalimanını günceller

### CitiesController (`/api/cities`)
- `PUT /api/cities/{id}` - Şehri günceller

### CityToursController (`/api/citytours`)
- `PUT /api/citytours/{id}` - Şehir turunu günceller

### DailyNotesController (`/api/dailynotes`)
- `PUT /api/dailynotes/{id}` - Günlük notu günceller

### DailyRevenuesController (`/api/dailyrevenues`)
- `PUT /api/dailyrevenues/{id}` - Günlük geliri günceller

### EmailsController (`/api/emails`)
- `PUT /api/emails/templates/{id}` - E-posta şablonunu günceller

### FilesController (`/api/files`)
- `PUT /api/files/{fileName}/metadata` - Dosya metadata'sını günceller

### GuestsController (`/api/guests`)
- `PUT /api/guests/{id}` - Misafiri günceller

### NotificationsController (`/api/notifications`)
- (PUT endpoint yok)

### PaymentsController (`/api/payments`)
- `PUT /api/payments/{id}` - Ödemeyi günceller

### PersonnelController (`/api/personnel`)
- `PUT /api/personnel/{id}` - Personel bilgilerini günceller

### ReservationsController (`/api/reservations`)
- `PUT /api/reservations/{id}` - Rezervasyonu günceller

### SettingsController (`/api/v1.0/settings`)
- `PUT /api/v1.0/settings/key/{key}` - Ayarı günceller
- `PUT /api/v1.0/settings/bulk` - Birden fazla ayarı günceller

### SmsController (`/api/sms`)
- `PUT /api/sms/{id}/status` - SMS durumunu günceller (gateway callback için)

### TransfersController (`/api/transfers`)
- `PUT /api/transfers/{id}` - Transferi günceller

### VehiclesController (`/api/vehicles`)
- `PUT /api/vehicles/{id}` - Aracı günceller

### YachtToursController (`/api/yachttours`)
- `PUT /api/yachttours/{id}` - Yat turunu günceller

---

## PATCH Endpoints

### NotificationsController (`/api/notifications`)
- `PATCH /api/notifications/{id}/read` - Bildirimi okundu olarak işaretler

### PersonnelController (`/api/personnel`)
- `PATCH /api/personnel/{id}/role` - Personel rolünü değiştirir

### SettingsController (`/api/v1.0/settings`)
- `PATCH /api/v1.0/settings/maintenance/toggle` - Bakım modunu açıp kapatır

### TransfersController (`/api/transfers`)
- `PATCH /api/transfers/{id}/status` - Transfer durumunu günceller
- `PATCH /api/transfers/{id}/assign-vehicle` - Transfer'e araç atar

---

## DELETE Endpoints

### AirportsController (`/api/airports`)
- `DELETE /api/airports/{id}` - Havalimanını siler

### CitiesController (`/api/cities`)
- `DELETE /api/cities/{id}` - Şehri siler

### CityToursController (`/api/citytours`)
- `DELETE /api/citytours/{id}` - Şehir turunu siler

### DailyNotesController (`/api/dailynotes`)
- `DELETE /api/dailynotes/{id}` - Günlük notu siler

### DailyRevenuesController (`/api/dailyrevenues`)
- `DELETE /api/dailyrevenues/{id}` - Günlük geliri siler

### EmailsController (`/api/emails`)
- `DELETE /api/emails/queue/clear` - Eski kuyruk kayıtlarını temizler
- `DELETE /api/emails/templates/{id}` - E-posta şablonunu siler

### FilesController (`/api/files`)
- `DELETE /api/files/{fileName}` - Dosyayı siler
- `DELETE /api/files/share/{shareToken}` - Paylaşım linkini iptal eder

### GuestsController (`/api/guests`)
- `DELETE /api/guests/{id}` - Misafiri siler

### NotificationsController (`/api/notifications`)
- `DELETE /api/notifications/{id}` - Bildirimi siler

### PaymentsController (`/api/payments`)
- `DELETE /api/payments/{id}` - Ödemeyi siler

### PersonnelController (`/api/personnel`)
- `DELETE /api/personnel/{id}` - Personeli siler

### TransfersController (`/api/transfers`)
- `DELETE /api/transfers/{id}` - Transferi siler

### VehiclesController (`/api/vehicles`)
- `DELETE /api/vehicles/{id}` - Aracı siler

### YachtToursController (`/api/yachttours`)
- `DELETE /api/yachttours/{id}` - Yat turunu siler

---

## Özet İstatistikler

- **Toplam Controller Sayısı**: 28
- **Toplam GET Endpoint**: ~150+
- **Toplam POST Endpoint**: ~50+
- **Toplam PUT Endpoint**: ~20+
- **Toplam PATCH Endpoint**: 5
- **Toplam DELETE Endpoint**: ~20+
- **Toplam Endpoint**: ~245+

