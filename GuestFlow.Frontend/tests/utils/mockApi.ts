import type { Page, Route } from '@playwright/test'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

export async function setupMockApi(page: Page) {
  const __filename = fileURLToPath(import.meta.url)
  const __dirname = path.dirname(__filename)
  const fixturesDir = path.resolve(__dirname, '..', 'fixtures')
  const guests = JSON.parse(fs.readFileSync(path.join(fixturesDir, 'guests.json'), 'utf8'))
  const transfers = JSON.parse(fs.readFileSync(path.join(fixturesDir, 'transfers.json'), 'utf8'))

  // Forward page console and pageerror to test output for easier debugging
  page.on('console', (msg) => {
    try {
      // include location if available (Playwright ConsoleMessage.location is a function)
      const loc = msg.location ? msg.location() : undefined
      const location = loc ? `${loc.url}:${loc.lineNumber}` : ''
      // serialize args (msg.args is a function returning JSHandle[])
      const argsHandles = msg.args ? msg.args() : []
      const args = argsHandles.length ? argsHandles.map((a: any) => a.toString()).join(' ') : msg.text()
      // eslint-disable-next-line no-console
      console.log(`[page.console] ${msg.type()} ${location} ${args}`)
    } catch {
      // ignore
    }
  })
  page.on('pageerror', (err) => {
    // eslint-disable-next-line no-console
    console.error('[page.error]', String(err))
  })
  // Log all network requests from the page for debugging
  page.on('request', (req) => {
    try {
      console.log(`[page.request] ${req.method()} ${req.url()}`)
    } catch { /* ignore */ }
  })

  // IMPORTANT: Keep these routes narrowly scoped to API calls.
  // Do NOT use broad patterns like "**/Guests**" because they can match Vite module URLs such as:
  // /src/pages/Guests/GuestsPage.tsx  -> which would break the app with MIME type errors.

  const guestsApi = /\/api\/(v[\d.]+\/)?guests(\b|\/|\?|$)/i
  const transfersApi = /\/api\/(v[\d.]+\/)?transfers(\b|\/|\?|$)/i
  const notificationsMyApi = /\/api\/(v[\d.]+\/)?notifications\/my(\b|\/|\?|$)/i
  const notificationsStatsApi = /\/api\/(v[\d.]+\/)?notifications\/statistics(\b|\/|\?|$)/i
  const anyApi = /\/api\/.*/i

  await page.route(guestsApi, (route: Route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(guests),
    })
  })

  await page.route(transfersApi, (route: Route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(transfers),
    })
  })

  // Notifications endpoints used in the layout
  await page.route(notificationsMyApi, (route: Route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: [] }),
    })
  })

  await page.route(notificationsStatsApi, (route: Route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: {} }),
    })
  })

  // Generic fallback for other API calls to avoid proxy errors
  await page.route(anyApi, (route: Route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: [], totalCount: 0 }),
    })
  })
}

