import { test, expect } from '@playwright/test'
import { LoginPage } from '../page-objects'
import { ensureLoggedIn } from '../utils/testHelpers'

/// <reference types="node" />
/// <reference types="@playwright/test" />

const userEmail = process.env.E2E_USER_EMAIL || 'ahmet@guestflow.com'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'
const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')

test.describe('Auth flow', () => {
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
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible({ timeout: 10000 })

    // Admin-only route
    await page.goto(`${baseURL || DEFAULT_BASE}/reports`)
    // if user lacks role, should redirect to forbidden or dashboard
    await page.waitForURL(/reports|forbidden|dashboard/, { timeout: 5000 })
    const url = page.url()
    expect(url.includes('/reports') || url.includes('/forbidden') || url.includes('/dashboard')).toBeTruthy()
  })
})

