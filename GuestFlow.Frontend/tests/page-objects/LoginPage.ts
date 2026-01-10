import type { Page } from '@playwright/test'

export class LoginPage {
  readonly page: Page

  constructor(page: Page) {
    this.page = page
  }

  async goto(baseURL?: string) {
    const base = baseURL || (process.env.E2E_BASE_URL ?? '')
    await this.page.goto(`${base}/login`)
  }

  async login(email: string, password: string) {
    const emailInput = this.page.locator('input[name="email"]')
    const passwordInput = this.page.locator('input[name="password"]')
    const submitButton = this.page.locator('button[type="submit"]')

    await emailInput.waitFor({ state: 'visible', timeout: 15000 })
    await emailInput.fill(email)
    await passwordInput.waitFor({ state: 'visible', timeout: 5000 })
    await passwordInput.fill(password)
    await submitButton.waitFor({ state: 'visible', timeout: 5000 })
    await submitButton.click()
  }
}

