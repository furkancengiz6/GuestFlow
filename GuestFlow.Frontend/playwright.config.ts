/// <reference types="node" />
/// <reference types="@playwright/test" />

import { defineConfig, devices } from '@playwright/test';

// Fix for Node.js globals in TypeScript (process)
import type { Config as PlaywrightTestConfig } from '@playwright/test';

const normalizedBase = (typeof process !== 'undefined' && process.env.E2E_BASE_URL
  ? process.env.E2E_BASE_URL
  : 'http://localhost:5173'
).toString().trim().replace(/\/$/, '');

const useWebServer =
  (process.env.E2E_USE_WEB_SERVER || '').toLowerCase() === 'true' ||
  (!(process.env.E2E_USE_WEB_SERVER) && normalizedBase.startsWith('http://localhost'));

export default defineConfig<PlaywrightTestConfig>({
  testDir: './tests',
  timeout: 30 * 1000,
  expect: {
    timeout: 5000,
  },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'playwright-report/results.json' }],
  ],
  // common settings applied to all projects (overridden later if needed)
  use: {
    trace: 'on-first-retry',
    headless: true,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    storageState: process.env.PLAYWRIGHT_STORAGE || 'tests/storageState.json',
    baseURL: normalizedBase,
  },
  // Default to Chromium-only for stability and speed.
  // Opt-in to full cross-browser runs with PLAYWRIGHT_ALL_BROWSERS=true.
  projects: (process.env.PLAYWRIGHT_ALL_BROWSERS || '').toLowerCase() === 'true'
    ? [
        { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
        { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
        { name: 'webkit', use: { ...devices['Desktop Safari'] } },
      ]
    : [
        { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
      ],
  webServer: useWebServer
    ? {
        command: 'npm run dev',
        url: normalizedBase,
        // Ensure Playwright starts a fresh dev server to avoid stale Vite optimize cache issues
        reuseExistingServer: true,
        // Allow longer startup time for dev server (clearing optimize deps can take longer)
        timeout: 180 * 1000,
      }
    : undefined,
  globalSetup: './playwright-global-setup',
 
})

