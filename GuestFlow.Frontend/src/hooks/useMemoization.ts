import { useMemo } from 'react'

/**
 * Hook for memoizing expensive array operations
 */
export const useMemoizedArray = <T>(
  array: T[],
  transformFn?: (arr: T[]) => T[]
): T[] => {
  return useMemo(() => {
    return transformFn ? transformFn(array) : array
  }, [array, transformFn])
}

/**
 * Hook for memoizing filtered data
 */
export const useMemoizedFilter = <T>(
  items: T[],
  filterFn: (item: T) => boolean
): T[] => {
  return useMemo(() => {
    return items.filter(filterFn)
  }, [items, filterFn])
}

/**
 * Hook for memoizing sorted data
 */
export const useMemoizedSort = <T>(
  items: T[],
  sortFn?: (a: T, b: T) => number
): T[] => {
  return useMemo(() => {
    if (!sortFn) return items
    return [...items].sort(sortFn)
  }, [items, sortFn])
}

/**
 * Hook for memoizing paginated data
 */
export const useMemoizedPagination = <T>(
  items: T[],
  page: number,
  pageSize: number
) => {
  return useMemo(() => {
    const startIndex = (page - 1) * pageSize
    const endIndex = startIndex + pageSize
    return {
      data: items.slice(startIndex, endIndex),
      totalPages: Math.ceil(items.length / pageSize),
      totalCount: items.length,
    }
  }, [items, page, pageSize])
}

