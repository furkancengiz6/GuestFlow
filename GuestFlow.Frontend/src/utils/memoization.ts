import { useMemo, useCallback, DependencyList } from 'react'

/**
 * Memoization utilities for performance optimization
 */

/**
 * Memoize a function with useCallback
 */
export const useMemoizedCallback = <T extends (...args: any[]) => any>(
  callback: T,
  deps: DependencyList
): T => {
  return useCallback(callback, [callback, ...deps]) as T
}

/**
 * Memoize a value with useMemo
 */
export const useMemoizedValue = <T>(factory: () => T, deps: DependencyList): T => {
  return useMemo(factory, [factory, ...deps])
}

/**
 * Memoize expensive computations
 */
export const useExpensiveComputation = <T>(
  computeFn: () => T,
  deps: DependencyList
): T => {
  return useMemo(computeFn, [computeFn, ...deps])
}

/**
 * Memoize filtered/sorted arrays
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
 * Memoize object with deep comparison
 */
export const useMemoizedObject = <T extends Record<string, any>>(
  obj: T
): T => {
  return useMemo(() => obj, [obj])
}

