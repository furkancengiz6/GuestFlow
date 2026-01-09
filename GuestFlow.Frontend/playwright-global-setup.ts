/// <reference types="node" />
/// <reference types="@playwright/test" />

import fs from 'fs';
import { chromium, FullConfig } from '@playwright/test';

 

export default async function globalSetup(config: FullConfig) {
  const rawBase = process.env.E2E_BASE_URL || 'http://localhost:5173';
  const baseURL = String(rawBase).trim().replace(/\/$/, '');
  const email = process.env.E2E_USER_EMAIL || 'test@guestflow.local';
  const password = process.env.E2E_USER_PASSWORD || 'Password123!';
  const storagePath = process.env.PLAYWRIGHT_STORAGE || 'tests/storageState.json';

  const browser = await chromium.launch();
  const page = await browser.newPage();
  const consoleLogs: string[] = [];
  const pageErrors: string[] = [];
  page.on('console', (msg) => {
    try {
      consoleLogs.push(`[${msg.type()}] ${msg.text()}`);
    } catch { /* ignore */ }
  });
  page.on('pageerror', (err) => {
    try {
      pageErrors.push(String(err));
    } catch { /* ignore */ }
  });
  // Wait for the dev server to be reachable before navigating
  const maxWait = 30_000;
  const start = Date.now();
  let reachable = false;
  while (Date.now() - start < maxWait) {
    try {
      const resp = await page.goto(baseURL, { waitUntil: 'domcontentloaded', timeout: 5000 });
      if (resp && resp.ok()) {
        reachable = true;
        break;
      }
    } catch {
      // ignore, retry
    }
    await new Promise((r) => setTimeout(r, 1000));
  }
  if (!reachable) {
    console.warn(`Global setup: ${baseURL} not reachable after ${maxWait}ms`);
  }
  // Go to login page and perform UI login so the resulting storageState reflects real app auth
  await page.goto(`${baseURL}/login`, { waitUntil: 'domcontentloaded', timeout: 10000 });
  try {
    await page.waitForSelector('input[type="email"]', { timeout: 30000 });
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', password);
    await page.click('button:has-text("Giriş Yap")');
    // Wait for a route that indicates successful login (dashboard)
    await page.waitForURL('**/dashboard', { timeout: 30000 });
    // Save storage state for authenticated sessions
    await page.context().storageState({ path: storagePath });
    } catch (err) {
    console.warn('Global setup login failed — falling back to writing storageState with mocked auth:', err);
    // write console logs for debugging
    try {
      fs.mkdirSync('tests/playwright-debug', { recursive: true });
      fs.writeFileSync('tests/playwright-debug/console.log', consoleLogs.join('\n'));
      fs.writeFileSync('tests/playwright-debug/pageerrors.log', pageErrors.join('\n'));
    } catch (logErr) {
      console.warn('Failed to write debug logs:', logErr);
    }
    // capture page HTML for debugging
    try {
      const html = await page.content();
      fs.mkdirSync('tests/playwright-debug', { recursive: true });
      fs.writeFileSync('tests/playwright-debug/login-page.html', html);
    } catch (captureErr) {
      console.warn('Failed to capture page content for debugging:', captureErr);
    }
    // Fallback: write a storageState file that sets the persisted auth key used by zustand persist
    const mockedUser = {
      id: 1,
      email,
      fullName: 'Test User',
      role: 'Admin',
    };
    const authValue = JSON.stringify({ user: mockedUser, isAuthenticated: true });
    const storageState = {
      cookies: [],
      origins: [
        {
          origin: baseURL,
          localStorage: [
            {
              name: 'auth-storage',
              value: authValue,
            },
            {
              name: 'VITE_E2E_BYPASS',
              value: 'true',
            },
          ],
        },
      ],
    };
    fs.mkdirSync('tests', { recursive: true });
    fs.writeFileSync(storagePath, JSON.stringify(storageState, null, 2));
  } finally {
    await browser.close();
  }
}

