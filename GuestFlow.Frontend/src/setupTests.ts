// Test setup file - Jest configuration
// This file configures Jest for React Testing Library
import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'

// TypeScript types are handled by Jest automatically

// Cleanup after each test (guarded for environments where helper may be undefined)
if (typeof afterEach === 'function') {
  afterEach(() => {
    cleanup()
  })
}

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // deprecated
    removeListener: jest.fn(), // deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
})

// Mock IntersectionObserver
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  takeRecords() {
    return []
  }
  unobserve() {}
} as any

// Mock ResizeObserver
global.ResizeObserver = class ResizeObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  unobserve() {}
} as any

// Mock URL.createObjectURL for export tests
global.URL.createObjectURL = jest.fn(() => 'mock-url')
global.URL.revokeObjectURL = jest.fn()

// Mock axios for API calls
jest.mock('axios', () => ({
  __esModule: true,
  default: {
    create: jest.fn(() => ({
      interceptors: {
        request: { use: jest.fn(), eject: jest.fn() },
        response: { use: jest.fn(), eject: jest.fn() }
      },
      get: jest.fn(),
      post: jest.fn(),
      put: jest.fn(),
      patch: jest.fn(),
      delete: jest.fn(),
      defaults: { headers: { common: {} } }
    })),
    isAxiosError: jest.fn(() => false),
    AxiosError: class AxiosError extends Error {
      constructor(message: string, code?: string) {
        super(message)
        this.name = 'AxiosError'
        this.code = code
      }
      code?: string
      response?: any
      request?: any
    }
  }
}))

// Note: Store and library mocks are handled individually in test files
// to avoid path resolution issues in setupTests.ts

/**
 * CI strict console policy:
 * - Fail tests on unexpected console.error / console.warn
 * - Allowlist a few known noisy framework messages (kept minimal)
 *
 * Override locally with: JEST_STRICT_CONSOLE=false
 */
const STRICT_CONSOLE =
  typeof process !== 'undefined' &&
  process.env &&
  process.env.CI === 'true' &&
  process.env.JEST_STRICT_CONSOLE !== 'false'

const isAllowedConsoleMessage = (level: 'error' | 'warn', args: any[]): boolean => {
  const first = args?.[0]
  if (typeof first !== 'string') return false

  // React legacy warnings (shouldn't happen often, but keep tests stable for now)
  if (first.includes('Warning: ReactDOM.render')) return true
  if (first.includes('Warning: validateDOMNesting')) return true

  // React Router internal warning (known noisy in some setups)
  if (level === 'warn' && first.includes('React Router Future Flag Warning')) return true

  return false
}

const formatConsoleArgs = (args: any[]) =>
  args
    .map((a) => {
      if (typeof a === 'string') return a
      if (a instanceof Error) return `${a.name}: ${a.message}\n${a.stack || ''}`
      try {
        return JSON.stringify(a)
      } catch {
        return String(a)
      }
    })
    .join(' ')

if (STRICT_CONSOLE && typeof beforeEach === 'function' && typeof afterEach === 'function') {
  let errorSpy: jest.SpyInstance | null = null
  let warnSpy: jest.SpyInstance | null = null

  beforeEach(() => {
    errorSpy = jest.spyOn(console, 'error').mockImplementation((...args: any[]) => {
      if (isAllowedConsoleMessage('error', args)) return
      throw new Error(`Unexpected console.error in test: ${formatConsoleArgs(args)}`)
    })
    warnSpy = jest.spyOn(console, 'warn').mockImplementation((...args: any[]) => {
      if (isAllowedConsoleMessage('warn', args)) return
      throw new Error(`Unexpected console.warn in test: ${formatConsoleArgs(args)}`)
    })
  })

  afterEach(() => {
    errorSpy?.mockRestore()
    warnSpy?.mockRestore()
    errorSpy = null
    warnSpy = null
  })
}

// Provide `vi` global for tests authored with Vitest shorthands
(global as any).vi = (global as any).vi || (global as any).jest
