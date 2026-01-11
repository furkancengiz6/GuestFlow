import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClientProvider, QueryErrorResetBoundary } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import CssBaseline from '@mui/material/CssBaseline'
import App from './App'
import { ThemeProviderWithToggle } from './theme/ThemeProviderWithToggle'
import NotificationProvider from './components/Common/NotificationProvider'
import AppErrorBoundary from './components/Common/AppErrorBoundary'
import QueryErrorFallback from './components/Common/QueryErrorFallback'
import GlobalLoadingIndicator from './components/Common/GlobalLoadingIndicator'
import { createQueryClient } from './config/queryClient'
import './i18n/config' // Initialize i18n
import './styles/print.css'

// React Query client oluştur (optimized configuration)
const queryClient = createQueryClient()

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <QueryErrorResetBoundary>
        {({ reset }) => (
          <AppErrorBoundary
            onReset={() => {
              reset()
              queryClient.resetQueries()
            }}
            fallback={(error, resetBoundary) => (
              <QueryErrorFallback
                error={error}
                onRetry={() => {
                  resetBoundary()
                  reset()
                  queryClient.resetQueries()
                }}
              />
            )}
          >
            <ThemeProviderWithToggle>
              <CssBaseline />
              <GlobalLoadingIndicator />
              <App />
              <NotificationProvider />
              <ReactQueryDevtools initialIsOpen={false} />
            </ThemeProviderWithToggle>
          </AppErrorBoundary>
        )}
      </QueryErrorResetBoundary>
    </QueryClientProvider>
  </React.StrictMode>,
)

