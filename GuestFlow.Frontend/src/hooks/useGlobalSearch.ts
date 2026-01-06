import { useState, useCallback } from 'react'

/**
 * Hook for managing global search state
 */
export const useGlobalSearch = () => {
  const [open, setOpen] = useState(false)

  const openSearch = useCallback(() => {
    setOpen(true)
  }, [])

  const closeSearch = useCallback(() => {
    setOpen(false)
  }, [])

  const toggleSearch = useCallback(() => {
    setOpen((prev) => !prev)
  }, [])

  return {
    open,
    openSearch,
    closeSearch,
    toggleSearch,
  }
}

