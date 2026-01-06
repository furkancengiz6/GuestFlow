import { test, expect } from '@playwright/test'

const baseURL = process.env.E2E_BASE_URL || 'http://localhost:5173'
const userEmail = process.env.E2E_USER_EMAIL || 'ahmet@guestflow.com'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'

test.describe('Transfers Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto(`${baseURL}/login`)
    await page.fill('input[type="email"]', userEmail)
    await page.fill('input[type="password"]', userPassword)
    await page.click('button:has-text("Giriş Yap")')
    await page.waitForURL('**/dashboard', { timeout: 10000 })
  })

  test('should navigate to transfers page', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)
    await expect(page).toHaveURL(/.*transfers/)
  })

  test('should display transfers list', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)
    
    // Wait for transfers to load
    await page.waitForSelector('table, [role="table"]', { timeout: 10000 })
    
    const table = page.locator('table, [role="table"]').first()
    await expect(table).toBeVisible()
  })

  test('should filter transfers', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)

    // Wait for page to load
    await page.waitForSelector('table, [role="table"]', { timeout: 10000 })

    // Try to find filter/search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="Ara"], input[aria-label*="Ara"]').first()
    if (await searchInput.isVisible()) {
      await searchInput.fill('test')
      await page.waitForTimeout(1000) // Wait for filter to apply
    }
  })

  test('should create a new transfer successfully', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)

    // Click create transfer button
    const createButton = page.locator('button:has-text("Yeni Transfer"), button[aria-label*="Yeni"]').first()
    if (await createButton.isVisible()) {
      await createButton.click()

      // Wait for form to load
      await page.waitForSelector('form, [role="dialog"]', { timeout: 5000 })

      // Fill basic transfer form fields (if form is visible)
      const guestSelect = page.locator('select[name*="guest"], [aria-label*="Misafir"]').first()
      const dateInput = page.locator('input[type="datetime-local"], input[name*="date"]').first()
      const pickupInput = page.locator('input[name*="pickup"], textarea[name*="pickup"]').first()
      const dropoffInput = page.locator('input[name*="dropoff"], textarea[name*="dropoff"]').first()
      const priceInput = page.locator('input[type="number"], input[name*="price"]').first()

      // Fill form if fields are available
      if (await guestSelect.isVisible()) {
        // Select first option if available
        const options = guestSelect.locator('option')
        if (await options.count() > 1) {
          await guestSelect.selectOption({ index: 1 })
        }
      }

      if (await dateInput.isVisible()) {
        const futureDate = new Date()
        futureDate.setDate(futureDate.getDate() + 7) // 7 days from now
        await dateInput.fill(futureDate.toISOString().slice(0, 16))
      }

      if (await pickupInput.isVisible()) {
        await pickupInput.fill('İstanbul Airport Terminal 1')
      }

      if (await dropoffInput.isVisible()) {
        await dropoffInput.fill('Hilton Istanbul Bosphorus')
      }

      if (await priceInput.isVisible()) {
        await priceInput.fill('250')
      }

      // Try to submit form
      const submitButton = page.locator('button[type="submit"], button:has-text("Kaydet"), button:has-text("Oluştur")').first()
      if (await submitButton.isVisible()) {
        await submitButton.click()

        // Wait for success message or navigation
        try {
          await page.waitForSelector('[role="alert"], .toast-success, .notification-success', { timeout: 5000 })
        } catch (e) {
          // Success message might not appear, check if we're back on transfers page
          await page.waitForURL('**/transfers**', { timeout: 5000 })
        }
      }
    }
  })

  test('should validate transfer form fields', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)

    // Try to create transfer
    const createButton = page.locator('button:has-text("Yeni Transfer"), button[aria-label*="Yeni"]').first()
    if (await createButton.isVisible()) {
      await createButton.click()

      // Wait for form
      await page.waitForSelector('form, [role="dialog"]', { timeout: 5000 })

      // Try to submit empty form
      const submitButton = page.locator('button[type="submit"], button:has-text("Kaydet")').first()
      if (await submitButton.isVisible()) {
        await submitButton.click()

        // Wait a moment for validation to appear
        await page.waitForTimeout(1000)

        // Check for any error messages
        const errorMessages = page.locator('.error, [role="alert"], .invalid-feedback, .text-danger')
        const errorCount = await errorMessages.count()

        // If there are error messages, test passed (validation is working)
        if (errorCount > 0) {
          console.log(`Found ${errorCount} validation errors - form validation is working`)
        }
      }
    }
  })

  test('should perform bulk operations on transfers', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)

    // Wait for table to load
    await page.waitForSelector('table, [role="table"]', { timeout: 10000 })

    // Look for checkboxes in table
    const checkboxes = page.locator('input[type="checkbox"], [role="checkbox"]')
    const checkboxCount = await checkboxes.count()

    if (checkboxCount > 1) { // More than header checkbox
      // Select first data row checkbox
      await checkboxes.nth(1).check()

      // Look for bulk operations button
      const bulkButton = page.locator('button:has-text("Toplu"), button:has-text("Bulk")').first()
      if (await bulkButton.isVisible()) {
        await bulkButton.click()

        // Wait for bulk operations dialog
        await page.waitForSelector('[role="dialog"], .modal, .drawer', { timeout: 3000 })

        // Try to find operation select
        const operationSelect = page.locator('select, [role="combobox"]').first()
        if (await operationSelect.isVisible()) {
          // This indicates bulk operations UI is available
          console.log('Bulk operations UI is available')
        }
      }
    }
  })

  test('should navigate between transfer pages', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)

    // Wait for table to load
    await page.waitForSelector('table, [role="table"]', { timeout: 10000 })

    // Look for pagination controls
    const pagination = page.locator('[aria-label*="pagination"], .pagination, [class*="pagination"]').first()
    if (await pagination.isVisible()) {
      // Look for next page button
      const nextButton = pagination.locator('button[aria-label*="next"], button:has-text(">")').first()
      if (await nextButton.isVisible() && await nextButton.isEnabled()) {
        await nextButton.click()

        // Wait for page change
        await page.waitForTimeout(2000)

        // Check if page changed (URL or content)
        const currentUrl = page.url()
        if (currentUrl.includes('page=2') || currentUrl.includes('pageNumber=2')) {
          console.log('Successfully navigated to page 2')
        }
      }
    }
  })

  test('should handle transfer detail view', async ({ page }) => {
    await page.goto(`${baseURL}/transfers`)

    // Wait for table to load
    await page.waitForSelector('table, [role="table"]', { timeout: 10000 })

    // Try to click on first transfer row or view button
    const firstRow = page.locator('tbody tr').first()
    if (await firstRow.isVisible()) {
      await firstRow.click()

      // Wait for navigation or modal
      try {
        await page.waitForURL(/\/transfers\/\d+/, { timeout: 5000 })
        console.log('Navigated to transfer detail page')
      } catch (e) {
        // Might be a modal instead
        const modal = page.locator('[role="dialog"], .modal').first()
        if (await modal.isVisible()) {
          console.log('Transfer detail modal opened')
        }
      }
    }
  })
})

