import type { Page } from '@playwright/test'
import { LoginPage } from '../page-objects/LoginPage'

export async function ensureLoggedIn(page: Page, email: string, password: string) {
  const login = new LoginPage(page)
  const base = (process.env.E2E_BASE_URL || '').toString().trim().replace(/\/$/, '')
  await login.goto(base)
  // If storageState was injected via addInitScript, it will be present after navigation.
  const hasAuthAfter = await page.evaluate(() => {
    try {
      return !!localStorage.getItem('auth-storage')
    } catch {
      return false
    }
  })
  if (hasAuthAfter) {
    // Avoid relying on /login redirect behavior; go directly to dashboard for stability.
    await page.goto(`${base}/dashboard`)
    await page.waitForURL('**/dashboard', { timeout: 15000 })
    return
  }

  await page.waitForSelector('input[name="email"]', { timeout: 5000 })
  await login.login(email, password)
  // wait for a predictable post-login signal (dashboard route)
  await page.waitForURL('**/dashboard', { timeout: 15000 })
}

