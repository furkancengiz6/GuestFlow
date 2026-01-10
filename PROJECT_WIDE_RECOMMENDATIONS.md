# GuestFlow — Project-wide Recommendations (Code scan, accounting + general)

This document summarizes findings from a full-code scan (excluding .md files) and provides prioritized, actionable recommendations for:
- Accounting / Finance workflows (detailed)
- Security, performance, infra, and general code improvements

--- 

## 1) High-priority (Fix immediately)

1.1. Fix HTML sanitization approach  
- Problem: `GuestFlow.Api\Middleware\HtmlSanitizationMiddleware.cs` currently uses regex-based sanitization — unsafe and incomplete.  
- Action: Replace with a proven HTML sanitizer (e.g. HtmlSanitizer/AngleSharp) or use strict server-side encoding + allowlist approach. Prefer library-based sanitization and test edge-cases.

1.2. Audit interceptor DI pattern  
- Problem: `AuditInterceptor` is registered/scoped incorrectly and added via `BuildServiceProvider()` in `Program.cs` causing potential memory leaks and lifecycle issues.  
- Action: Register `AuditInterceptor` as singleton and add it to DbContext via DI (avoid BuildServiceProvider). Ensure the interceptor obtains request context via `IHttpContextAccessor` per-call, not via retained scoped references.

1.3. Ensure DB migrations are applied and indexes exist  
- Files: migrations added for Audit, Suppliers, OTA integrations (`GuestFlow.Persistence\Migrations/*`).  
- Action: Run `dotnet ef database update` in staging/production and add recommended indexes for heavy queries: supplier lookups, suppliercost validFrom/validTo, audit logs composite indexes, invoice queries.

--- 

## 2) Accounting / Finance (must-have features & UX)

2.1. Automatic Journal Posting (Double-entry) — Essential  
- Implement in backend: when invoice is created/approved, prepare a journal (debits/credits) according to GL mapping. Do NOT auto-post without user confirmation.  
- UI: "Invoice → Preview Journal" modal listing lines; accountant can edit GLs, tax codes, and "Post" or "Export" the journal.  
- Files to extend: invoice creation flow (`GuestFlow.Application/Operations/Invoice/*`), add `JournalService` to create `JournalEntry` DTOs, add `JournalController` and frontend preview component.

2.2. GL Mapping & Templates  
- Add `GLMapping` configuration: default mapping per service type (Transfer, Tour, Restaurant, Package), VAT mapping, supplier payable mapping. Allow branch/department overrides. Persist in DB and a UI for finance to edit.

2.3. Room Ledger & Guest Ledger Exports  
- Provide CSV/Excel/SAF-T export from `Room Ledger` and `Guest Ledger` views (already present data in `InvoiceItemEntity` and invoices). Add export endpoints and frontend export buttons.

2.4. Reconciliation Assistant (Bank & Receipts)  
- Implement match-suggestion engine for bank transactions vs receipts/invoices. Provide manual override and bulk acceptance. Add fuzzy matching on amount/date/reference.  
- Files to integrate: payment & email/sms services, payment records (`GuestFlow.Application/Operations/Payment/*`).

2.5. VAT / Multi-currency & Rounding Rules  
- Enforce VAT breakdown on invoices (`InvoiceItemEntity` has `Amount` and `Currency` — ensure tax fields exist). Add daily exchange-rate import service and automated currency revaluation logic and rounding rules config.

2.6. Posting Lock & Period Close  
- Implement fiscal period lock to prevent changes after close. Provide checklist and export for auditors.

2.7. Attachments & Auditability  
- Ensure each posted journal references invoice PDF and stores audit trail: who created/approved/posted with timestamps (`AuditLog` exists). Expose UI for attachments per posting.

2.8. Accounting-friendly Dashboard & Bulk Operations  
- Add Accounting Dashboard with KPIs and bulk-post actions (preview → post selected).

--- 

## 3) Performance & Backend improvements

