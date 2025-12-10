import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import CssBaseline from '@mui/material/CssBaseline'
import App from './App'
import { ThemeProviderWithToggle } from './theme/useTheme'
import NotificationProvider from './components/Common/NotificationProvider'
import './styles/print.css'

// React Query client oluştur
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 dakika
    },
  },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProviderWithToggle>
        <CssBaseline />
        <App />
        <NotificationProvider />
        <ReactQueryDevtools initialIsOpen={false} />
      </ThemeProviderWithToggle>
    </QueryClientProvider>
  </React.StrictMode>,
)

