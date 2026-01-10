import { test, expect } from '@playwright/test'
import { ensureLoggedIn } from '../utils/testHelpers'
import { setupMockApi } from '../utils/mockApi'

const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')
const userEmail = process.env.E2E_USER_EMAIL || 'smoke@guestflow.local'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'

test.describe('Smoke: dashboard', () => {
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

  test('loads dashboard route', async ({ page }) => {
    await page.goto(`${DEFAULT_BASE}/dashboard`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })
    await expect(page).toHaveURL(/dashboard/)
    await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible({ timeout: 15000 })
  })
})

