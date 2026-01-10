import { test, expect } from '@playwright/test'
import { ensureLoggedIn } from '../utils/testHelpers'
import { setupMockApi } from '../utils/mockApi'

const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')
const userEmail = process.env.E2E_USER_EMAIL || 'smoke@guestflow.local'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'

test.describe('Smoke: guests', () => {
  test.beforeEach(async ({ page }) => {
    await setupMockApi(page)
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

    await ensureLoggedIn(page, userEmail, userPassword)
  })

  test('loads guests route', async ({ page }) => {
    // Warm Vite module graph to reduce dynamic-import flakiness
    await page.request.get(`${DEFAULT_BASE}/src/pages/Guests/GuestsPage.tsx`).catch(() => {})
    await page.goto(`${DEFAULT_BASE}/guests`)
    await page.waitForLoadState('networkidle', { timeout: 20000 })
    await expect(page).toHaveURL(/guests/)
    await page.waitForSelector('h1, h2, h3, h4, h5, h6', { timeout: 15000 })
    await expect(page.locator('h1, h2, h3, h4, h5, h6').first()).toContainText(/misafir/i, { timeout: 15000 })
  })
})

