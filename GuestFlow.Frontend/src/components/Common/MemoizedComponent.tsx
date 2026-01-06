import { memo, ReactNode, ComponentType } from 'react'

/**
 * Higher-order component for memoizing components
 */
export const withMemo = <P extends object>(
  Component: ComponentType<P>,
  areEqual?: (prevProps: P, nextProps: P) => boolean
) => {
  return memo(Component, areEqual)
}

/**
 * Memoized wrapper component
 */
interface MemoizedComponentProps {
  children: ReactNode
}

export const MemoizedComponent = memo(({ children }: MemoizedComponentProps) => {
  return <>{children}</>
})

MemoizedComponent.displayName = 'MemoizedComponent'

export default MemoizedComponent