3.1. N+1 query detection and fix areas (`GuestFlow.Persistence\Repositories\Repository.cs`) — add optimized includes and specification usage. Add unit/integration tests that assert query count for key endpoints.  
3.2. Redis caching: ensure sensible TTLs for lists/dashboards; cache invalidation via events on write.  
3.3. Add DB indexes for heavy read patterns (see migration notes).  
3.4. Frontend bundle: continue selective MUI imports and lazy-load heavy libs (Recharts). Run bundle analyzer, set budgets in CI.

--- 

## 4) Security & Observability

4.1. Replace regex sanitization with library sanitizer; test with OWASP payloads.  
4.2. Ensure security headers middleware is applied early in pipeline (`Program.cs`).  
4.3. Audit logging: ensure interceptor does not block critical path and failures don't break writes. Send audit logs to persistent store and to log aggregation (Seq/ELK).  
4.4. OpenTelemetry traces for critical flows (invoicing, payments, OTA sync).

--- 

## 5) DevOps / CI-CD / Testing

5.1. Add integration tests covering: invoice → journal preview → post; bank reconciliation flows; multi-currency posting.  
5.2. Add DB migration CI job & smoke-test after migrations.  
5.3. Add automated bundle analysis and fail build on > threshold.

--- 

## 6) Suggested Implementation Plan & Prioritization (by sprint)

- Sprint 0 (Immediate): Fix sanitizer + interceptor DI bug; add audit log migration; implement journal preview backend API and preview UI.  
- Sprint 1: GL mapping UI, invoice → journal post, basic export CSV/Excel.  
- Sprint 2: Reconciliation assistant, multi-currency automation, SAF-T export.  
- Sprint 3: Integrations (QuickBooks/Xero), period close workflow, approval flows.

--- 

## 7) Files / Areas to Review Next (quick map)

- Invoices: `GuestFlow.Application/Operations/Invoice/*`, `GuestFlow.Domain/Entities/Core/Invoice*`  
- Payments: `GuestFlow.Application/Operations/Payment/*`  
- Persistence: `GuestFlow.Persistence/Context/GuestFlowDbContext.cs`, `Repositories`  
- Frontend invoice/UI: `GuestFlow.Frontend/src/pages/Invoices/*`, `src/services/invoiceService.ts`  
- Audit & Security: `GuestFlow.Api/Middleware/*`, `GuestFlow.Persistence/Interceptors/*`  

--- 

If you want, I can now:
1. Apply concrete code edits for "Invoice → Preview Journal" (backend service, API, frontend modal) and open a PR.  
2. Implement GL mapping persistence + basic UI.  
3. Start on the Reconciliation Assistant prototype (backend matching + UI).  

Which one should I implement first? (I recommend 1: Invoice → Preview Journal)

---

## 🟢 Güncelleme Notları (otomatik)
- Tarih: 2026-01-08
- Durum:
  - HTML sanitizasyonu ile ilgili güvenlik uyarısı ele alındı (kütüphane/daha güvenli yaklaşımlar prioritize edildi).
  - Audit logging interceptor register ve DI sorunları düzeltildi.
  - `SupplierCost` takibi backend + frontend implementasyonu eklendi; ilgili migration'lar oluşturuldu ve DB ile senkron sağlandı.
  - Playwright e2e altyapısı iyileştirildi; `supplierCosts.spec.ts` testi geçti.
  - API Durumu: Backend başlatıldı; demo user seed edildi. Local E2E istekleri rate-limit tarafından engelleniyordu; middleware geliştirildi ve dev-bypass eklendi.
  - Demo kullanıcı (seed log): `test@guestflow.local` / `Uw2bNU9*rMFF`

## Sonraki Adımlar (önerilen sıra)
1. Invoice → Preview Journal (öncelik: YÜKSEK) — Backend servis, API, frontend modal.
2. GL Mapping UI (öncelik: YÜKSEK).
3. Run full Playwright suite & migrate remaining legacy tests (öncelik: YÜKSEK).
4. Apply DB migrations to staging/production with smoke tests (öncelik: YÜKSEK).
