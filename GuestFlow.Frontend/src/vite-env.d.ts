/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string
  readonly VITE_ENV?: string
  readonly VITE_APP_NAME?: string
  readonly VITE_APP_VERSION?: string
  readonly VITE_ENABLE_ANALYTICS?: string
  readonly VITE_ENABLE_ERROR_TRACKING?: string
  readonly VITE_API_TIMEOUT?: string
  readonly VITE_SIGNALR_URL?: string
  readonly VITE_MAX_FILE_SIZE?: string
  readonly VITE_DEFAULT_PAGE_SIZE?: string
  readonly VITE_SESSION_TIMEOUT?: string
  readonly VITE_DEV_MODE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

