import { QueryClient } from '@tanstack/react-query'
import { queryKeys } from './queryKeys'

/**
 * Cache management utilities for React Query
 */
export class CacheManager {
  constructor(private queryClient: QueryClient) {}

  /**
   * Invalidate all queries for a specific resource
   */
  invalidateResource(resource: keyof typeof queryKeys) {
    const resourceKeys = queryKeys[resource]
    if (resourceKeys && 'all' in resourceKeys) {
      this.queryClient.invalidateQueries({ queryKey: resourceKeys.all })
    }
  }

  /**
   * Invalidate all list queries for a specific resource
   */
  invalidateLists(resource: keyof typeof queryKeys) {
    const resourceKeys = queryKeys[resource]
    if (resourceKeys && 'lists' in resourceKeys) {
      this.queryClient.invalidateQueries({ queryKey: resourceKeys.lists() })
    }
  }

  /**
   * Invalidate a specific detail query
   */
  invalidateDetail(resource: keyof typeof queryKeys, id: number) {
    const resourceKeys = queryKeys[resource]
    if (resourceKeys && 'details' in resourceKeys && 'detail' in resourceKeys) {
      this.queryClient.invalidateQueries({ queryKey: resourceKeys.detail(id) })
    }
  }

  /**
   * Remove a specific query from cache
   */
  removeQuery(queryKey: readonly unknown[]) {
    this.queryClient.removeQueries({ queryKey })
  }

  /**
   * Clear all queries
   */
  clearAll() {
    this.queryClient.clear()
  }

  /**
   * Prefetch a query
   */
  async prefetchQuery<T>(
    queryKey: readonly unknown[],
    queryFn: () => Promise<T>
  ) {
    await this.queryClient.prefetchQuery({
      queryKey,
      queryFn,
    })
  }

  /**
   * Set query data directly (useful for optimistic updates)
   */
  setQueryData<T>(queryKey: readonly unknown[], data: T) {
    this.queryClient.setQueryData(queryKey, data)
  }

  /**
   * Get query data
   */
  getQueryData<T>(queryKey: readonly unknown[]): T | undefined {
    return this.queryClient.getQueryData<T>(queryKey)
  }

  /**
   * Refetch all active queries
   */
  refetchAll() {
    return this.queryClient.refetchQueries()
  }

  /**
   * Refetch queries for a specific resource
   */
  refetchResource(resource: keyof typeof queryKeys) {
    const resourceKeys = queryKeys[resource]
    if (resourceKeys && 'all' in resourceKeys) {
      return this.queryClient.refetchQueries({ queryKey: resourceKeys.all })
    }
  }
}

/**
 * Hook to get cache manager instance
 */
export const useCacheManager = (queryClient: QueryClient) => {
  return new CacheManager(queryClient)
}

