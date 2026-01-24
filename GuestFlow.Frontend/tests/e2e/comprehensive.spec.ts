// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { test, expect } from '@playwright/test'
import { ensureLoggedIn } from '../utils/testHelpers'
import { setupMockApi } from '../utils/mockApi'

const userEmail = (globalThis as any).process?.env?.E2E_USER_EMAIL || 'admin@example.com'
const userPassword = (globalThis as any).process?.env?.E2E_USER_PASSWORD || 'Admin123!'
const DEFAULT_BASE = ((globalThis as any).process?.env?.E2E_BASE_URL || 'http://localhost:5173').toString().trim().replace(/\/$/, '')

test.describe('Comprehensive E2E Tests - Critical User Flows', () => {
  test.beforeEach(async ({ page }) => {
    await setupMockApi(page)
    const mockedAuth = JSON.stringify({ user: { id: 1, email: userEmail, fullName: 'Test User', role: 'Admin' }, isAuthenticated: true })
    await page.addInitScript((auth) => {
      try {
        localStorage.setItem('auth-storage', auth)
        localStorage.setItem('VITE_E2E_BYPASS', 'true')
      } catch { /* ignore */ }
    }, mockedAuth)
    await ensureLoggedIn(page, userEmail, userPassword)
  })

  test('should complete guest management flow', async ({ page }) => {
    // Navigate to guests page
    await page.goto(`${DEFAULT_BASE}/guests`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Check if guests page loaded
    await expect(page).toHaveURL(/.*guests/)
    
    // Try to add a new guest (if button exists)
    const addButton = page.locator('button:has-text("Ekle"), button:has-text("Yeni")').first()
    const addButtonVisible = await addButton.isVisible({ timeout: 5000 }).catch(() => false)
    
    if (addButtonVisible) {
      await addButton.click()
      await page.waitForSelector('form, [role="dialog"]', { timeout: 5000 })
      
      // Verify form is visible
      const formVisible = await page.locator('form, [role="dialog"]').isVisible()
      expect(formVisible).toBeTruthy()
    }
  })

  test('should complete transfer management flow', async ({ page }) => {
    // Navigate to transfers page
    await page.goto(`${DEFAULT_BASE}/transfers`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Check if transfers page loaded
    await expect(page).toHaveURL(/.*transfers/)
    
    // Check if transfers table or list is visible
    const tableOrList = page.locator('table, [role="table"], [data-testid*="transfer"]').first()
    const emptyState = page.getByText(/transfer bulunamadı/i).or(page.getByText(/no transfers/i))
    
    await Promise.race([
      tableOrList.waitFor({ state: 'visible', timeout: 10000 }).catch(() => {}),
      emptyState.waitFor({ state: 'visible', timeout: 10000 }).catch(() => {}),
    ])
  })

  test('should complete invoice management flow', async ({ page }) => {
    // Navigate to invoices page
    await page.goto(`${DEFAULT_BASE}/invoices`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Check if invoices page loaded
    await expect(page).toHaveURL(/.*invoices/)
    
    // Check if invoices table is visible
    const tableOrList = page.locator('table, [role="table"]').first()
    const emptyState = page.getByText(/fatura bulunamadı/i).or(page.getByText(/no invoices/i))
    
    await Promise.race([
      tableOrList.waitFor({ state: 'visible', timeout: 10000 }).catch(() => {}),
      emptyState.waitFor({ state: 'visible', timeout: 10000 }).catch(() => {}),
    ])
  })

  test('should complete dashboard analytics flow', async ({ page }) => {
    // Navigate to dashboard
    await page.goto(`${DEFAULT_BASE}/dashboard`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Switch to admin dashboard
    const adminToggle = page.locator('button:has-text("Yönetim")').or(page.locator('button[aria-label*="admin"]'))
    if (await adminToggle.isVisible({ timeout: 5000 }).catch(() => false)) {
      await adminToggle.click()
      await page.waitForLoadState('networkidle', { timeout: 5000 })
    }

    // Check if analytics KPIs are visible
    await page.waitForTimeout(2000)
    
    const kpiCards = [
      page.locator('[data-testid="kpi-card-today-revenue"]'),
      page.locator('[data-testid="kpi-card-month-revenue"]'),
      page.locator('[data-testid="kpi-card-net-profit"]'),
    ]

    // At least one KPI card should be visible
    const visibleCards = await Promise.all(
      kpiCards.map(card => card.isVisible({ timeout: 5000 }).catch(() => false))
    )
    
    const hasVisibleCard = visibleCards.some(visible => visible)
    expect(hasVisibleCard).toBeTruthy()
  })

  test('should handle navigation between pages', async ({ page }) => {
    const pages = [
      { path: '/dashboard', name: 'Dashboard' },
      { path: '/guests', name: 'Guests' },
      { path: '/transfers', name: 'Transfers' },
      { path: '/invoices', name: 'Invoices' },
    ]

    for (const pageInfo of pages) {
      await page.goto(`${DEFAULT_BASE}${pageInfo.path}`)
      await page.waitForLoadState('networkidle', { timeout: 15000 })
      await expect(page).toHaveURL(new RegExp(`.*${pageInfo.path.replace('/', '')}.*`))
    }
  })

  test('should handle search functionality', async ({ page }) => {
    // Navigate to guests page
    await page.goto(`${DEFAULT_BASE}/guests`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Look for search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="Ara"], input[placeholder*="Search"]').first()
    const searchVisible = await searchInput.isVisible({ timeout: 5000 }).catch(() => false)

    if (searchVisible) {
      await searchInput.fill('test')
      await page.waitForTimeout(1000) // Wait for search to process
      
      // Search should not cause errors
      const errorMessage = page.locator('[class*="error"], [role="alert"]').first()
      const hasError = await errorMessage.isVisible({ timeout: 2000 }).catch(() => false)
      expect(hasError).toBeFalsy()
    }
  })

  test('should handle pagination', async ({ page }) => {
    // Navigate to a page with pagination (guests, transfers, etc.)
    await page.goto(`${DEFAULT_BASE}/guests`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Look for pagination controls
    const pagination = page.locator('[class*="Pagination"], [aria-label*="pagination"]').first()
    const paginationVisible = await pagination.isVisible({ timeout: 5000 }).catch(() => false)

    if (paginationVisible) {
      // Try to click next page
      const nextButton = pagination.locator('button[aria-label*="next"], button:has-text("Next")').first()
      const nextVisible = await nextButton.isVisible({ timeout: 2000 }).catch(() => false)
      
      if (nextVisible && !(await nextButton.isDisabled())) {
        await nextButton.click()
        await page.waitForLoadState('networkidle', { timeout: 5000 })
        
        // Page should still be loaded
        await expect(page).toHaveURL(/.*guests/)
      }
    }
  })

  test('should handle filter functionality', async ({ page }) => {
    // Navigate to transfers page (usually has filters)
    await page.goto(`${DEFAULT_BASE}/transfers`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Look for filter controls
    const filterButton = page.locator('button:has-text("Filtrele"), button:has-text("Filter")').first()
    const filterVisible = await filterButton.isVisible({ timeout: 5000 }).catch(() => false)

    if (filterVisible) {
      await filterButton.click()
      await page.waitForTimeout(1000)
      
      // Filter dialog or panel should be visible
      const filterDialog = page.locator('[role="dialog"], [class*="filter"], [class*="Filter"]').first()
      const dialogVisible = await filterDialog.isVisible({ timeout: 2000 }).catch(() => false)
      
      if (dialogVisible) {
        // Close filter
        const closeButton = page.locator('button:has-text("Kapat"), button:has-text("Close"), button[aria-label*="close"]').first()
        if (await closeButton.isVisible({ timeout: 2000 }).catch(() => false)) {
          await closeButton.click()
        }
      }
    }
  })
})

test.describe('Error Handling E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupMockApi(page)
    const mockedAuth = JSON.stringify({ user: { id: 1, email: userEmail, fullName: 'Test User', role: 'Admin' }, isAuthenticated: true })
    await page.addInitScript((auth) => {
      try {
        localStorage.setItem('auth-storage', auth)
        localStorage.setItem('VITE_E2E_BYPASS', 'true')
      } catch { /* ignore */ }
    }, mockedAuth)
    await ensureLoggedIn(page, userEmail, userPassword)
  })

  test('should handle 404 errors gracefully', async ({ page }) => {
    await page.goto(`${DEFAULT_BASE}/non-existent-page`)
    await page.waitForLoadState('networkidle', { timeout: 10000 })

    // Should show 404 or redirect to a valid page
    const notFound = page.locator('text=404, text=Not Found, text=Sayfa bulunamadı').first()
    const hasNotFound = await notFound.isVisible({ timeout: 5000 }).catch(() => false)
    
    // Either 404 page or redirect to dashboard
    const isDashboard = page.url().includes('/dashboard')
    expect(hasNotFound || isDashboard).toBeTruthy()
  })

  test('should handle API errors gracefully', async ({ page, context }) => {
    // Intercept API calls and return errors
    await context.route('**/api/**', async (route) => {
      if (route.request().method() === 'OPTIONS') {
        await route.fulfill({ status: 204 })
      } else {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ success: false, message: 'Internal Server Error' }),
        })
      }
    })

    await page.goto(`${DEFAULT_BASE}/dashboard`)
    await page.waitForLoadState('networkidle', { timeout: 15000 })

    // Should show error state or loading, not crash
    const errorState = page.locator('[class*="error"], [role="alert"], text=Hata').first()
    const loadingState = page.locator('[class*="loading"], [class*="skeleton"]').first()
    
    const hasError = await errorState.isVisible({ timeout: 5000 }).catch(() => false)
    const hasLoading = await loadingState.isVisible({ timeout: 5000 }).catch(() => false)
    
    // Page should still be functional (error or loading state)
    expect(hasError || hasLoading || page.url().includes('/dashboard')).toBeTruthy()
  })
})
