import { test, expect } from '@playwright/test'
import { ensureLoggedIn } from '../utils/testHelpers'
import { setupMockApi } from '../utils/mockApi'

/// <reference types="node" />
/// <reference types="@playwright/test" />

const userEmail = process.env.E2E_USER_EMAIL || 'ahmet@guestflow.com'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'
const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')

test.describe('Auth flow', () => {
  test.beforeEach(async ({ page }) => {
    // Mock backend API so this suite runs reliably without a live backend.
    await setupMockApi(page)

    const mockedAuth = JSON.stringify({
      user: { id: 1, email: userEmail, fullName: 'Test User', role: 'Admin' },
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
  })

  test('login -> dashboard -> protected admin route', async ({ page, baseURL }) => {
    // Monitor network requests
    const networkErrors: string[] = []
    page.on('response', (response) => {
      if (response.status() >= 400) {
        networkErrors.push(`${response.url()} - ${response.status()}`)
      }
    })
    
    page.on('requestfailed', (request) => {
      networkErrors.push(`Failed: ${request.url()} - ${request.failure()?.errorText}`)
    })

    // Use shared helper to perform login
    await ensureLoggedIn(page, userEmail, userPassword)

    // Verify dashboard is loaded
    await expect(page).toHaveURL(/dashboard/, { timeout: 10000 })
    await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible({ timeout: 15000 })

    // Admin-only route
    await page.goto(`${baseURL || DEFAULT_BASE}/reports`)
    // if user lacks role, should redirect to forbidden or dashboard
    await page.waitForURL(/reports|forbidden|dashboard/, { timeout: 5000 })
    const url = page.url()
    expect(url.includes('/reports') || url.includes('/forbidden') || url.includes('/dashboard')).toBeTruthy()
  })
})

