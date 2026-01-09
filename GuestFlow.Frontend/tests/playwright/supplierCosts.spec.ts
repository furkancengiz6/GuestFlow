import { test, expect } from '@playwright/test';

/// <reference types="node" />
/// <reference types="@playwright/test" />

const DEFAULT_BASE = (process.env.E2E_BASE_URL || 'http://localhost:5175').toString().trim().replace(/\/$/, '');

test.describe('Supplier Costs page', () => {
  test('loads and shows UI elements', async ({ page, baseURL }) => {
    await page.goto(`${baseURL || DEFAULT_BASE}/suppliers/costs`);
    await expect(page.getByRole('heading', { name: 'Supplier Costs' })).toBeVisible({ timeout: 15000 });
    await expect(page.getByRole('button', { name: 'Sync Supplier Costs' })).toBeVisible({ timeout: 15000 });
    await expect(page.getByRole('button', { name: 'Create Supplier Cost' })).toBeVisible({ timeout: 15000 });
  });
});

