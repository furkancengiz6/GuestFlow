import { test, expect } from '@playwright/test'
import { ensureLoggedIn } from '../utils/testHelpers'
import { setupMockApi } from '../utils/mockApi'

/// <reference types="node" />
/// <reference types="@playwright/test" />

const userEmail = (globalThis as any).process?.env?.E2E_USER_EMAIL || 'ahmet@guestflow.com'
const userPassword = (globalThis as any).process?.env?.E2E_USER_PASSWORD || 'Admin123!'
const DEFAULT_BASE = ((globalThis as any).process?.env?.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')

test.describe('Guests Management', () => {
  // Helper to retry navigation if Vite dynamic import fails and app shows a transient error message
  async function ensureComponentLoaded(page: any) {
    const errorLocator = page.locator('text=Failed to load component')
    for (let i = 0; i < 3; i++) {
      if (!(await errorLocator.isVisible().catch(() => false))) return
      await page.reload()
      await page.waitForLoadState('networkidle', { timeout: 15000 })
    }
  }
  test.beforeEach(async ({ page }) => {
    await setupMockApi(page)
    // Ensure tests run authenticated by injecting a mocked auth state in localStorage
    const mockedAuth = JSON.stringify({ user: { id: 1, email: userEmail, fullName: 'Test User', role: 'Admin' }, isAuthenticated: true })
    await page.addInitScript((auth) => {
      try {
        localStorage.setItem('auth-storage', auth)
        localStorage.setItem('VITE_E2E_BYPASS', 'true')
      } catch { /* ignore */ }
    }, mockedAuth)
    await ensureLoggedIn(page, userEmail, userPassword)
  })

  test('should navigate to guests page', async ({ page }) => {
    // Prefetch the GuestsPage module to avoid intermittent Vite JSON responses on dynamic import
    await page.request.get(`${DEFAULT_BASE}/src/pages/Guests/GuestsPage.tsx`).catch(() => {})
    await page.goto(`${DEFAULT_BASE}/guests`)
    await ensureComponentLoaded(page)
    await expect(page).toHaveURL(/.*guests/)
    // Wait for network idle and for a heading to appear (avoid flakiness on dynamic imports)
    await page.waitForLoadState('networkidle', { timeout: 15000 })
    await page.waitForSelector('h1, h2, h3, h4, h5, h6', { timeout: 15000 })
    // Use the first heading to avoid strict locator errors when multiple headings exist
    await expect(page.locator('h1, h2, h3, h4, h5, h6').first()).toContainText(/misafir/i, { timeout: 15000 })
  })

  test('should display guests list', async ({ page }) => {
    // Prefetch the GuestsPage module to warm Vite dev server and avoid intermittent module load failures
    await page.request.get(`${DEFAULT_BASE}/src/pages/Guests/GuestsPage.tsx`).catch(() => {})
    await page.goto(`${DEFAULT_BASE}/guests`)
    await ensureComponentLoaded(page)
    
    // Wait for network and table or list to load (dynamic imports can delay rendering)
    await page.waitForLoadState('networkidle', { timeout: 20000 })
    await page.waitForSelector('table, [role="table"], [data-testid="guests-table"]', {
      timeout: 20000,
    })
    
    // Verify table is visible
    const table = page.locator('table, [role="table"]').first()
    await expect(table).toBeVisible()
  })

  test('should open add guest form', async ({ page }) => {
    // Prefetch the GuestsPage module to warm Vite dev server
    await page.request.get(`${DEFAULT_BASE}/src/pages/Guests/GuestsPage.tsx`).catch(() => {})
    await page.goto(`${DEFAULT_BASE}/guests`)
    await ensureComponentLoaded(page)
    
    // Click add button
    const addButton = page.locator('button:has-text("Ekle"), button:has-text("Yeni"), [aria-label*="Ekle"]').first()
    if (await addButton.isVisible()) {
      await addButton.click()
      
      // Wait for form dialog
      await page.waitForSelector('form, [role="dialog"]', { timeout: 5000 })
      
      // Verify form fields are visible
      await expect(page.locator('input[name*="name"], input[name*="fullName"]').first()).toBeVisible({ timeout: 5000 })
    }
  })
})

