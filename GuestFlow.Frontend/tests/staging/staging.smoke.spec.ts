import { test, expect, type Page } from '@playwright/test'

const baseURL = (process.env.E2E_BASE_URL || '').toString().trim().replace(/\/$/, '')
const apiBase =
  (process.env.E2E_API_BASE_URL || '').toString().trim().replace(/\/$/, '') ||
  `${baseURL}/api/v1.0`

async function getAccessToken(page: Page): Promise<string> {
  const resp = await page.request.post(`${apiBase}/auth/refresh-token`, {
    data: {},
  })
  expect(resp.status(), 'refresh-token should succeed').toBe(200)
  const body: any = await resp.json()
  const accessToken = body?.data?.accessToken || body?.accessToken
  expect(accessToken, 'refresh-token must return accessToken').toBeTruthy()
  return String(accessToken)
}

async function pickInvoiceId(page: Page, token: string): Promise<number> {
  const explicit = Number(process.env.E2E_INVOICE_ID || '')
  if (explicit) return explicit

  // Try first page and pick first invoice that is NOT journal-posted (by checking detail).
  const listResp = await page.request.get(`${apiBase}/Invoices?pageNumber=1&pageSize=10`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  expect(listResp.status(), 'Invoices list must succeed').toBe(200)
  const listJson: any = await listResp.json()
  const items: any[] = listJson?.data?.data || listJson?.data || listJson?.items || []

  for (const inv of items) {
    const id = Number(inv?.id)
    if (!id) continue
    const detailResp = await page.request.get(`${apiBase}/Invoices/${id}/detail`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (!detailResp.ok()) continue
    const detailJson: any = await detailResp.json()
    const detail = detailJson?.data || detailJson
    if (detail && detail.isJournalPosted === false) return id
  }

  throw new Error(
    'Could not find a non-posted invoice automatically. Set E2E_INVOICE_ID to a known invoice id in staging.'
  )
}

test.describe('Staging smoke (real backend)', () => {
  // This suite requires a REAL backend + authenticated refresh cookie created by globalSetup UI login.
  // Skip by default so local/CI mocked E2E runs are stable.
  test.skip(
    !process.env.E2E_REAL_BACKEND || !baseURL,
    'Set E2E_REAL_BACKEND=true and E2E_BASE_URL/E2E_API_BASE_URL to run staging smoke against a real backend.'
  )

  test('auth: login lands on dashboard', async ({ page }) => {
    await page.goto(`${baseURL}/dashboard`)
    await page.waitForLoadState('domcontentloaded')
    await expect(page).toHaveURL(/dashboard/)
    await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible({ timeout: 15000 })
  })

  test('invoice -> journal preview -> post -> export', async ({ page }) => {
    // Ensure we have a fresh access token (via refresh cookie from UI login in globalSetup)
    const token = await getAccessToken(page)
    const invoiceId = await pickInvoiceId(page, token)

    await page.goto(`${baseURL}/invoices/${invoiceId}`)
    await page.waitForLoadState('networkidle', { timeout: 20000 })

    // Open journal preview dialog
    await page.getByRole('button', { name: /journal preview/i }).click()
    await expect(page.getByRole('heading', { name: 'Journal Preview' })).toBeVisible({ timeout: 15000 })

    // Post (should succeed once; UI may disable if already posted)
    const postBtn = page.getByRole('button', { name: /post journal/i })
    await expect(postBtn).toBeVisible({ timeout: 15000 })
    if (await postBtn.isEnabled()) {
      await postBtn.click()
    }

    // Invoice header should show journal posted state after post (or if already posted)
    await expect(page.getByText(/journal posted/i)).toBeVisible({ timeout: 30000 })

    // Export: journal CSV endpoint responds for a small date range
    const today = new Date().toISOString().slice(0, 10)
    const exportResp = await page.request.get(
      `${apiBase}/Export/journal/csv?startDate=${today}&endDate=${today}`,
      { headers: { Authorization: `Bearer ${token}` } }
    )
    expect(exportResp.status(), 'Export CSV should return 200').toBe(200)
    const ct = exportResp.headers()['content-type'] || ''
    expect(ct, 'Export CSV content-type').toContain('text/csv')
  })

  test('CRUD smoke (API): create -> update -> delete guest', async ({ page }) => {
    const token = await getAccessToken(page)

    const unique = `e2e-${Date.now()}`
    const createResp = await page.request.post(`${apiBase}/Guests`, {
      headers: { Authorization: `Bearer ${token}` },
      data: {
        fullName: `E2E Guest ${unique}`,
        email: `e2e.${unique}@guestflow.local`,
        phoneNumber: '+900000000000',
        nationality: 'TR',
        isSpecialGuest: false,
      },
    })
    expect(createResp.status(), 'Guest create should succeed').toBe(200)
    const createJson: any = await createResp.json()
    const created = createJson?.data || createJson
    const guestId = Number(created?.id)
    expect(guestId, 'Guest id').toBeTruthy()

    const updateResp = await page.request.put(`${apiBase}/Guests/${guestId}`, {
      headers: { Authorization: `Bearer ${token}` },
      data: {
        fullName: `E2E Guest ${unique} Updated`,
        email: `e2e.${unique}@guestflow.local`,
        phoneNumber: '+900000000000',
        nationality: 'TR',
        isSpecialGuest: true,
      },
    })
    expect(updateResp.status(), 'Guest update should succeed').toBe(200)

    const deleteResp = await page.request.delete(`${apiBase}/Guests/${guestId}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    // Some endpoints return 200/204; accept both
    expect([200, 204], 'Guest delete should succeed').toContain(deleteResp.status())
  })
})

