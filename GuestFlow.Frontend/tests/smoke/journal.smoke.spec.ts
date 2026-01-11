import { test, expect } from '@playwright/test'
import { ensureLoggedIn } from '../utils/testHelpers'
import { setupMockApi } from '../utils/mockApi'

const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')
const userEmail = process.env.E2E_USER_EMAIL || 'smoke@guestflow.local'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'

test.describe('Smoke: invoice -> journal preview -> post (+ export)', () => {
  test.beforeEach(async ({ page }) => {
    // We want precise control over invoice/journal/export responses, so disable the generic fallback.
    await setupMockApi(page, { includeGenericFallback: false })

    const mockedAuth = JSON.stringify({
      user: { id: 1, email: userEmail, fullName: 'Smoke User', role: 'Admin' },
      isAuthenticated: true,
    })
    await page.addInitScript((auth) => {
      try {
        localStorage.setItem('auth-storage', auth)
        localStorage.setItem('VITE_E2E_BYPASS', 'true')
      } catch {
        /* ignore */
      }
    }, mockedAuth)

    // State: after posting, invoice detail should reflect posted JE
    let posted = false
    const invoiceId = 123

    const invoiceDetailApi = /\/api\/(v[\d.]+\/)?invoices\/\d+\/detail(\b|\/|\?|$)/i
    const journalPreviewApi = /\/api\/(v[\d.]+\/)?journal\/preview(\b|\/|\?|$)/i
    const journalPostApi = /\/api\/(v[\d.]+\/)?journal\/post(\b|\/|\?|$)/i
    const exportJournalCsvApi = /\/api\/(v[\d.]+\/)?export\/journal\/csv(\b|\/|\?|$)/i
    const anyApi = /\/api\/.*/i

    // Catch-all for any other API calls the layout triggers
    // NOTE: We register this FIRST so more specific mocks below can override it.
    await page.route(anyApi, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true, data: [] }),
      })
    })

    await page.route(invoiceDetailApi, async (route) => {
      const detail = {
        id: invoiceId,
        invoiceNumber: 1000123,
        issueDate: '2026-01-11',
        totalAmount: 100,
        netTotal: 83.33,
        vatTotal: 16.67,
        currency: 'TRY',
        notes: 'smoke invoice',
        pdfUrl: '',
        hasPdf: false,
        createdDate: '2026-01-11',
        isJournalPosted: posted,
        journalEntryId: posted ? 555 : undefined,
        journalPostingDate: posted ? '2026-01-11' : undefined,
        guest: {
          id: 1,
          fullName: 'Smoke Guest',
          guestCode: 'SMK-0001',
          email: 'guest@guestflow.local',
          phoneNumber: '+905551234567',
          nationality: 'TR',
          isSpecialGuest: false,
        },
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true, data: detail }),
      })
    })

    await page.route(journalPreviewApi, async (route) => {
      const preview = {
        invoiceId,
        description: 'Invoice 1000123',
        currency: 'TRY',
        lines: [
          { accountCode: '1100', debit: 100, credit: 0, description: 'Accounts Receivable' },
          { accountCode: '4000', debit: 0, credit: 83.33, description: 'Transfer #1' },
          { accountCode: '3910', debit: 0, credit: 16.67, description: 'VAT Payable' },
        ],
        totalDebit: 100,
        totalCredit: 100,
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true, data: preview }),
      })
    })

    await page.route(journalPostApi, async (route) => {
      posted = true
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true, data: true, message: 'Journal posted successfully' }),
      })
    })

    await page.route(exportJournalCsvApi, async (route) => {
      const csv = [
        'JournalEntryId,InvoiceId,PostingDate,Currency,Description,TotalDebit,TotalCredit,CreatedBy,LineAccountCode,LineDebit,LineCredit,LineDescription',
        '555,123,2026-01-11,TRY,Posted for Invoice 1000123,100,100,system,1100,100,0,AR',
      ].join('\n')

      await route.fulfill({
        status: 200,
        contentType: 'text/csv; charset=utf-8',
        body: csv,
      })
    })

    await ensureLoggedIn(page, userEmail, userPassword)
  })

  test('can preview and post journal, then export CSV endpoint responds', async ({ page }) => {
    // Go directly to invoice detail
    await page.goto(`${DEFAULT_BASE}/invoices/123`)
    await page.waitForLoadState('networkidle', { timeout: 20000 })

    // Open preview
    await page.getByRole('button', { name: /journal preview/i }).click()
    await expect(page.getByRole('heading', { name: 'Journal Preview' })).toBeVisible({ timeout: 15000 })

    // VAT line should be present
    await expect(page.getByText('3910')).toBeVisible({ timeout: 15000 })

    // Post
    await page.getByRole('button', { name: /post journal/i }).click()
    await expect(page.getByRole('button', { name: /^posted$/i })).toBeVisible({ timeout: 15000 })

    // Invoice header should show "Journal Posted"
    await expect(page.getByText(/journal posted/i)).toBeVisible({ timeout: 15000 })

    // Export endpoint should respond with CSV (use page fetch so route interception applies)
    const exportResult = await page.evaluate(async () => {
      const resp = await fetch(
        'http://localhost:5146/api/v1.0/Export/journal/csv?startDate=2026-01-10&endDate=2026-01-11'
      )
      return {
        status: resp.status,
        contentType: resp.headers.get('content-type') || '',
        text: await resp.text(),
      }
    })
    expect(exportResult.status).toBe(200)
    expect(exportResult.contentType).toContain('text/csv')
    expect(exportResult.text).toContain('JournalEntryId,InvoiceId,PostingDate')
    expect(exportResult.text).toContain('555,123,2026-01-11')
  })
})

