import { useMemo, useCallback, DependencyList } from 'react'

/**
 * Custom hook for memoization utilities
 */
export const useMemoization = () => {
  /**
   * Memoize a callback function
   */
  const memoizeCallback = useCallback(
    <T extends (...args: any[]) => any>(callback: T, deps: DependencyList): T => {
      // eslint-disable-next-line react-hooks/exhaustive-deps
      return useCallback(callback, deps) as T
    },
    []
  )

  /**
   * Memoize a computed value
   */
  const memoizeValue = useCallback(
    <T>(factory: () => T, deps: DependencyList): T => {
      // eslint-disable-next-line react-hooks/exhaustive-deps
      return useMemo(factory, deps)
    },
    []
  )

  return {
    memoizeCallback,
    memoizeValue,
  }
}

/**
 * Hook for memoizing expensive array operations
 */
export const useMemoizedArray = <T>(
  array: T[],
  transformFn?: (arr: T[]) => T[],
  deps: DependencyList = [array]
): T[] => {
  return useMemo(() => {
    return transformFn ? transformFn(array) : array
  }, deps)
}

/**
 * Hook for memoizing filtered data
 */
export const useMemoizedFilter = <T>(
  items: T[],
  filterFn: (item: T) => boolean,
  deps: DependencyList = [items]
): T[] => {
  return useMemo(() => {
    return items.filter(filterFn)
  }, deps)
}

/**
 * Hook for memoizing sorted data
 */
export const useMemoizedSort = <T>(
  items: T[],
  sortFn?: (a: T, b: T) => number,
  deps: DependencyList = [items]
): T[] => {
  return useMemo(() => {
    if (!sortFn) return items
    return [...items].sort(sortFn)
  }, deps)
}

/**
 * Hook for memoizing paginated data
 */
export const useMemoizedPagination = <T>(
  items: T[],
  page: number,
  pageSize: number,
  deps: DependencyList = [items, page, pageSize]
) => {
  return useMemo(() => {
    const startIndex = (page - 1) * pageSize
    const endIndex = startIndex + pageSize
    return {
      data: items.slice(startIndex, endIndex),
      totalPages: Math.ceil(items.length / pageSize),
      totalCount: items.length,
    }
  }, deps)
}

