/// <reference types="node" />
/// <reference types="@playwright/test" />

import { defineConfig, devices } from '@playwright/test';

// Fix for Node.js globals in TypeScript (process)
import type { Config as PlaywrightTestConfig } from '@playwright/test';

const normalizedBase = (typeof process !== 'undefined' && process.env.E2E_BASE_URL
  ? process.env.E2E_BASE_URL
  : 'http://localhost:5173'
).toString().trim().replace(/\/$/, '');

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
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: normalizedBase,
    reuseExistingServer: true,
    timeout: 120 * 1000,
  },
  globalSetup: './playwright-global-setup',
 
})

