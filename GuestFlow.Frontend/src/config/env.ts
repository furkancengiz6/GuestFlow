/**
 * Environment configuration
 * All environment variables should be accessed through this file
 *
 * PRODUCTION SAFETY:
 * - Required variables MUST be set in production (no defaults)
 * - localhost URLs are NEVER used in production builds
 * - Validation throws errors in production for missing required vars
 */

interface EnvConfig {
  apiBaseUrl: string
  env: string
  appName: string
  appVersion: string
  enableAnalytics: boolean
  enableErrorTracking: boolean
  apiTimeout: number
  signalRUrl: string
  maxFileSize: number
  defaultPageSize: number
  sessionTimeout: number
  isDev: boolean
  googleMapsApiKey?: string
  stripePublishableKey: string
}

/**
 * Get required environment variable (throws in production if missing)
 */
const getEnv = (key: string, defaultValue: string = ''): string => {
  const value = process.env[key]
  if (!value) {
    const isProduction = process.env.PROD || process.env.NODE_ENV === 'production'
    if (isProduction && defaultValue.includes('localhost')) {
      throw new Error(`Cannot use localhost default '${defaultValue}' for '${key}' in production. Set proper production URL.`)
    }
  }
  return value || defaultValue
}

/**
 * Get boolean environment variable
 */
const getEnvBool = (key: string, defaultValue: boolean = false): boolean => {
  const value = process.env[key]
  if (value === undefined) return defaultValue
  return value === 'true' || value === '1'
}

/**
 * Get number environment variable
 */
const getEnvNumber = (key: string, defaultValue: number = 0): number => {
  const value = process.env[key]
  if (value === undefined) return defaultValue
  const parsed = Number(value)
  return isNaN(parsed) ? defaultValue : parsed
}

/**
 * Environment configuration object
 */
export const env: EnvConfig = {
  apiBaseUrl: getEnv('VITE_API_BASE_URL', 'http://localhost:5146/api/v1.0'),
  googleMapsApiKey: getEnv('VITE_GOOGLE_MAPS_API_KEY', ''),
  stripePublishableKey: getEnv('VITE_STRIPE_PUBLISHABLE_KEY', 'pk_test_sample'),
  env: getEnv('VITE_ENV', 'development'),
  appName: getEnv('VITE_APP_NAME', 'GuestFlow'),
  appVersion: getEnv('VITE_APP_VERSION', '1.0.0'),
  enableAnalytics: getEnvBool('VITE_ENABLE_ANALYTICS', false),
  enableErrorTracking: getEnvBool('VITE_ENABLE_ERROR_TRACKING', false),
  apiTimeout: getEnvNumber('VITE_API_TIMEOUT', 30000),
  signalRUrl: getEnv('VITE_SIGNALR_URL', '/hubs/notifications'),
  maxFileSize: getEnvNumber('VITE_MAX_FILE_SIZE', 10485760), // 10MB
  defaultPageSize: getEnvNumber('VITE_DEFAULT_PAGE_SIZE', 10),
  sessionTimeout: getEnvNumber('VITE_SESSION_TIMEOUT', 1800000), // 30 minutes
  isDev: getEnv('VITE_ENV', 'development') === 'development' || getEnvBool('VITE_DEV_MODE', false),
}

/**
 * Validate required environment variables
 * Throws errors in production, warns in development
 */
export const validateEnv = (): void => {
  const required = ['VITE_API_BASE_URL']
  const missing: string[] = []
  const isProduction = process.env.PROD || process.env.NODE_ENV === 'production'

  required.forEach((key) => {
    if (!process.env[key]) {
      missing.push(key)
    }
  })

  if (missing.length > 0) {
    const message = `Missing required environment variables: ${missing.join(', ')}`
    if (isProduction) {
      throw new Error(`${message}. Production builds require all required environment variables to be set.`)
    } else {
      console.warn(message)
      console.warn('Using default values. This may cause issues in production.')
    }
  }

  // Validate production URLs don't contain localhost
  if (isProduction && env.apiBaseUrl.includes('localhost')) {
    throw new Error('Production builds cannot use localhost URLs. Set VITE_API_BASE_URL to a proper production URL.')
  }
}

// Validate on import
validateEnv()

export default env

