import type { Page } from '@playwright/test'
import { LoginPage } from '../page-objects/LoginPage'

export async function ensureLoggedIn(page: Page, email: string, password: string) {
  const login = new LoginPage(page)
  await login.goto(process.env.E2E_BASE_URL)
  // If storageState was injected via addInitScript, it will be present after navigation.
  const hasAuthAfter = await page.evaluate(() => {
    try {
      return !!localStorage.getItem('auth-storage')
    } catch {
      return false
    }
  })
  if (hasAuthAfter) {
    try {
      await page.waitForURL('**/dashboard', { timeout: 3000 })
      return
    } catch {
      // not redirected, continue to login flow
    }
  }

  await page.waitForSelector('input[name="email"]', { timeout: 5000 })
  await login.login(email, password)
  // wait for a predictable post-login signal (dashboard route)
  await page.waitForURL('**/dashboard', { timeout: 15000 })
}

