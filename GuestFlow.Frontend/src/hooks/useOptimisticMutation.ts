import { useMutation, useQueryClient, UseMutationOptions, UseMutationResult } from '@tanstack/react-query'
import { useCallback } from 'react'

interface OptimisticUpdateConfig<TData, TVariables, TContext> {
  // Query key to update
  queryKey: readonly unknown[]
  
  // Function to update the cache optimistically
  optimisticUpdate: (variables: TVariables) => (oldData: TData | undefined) => TData | undefined
  
  // Function to rollback on error
  onError?: (error: unknown, variables: TVariables, context: TContext | undefined) => void
  
  // Function to finalize on success
  onSuccess?: (data: TData, variables: TVariables, context: TContext | undefined) => void
}

/**
 * Hook for optimistic mutations
 * Updates the cache immediately before the mutation completes
 */
export function useOptimisticMutation<
  TData = unknown,
  TError = unknown,
  TVariables = void,
  TContext = unknown
>(
  mutationFn: (variables: TVariables) => Promise<TData>,
  config: OptimisticUpdateConfig<TData, TVariables, TContext>,
  options?: Omit<UseMutationOptions<TData, TError, TVariables, TContext>, 'mutationFn' | 'onMutate' | 'onError' | 'onSuccess'>
): UseMutationResult<TData, TError, TVariables, TContext> {
  const queryClient = useQueryClient()

  const onMutate = useCallback(
    async (variables: TVariables) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: config.queryKey })

      // Snapshot the previous value
      const previousData = queryClient.getQueryData<TData>(config.queryKey)

      // Optimistically update the cache
      queryClient.setQueryData<TData>(
        config.queryKey,
        config.optimisticUpdate(variables)
      )

      // Return context with previous data for rollback
      return { previousData } as TContext
    },
    [queryClient, config]
  )

  const onError = useCallback(
    (error: TError, variables: TVariables, context: TContext | undefined) => {
      // Rollback on error
      if (context && typeof context === 'object' && 'previousData' in context) {
        queryClient.setQueryData(config.queryKey, (context as { previousData: TData }).previousData)
      }

      // Call custom error handler
      config.onError?.(error, variables, context)
    },
    [queryClient, config]
  )

  const onSuccess = useCallback(
    (data: TData, variables: TVariables, context: TContext | undefined) => {
      // Invalidate to refetch with server data
      queryClient.invalidateQueries({ queryKey: config.queryKey })

      // Call custom success handler
      config.onSuccess?.(data, variables, context)
    },
    [queryClient, config]
  )

  return useMutation<TData, TError, TVariables, TContext>({
    mutationFn,
    onMutate,
    onError,
    onSuccess,
    ...options,
  })
}

