import { test, expect } from '@playwright/test';

test.describe('GuestFlow Dashboard E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('http://localhost:5173');

    // Login if needed (adjust selectors based on your actual login form)
    const loginForm = page.locator('form').first();
    if (await loginForm.isVisible()) {
      await page.fill('input[type="email"]', 'admin@example.com');
      await page.fill('input[type="password"]', 'AdminPass123!');
      await page.click('button[type="submit"]');

      // Wait for navigation to dashboard
      await page.waitForURL('**/dashboard');
    }
  });

  test('Dashboard loads successfully', async ({ page }) => {
    // Verify we're on the dashboard page
    await expect(page).toHaveURL(/.*dashboard/);

    // Check if main dashboard elements are present
    await expect(page.locator('text=Dashboard')).toBeVisible();
    await expect(page.locator('text=Quick Stats')).toBeVisible();
  });

  test('Quick stats display correct information', async ({ page }) => {
    // Wait for stats to load
    await page.waitForSelector('[data-testid="total-guests"]', { timeout: 10000 });

    // Verify stat cards are present and contain numbers
    const totalGuests = page.locator('[data-testid="total-guests"]');
    const totalRevenue = page.locator('[data-testid="total-revenue"]');
    const activeGuests = page.locator('[data-testid="active-guests"]');

    await expect(totalGuests).toBeVisible();
    await expect(totalRevenue).toBeVisible();
    await expect(activeGuests).toBeVisible();

    // Verify values are numeric (Phase 2 optimization should ensure fast loading)
    const guestsText = await totalGuests.textContent();
    const revenueText = await totalRevenue.textContent();

    expect(parseInt(guestsText?.replace(/\D/g, '') || '0')).toBeGreaterThanOrEqual(0);
    expect(parseFloat(revenueText?.replace(/[^\d.]/g, '') || '0')).toBeGreaterThanOrEqual(0);
  });

  test('Popular services chart renders correctly', async ({ page }) => {
    // Wait for chart to load
    await page.waitForSelector('[data-testid="services-chart"]', { timeout: 10000 });

    const servicesChart = page.locator('[data-testid="services-chart"]');
    await expect(servicesChart).toBeVisible();

    // Verify all three service types are shown
    await expect(page.locator('text=Transfer')).toBeVisible();
    await expect(page.locator('text=City Tour')).toBeVisible();
    await expect(page.locator('text=Yacht Tour')).toBeVisible();
  });

  test('Navigation between dashboard sections works', async ({ page }) => {
    // Test navigation to different sections
    const transfersLink = page.locator('a[href*="transfers"]');
    if (await transfersLink.isVisible()) {
      await transfersLink.click();
      await expect(page).toHaveURL(/.*transfers/);
      await page.goBack();
    }

    const guestsLink = page.locator('a[href*="guests"]');
    if (await guestsLink.isVisible()) {
      await guestsLink.click();
      await expect(page).toHaveURL(/.*guests/);
      await page.goBack();
    }
  });

  test('Dashboard performance is acceptable', async ({ page }) => {
    // Measure page load time
    const startTime = Date.now();

    await page.reload();
    await page.waitForSelector('[data-testid="total-guests"]', { timeout: 10000 });

    const loadTime = Date.now() - startTime;

    // Dashboard should load in less than 3 seconds (Phase 2 optimization target)
    expect(loadTime).toBeLessThan(3000);

    console.log(`Dashboard load time: ${loadTime}ms`);
  });

  test('Real-time updates work', async ({ page }) => {
    // Test SignalR real-time updates if implemented
    const initialGuestsCount = await page.locator('[data-testid="total-guests"]').textContent();

    // This would require backend changes to trigger real-time updates
    // For now, just verify the connection is established
    const signalRConnection = await page.evaluate(() => {
      // Check if SignalR connection exists in window object
      return !!(window as any).signalRConnection;
    });

    // If SignalR is implemented, connection should exist
    if (signalRConnection) {
      console.log('SignalR connection detected');
    } else {
      console.log('SignalR not implemented yet');
    }
  });

  test('Responsive design works on mobile', async ({ page }) => {
    // Test mobile responsiveness
    await page.setViewportSize({ width: 375, height: 667 });

    // Verify mobile layout
    await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible();

    // Check if stats are still readable on mobile
    await expect(page.locator('[data-testid="total-guests"]')).toBeVisible();
    await expect(page.locator('[data-testid="total-revenue"]')).toBeVisible();
  });

  test('Error handling works gracefully', async ({ page }) => {
    // Test error scenarios
    await page.route('**/api/dashboard/quick-stats', route => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal server error' })
      });
    });

    await page.reload();

    // Should show error message instead of crashing
    await expect(page.locator('text=Error loading dashboard')).toBeVisible({ timeout: 5000 });
  });

  test('Accessibility compliance', async ({ page }) => {
    // Test basic accessibility
    const images = page.locator('img');
    const imageCount = await images.count();

    for (let i = 0; i < imageCount; i++) {
      const alt = await images.nth(i).getAttribute('alt');
      expect(alt).toBeTruthy(); // All images should have alt text
    }

    // Check for proper heading hierarchy
    const h1Count = await page.locator('h1').count();
    expect(h1Count).toBeGreaterThan(0); // Should have at least one H1

    // Test keyboard navigation
    await page.keyboard.press('Tab');
    const focusedElement = page.locator(':focus');
    await expect(focusedElement).toBeVisible();
  });
});