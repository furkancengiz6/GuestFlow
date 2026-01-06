import { QueryClient, QueryCache, MutationCache } from '@tanstack/react-query'

/**
 * Custom retry function that retries based on error type
 */
const shouldRetry = (failureCount: number, error: unknown): boolean => {
  // Don't retry on 4xx errors (client errors)
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as { response?: { status?: number } }
    const status = axiosError.response?.status

    // Don't retry on client errors (4xx)
    if (status && status >= 400 && status < 500) {
      return false
    }
  }

  // Retry up to 3 times for network errors and server errors
  return failureCount < 3
}

/**
 * Custom retry delay function with exponential backoff
 */
const retryDelay = (attemptIndex: number): number => {
  // Exponential backoff: 1s, 2s, 4s
  return Math.min(1000 * 2 ** attemptIndex, 5000)
}

/**
 * Create and configure QueryClient with optimized settings
 */
export const createQueryClient = () => {
  const queryClient = new QueryClient({
    queryCache: new QueryCache({
      onError: (error) => {
        // Global error handler for queries
        if (import.meta.env.DEV) {
          console.error('Query Error:', error)
        }
      },
    }),
    mutationCache: new MutationCache({
      onError: (error) => {
        // Global error handler for mutations
        if (import.meta.env.DEV) {
          console.error('Mutation Error:', error)
        }
      },
    }),
    defaultOptions: {
      queries: {
        // Cache settings
        staleTime: 5 * 60 * 1000, // 5 minutes - data is fresh for 5 minutes
        gcTime: 10 * 60 * 1000, // 10 minutes - cache is kept for 10 minutes (formerly cacheTime)
        
        // Refetch settings
        refetchOnWindowFocus: false, // Don't refetch on window focus
        refetchOnMount: true, // Refetch when component mounts
        refetchOnReconnect: true, // Refetch when network reconnects
        
        // Retry settings
        retry: shouldRetry,
        retryDelay: retryDelay,
        
        // Error handling is done at component level
      },
      mutations: {
        // Retry settings for mutations
        retry: (failureCount, error) => {
          // Only retry on network errors, not on validation errors
          if (error && typeof error === 'object' && 'response' in error) {
            const axiosError = error as { response?: { status?: number } }
            const status = axiosError.response?.status
            
            // Don't retry on client errors
            if (status && status >= 400 && status < 500) {
              return false
            }
          }
          
          // Retry once on network/server errors
          return failureCount < 1
        },
        retryDelay: retryDelay,
        
        // Error handling is done at component level
      },
    },
  })

  return queryClient
}

