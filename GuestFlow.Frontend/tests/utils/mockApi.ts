import type { Page, Route } from '@playwright/test'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

export async function setupMockApi(
  page: Page,
  options: { includeGenericFallback?: boolean } = {}
) {
  const includeGenericFallback = options.includeGenericFallback ?? true
  const allowOrigin = (process.env.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')
  const corsHeaders = {
    // NOTE: axios uses withCredentials=true; so we must NOT use "*" for allow-origin.
    'access-control-allow-origin': allowOrigin,
    'access-control-allow-credentials': 'true',
    'access-control-allow-methods': 'GET,POST,PUT,DELETE,PATCH,OPTIONS',
    'access-control-allow-headers': 'authorization,content-type,x-requested-with',
  } as const
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
      console.log(`[page.console] ${msg.type()} ${location} ${args}`)
    } catch {
      // ignore
    }
  })
  page.on('pageerror', (err) => {
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
  // Analytics routes - MUST be before generic anyApi fallback
  const analyticsKpisApi = /\/api\/(v[\d.]+\/)?analytics\/kpis\/realtime(\b|\/|\?|$)/i
  const analyticsTrendApi = /\/api\/(v[\d.]+\/)?analytics\/revenue\/trend(\b|\/|\?|$)/i
  const dashboardApi = /\/api\/(v[\d.]+\/)?dashboard\/(quick-stats|overview|recent-activities|revenue-chart|upcoming-bookings|guest-statistics|unpaid-services|upcoming-services)(\b|\/|\?|$)/i
  const anyApi = /\/api\/.*/i

  await page.route(guestsApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      headers: corsHeaders,
      body: JSON.stringify(guests),
    })
  })

  await page.route(transfersApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      headers: corsHeaders,
      body: JSON.stringify(transfers),
    })
  })

  // Notifications endpoints used in the layout
  await page.route(notificationsMyApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      headers: corsHeaders,
      body: JSON.stringify({ data: [] }),
    })
  })

  await page.route(notificationsStatsApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      headers: corsHeaders,
      body: JSON.stringify({ data: {} }),
    })
  })

  // Analytics KPI mock
  await page.route(analyticsKpisApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      headers: corsHeaders,
      body: JSON.stringify({
        success: true,
        data: {
          todayRevenue: 15000.50,
          thisMonthRevenue: 450000.75,
          thisMonthNetProfit: 135000.25,
          averageRevenuePerService: 2500.00,
          todayServiceCount: 6,
          thisMonthServiceCount: 180,
          mostProfitableServices: [
            {
              serviceType: 'Transfer',
              totalRevenue: 200000,
              totalCost: 120000,
              netProfit: 80000,
              profitMargin: 40.0,
              serviceCount: 100,
            },
            {
              serviceType: 'CityTour',
              totalRevenue: 150000,
              totalCost: 90000,
              netProfit: 60000,
              profitMargin: 40.0,
              serviceCount: 50,
            },
          ],
          revenueGrowthRate: 15.5,
          profitMargin: 30.0,
        },
      }),
    })
  })

  // Analytics Revenue Trend mock
  await page.route(analyticsTrendApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    const url = new URL(route.request().url())
    const period = url.searchParams.get('period') || 'daily'
    
    // Generate mock data points based on period
    const dataPoints = []
    const count = period === 'daily' ? 30 : period === 'weekly' ? 12 : 12
    
    for (let i = 0; i < count; i++) {
      dataPoints.push({
        label: period === 'daily' ? `${i + 1}.01` : period === 'weekly' ? `Week ${i + 1}` : `Month ${i + 1}`,
        date: new Date(2025, 0, i + 1).toISOString(),
        revenue: 10000 + Math.random() * 5000,
        cost: 6000 + Math.random() * 3000,
        netProfit: 4000 + Math.random() * 2000,
        serviceCount: 5 + Math.floor(Math.random() * 10),
      })
    }
    
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      headers: corsHeaders,
      body: JSON.stringify({
        success: true,
        data: {
          period,
          dataPoints,
          totalRevenue: dataPoints.reduce((sum, p) => sum + p.revenue, 0),
          averageRevenue: dataPoints.reduce((sum, p) => sum + p.revenue, 0) / dataPoints.length,
          growthRate: 12.5,
        },
      }),
    })
  })

  // Dashboard API mock (for existing dashboard functionality)
  await page.route(dashboardApi, (route: Route) => {
    if (route.request().method().toUpperCase() === 'OPTIONS') {
      return route.fulfill({ status: 204, headers: corsHeaders })
    }
    const url = route.request().url()
    
    // Mock different dashboard endpoints
    if (url.includes('quick-stats')) {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        headers: corsHeaders,
        body: JSON.stringify({
          success: true,
          data: {
            totalGuests: 100,
            totalTransfers: 50,
            totalCityTours: 30,
            totalYachtTours: 20,
            totalInvoices: 75,
          },
        }),
      })
    } else {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        headers: corsHeaders,
        body: JSON.stringify({ success: true, data: {} }),
      })
    }
  })

  // Generic fallback for other API calls to avoid proxy errors
  // IMPORTANT: This must be LAST to avoid overriding specific routes above
  if (includeGenericFallback) {
    await page.route(anyApi, (route: Route) => {
      if (route.request().method().toUpperCase() === 'OPTIONS') {
        return route.fulfill({ status: 204, headers: corsHeaders })
      }
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        headers: corsHeaders,
        body: JSON.stringify({ data: [], totalCount: 0 }),
      })
    })
  }
}

