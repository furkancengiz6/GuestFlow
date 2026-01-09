#!/usr/bin/env node
'use strict'

const fs = require('fs')
const path = require('path')

const rawBase = process.env.E2E_BASE_URL || 'http://localhost:5173'
const baseURL = String(rawBase).trim().replace(/\/$/, '')
const email = process.env.E2E_USER_EMAIL || 'e2e@guestflow.test'
const password = process.env.E2E_USER_PASSWORD || 'Password123!'
const storagePath = process.env.PLAYWRIGHT_STORAGE || 'tests/storageState.json'

// The app expects a persisted auth key in localStorage named 'auth-storage' (used in global-setup)
const mockedUser = {
  id: 1,
  email,
  fullName: 'E2E Test User',
  role: 'Admin',
}

const authValue = JSON.stringify({ user: mockedUser, isAuthenticated: true })

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
}

const outDir = path.dirname(storagePath)
try {
  fs.mkdirSync(outDir, { recursive: true })
  fs.writeFileSync(storagePath, JSON.stringify(storageState, null, 2))
  console.log(`Wrote storageState to ${storagePath} with user ${email}`)
  process.exitCode = 0
} catch (err) {
  console.error('Failed to write storageState:', err)
  process.exitCode = 1
}

