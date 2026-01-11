import { memo, ReactNode } from 'react'

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

