# GuestFlow — Security Policy

Bu doküman, GuestFlow projesi için güvenlik bildirim ve destek politikasını tanımlar.

## Supported Versions (Desteklenen Versiyonlar)

GuestFlow için **güvenlik güncellemeleri** aşağıdaki sürümlere verilir:

| Version | Supported |
| --- | --- |
| 1.x | ✅ |
| < 1.0 | ❌ |

> Not: Bu repo aktif geliştirme altında olduğundan “1.x” = `master/main` ve release tag’leri olarak düşünülmelidir.

## Reporting a Vulnerability (Zafiyet Bildirimi)

### Nereden Bildirebilirim?
- **Tercih edilen yöntem**: GitHub “Security Advisory” / “Report a vulnerability” (repo Security sekmesi)
- Alternatif: Repo owner’ına direkt mesaj/e-posta (kurum içi kullanımda)

### Bildirimde Neler Olsun?
- Etkilenen endpoint/modül (örn. `auth`, `files`, `payments`, `imports`)
- PoC adımları veya minimal örnek istek/response
- Etki (PII sızıntısı, auth bypass, RCE, vs.)
- Varsa log/stack trace/screenshot

### Geri Dönüş Süresi (SLA)
- **İlk yanıt hedefi**: 72 saat içinde
- **Triyaj hedefi**: 7 gün içinde (severity + kapsam netleştirme)

### Güvenlik İlkeleri (Özet)
- **Secrets**: Production’da `JWT__SecretKey` zorunlu; demo seed kapalı.
- **Auth**: JWT + refresh token (cookie) ve role-based authorization.
- **Rate limiting / security headers / sanitization**: middleware pipeline ile uygulanır.

## Out of Scope (Kapsam Dışı)
- Local ortamda bilinçli açılmış dev bypass’lar (örn. E2E bypass) — production’da kapalı olmalıdır.
- “Self-hosted” yanlış konfigürasyon kaynaklı açıklar (ör. CORS’ı wildcard yapma).
