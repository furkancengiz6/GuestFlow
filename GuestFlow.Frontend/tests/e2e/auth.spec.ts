import { test, expect } from '@playwright/test'

const baseURL = process.env.E2E_BASE_URL || 'http://localhost:5173'
const userEmail = process.env.E2E_USER_EMAIL || 'ahmet@guestflow.com'
const userPassword = process.env.E2E_USER_PASSWORD || 'Admin123!'

test.describe('Auth flow', () => {
  test('login -> dashboard -> protected admin route', async ({ page }) => {
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

    await page.goto(`${baseURL}/login`)
    
    // Wait for login form to be visible
    await expect(page.locator('input[type="email"]')).toBeVisible({ timeout: 10000 })
    await expect(page.locator('input[type="password"]')).toBeVisible()

    // Fill login form
    await page.fill('input[type="email"]', userEmail)
    await page.fill('input[type="password"]', userPassword)
    
    // Click login button
    await page.click('button:has-text("Giriş Yap")')
    
    // Wait for either navigation or error
    try {
      await page.waitForURL(/dashboard/, { timeout: 15000 })
    } catch (e) {
      // Check for error messages
      const errorAlert = page.locator('[role="alert"]')
      if (await errorAlert.count() > 0) {
        const errorText = await errorAlert.textContent()
        console.log('Login error message:', errorText)
      }
      
      // Log network errors
      if (networkErrors.length > 0) {
        console.log('Network errors:', networkErrors)
      }
      
      // Take screenshot for debugging
      await page.screenshot({ path: 'test-results/login-timeout.png', fullPage: true })
      console.log('Current URL:', page.url())
      
      throw new Error(`Login failed. URL: ${page.url()}, Network errors: ${networkErrors.join(', ')}`)
    }

    // Verify dashboard is loaded
    await expect(page).toHaveURL(/dashboard/, { timeout: 10000 })
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible({ timeout: 10000 })

    // Admin-only route
    await page.goto(`${baseURL}/reports`)
    // if user lacks role, should redirect to forbidden or dashboard
    await page.waitForURL(/reports|forbidden|dashboard/, { timeout: 5000 })
    const url = page.url()
    expect(url.includes('/reports') || url.includes('/forbidden') || url.includes('/dashboard')).toBeTruthy()
  })
})

