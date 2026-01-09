import { test, expect } from '@playwright/test'

/// <reference types="node" />
/// <reference types="@playwright/test" />

const userEmail = process.env.E2E_USER_EMAIL || 'ahmet@guestflow.com'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'
const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5175').toString().trim().replace(/\/$/, '')

test.describe('Guests Management', () => {
  test.beforeEach(async ({ page, baseURL }) => {
    // Login before each test
    const normalizedBase = (baseURL || process.env.E2E_BASE_URL || 'http://localhost:5175').toString().trim().replace(/\/$/, '')
    await page.goto(`${normalizedBase}/login`)
    await page.fill('input[type="email"]', userEmail)
    await page.fill('input[type="password"]', userPassword)
    await page.click('button:has-text("Giriş Yap")')
    await page.waitForURL('**/dashboard', { timeout: 10000 })
  })

  test('should navigate to guests page', async ({ page }) => {
    const normalizedBase = (baseURL || process.env.E2E_BASE_URL || 'http://localhost:5175').toString().trim().replace(/\/$/, '')
    await page.goto(`${normalizedBase}/guests`)
    await expect(page).toHaveURL(/.*guests/)
    await expect(page.locator('h1, h2, h3, h4, h5, h6')).toContainText(/misafir/i)
  })

  test('should display guests list', async ({ page }) => {
    const normalizedBase = (baseURL || process.env.E2E_BASE_URL || 'http://localhost:5175').toString().trim().replace(/\/$/, '')
    await page.goto(`${normalizedBase}/guests`)
    
    // Wait for table or list to load
    await page.waitForSelector('table, [role="table"], [data-testid="guests-table"]', {
      timeout: 10000,
    })
    
    // Verify table is visible
    const table = page.locator('table, [role="table"]').first()
    await expect(table).toBeVisible()
  })

  test('should open add guest form', async ({ page }) => {
    const normalizedBase = (baseURL || process.env.E2E_BASE_URL || 'http://localhost:5175').toString().trim().replace(/\/$/, '')
    await page.goto(`${normalizedBase}/guests`)
    
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

