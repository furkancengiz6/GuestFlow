import { memo, ComponentType } from 'react'

/**
 * Higher-order component for memoizing components
 */
export const withMemo = <P extends object>(
  Component: ComponentType<P>,
  areEqual?: (prevProps: P, nextProps: P) => boolean
) => {
  return memo(Component, areEqual)
}

