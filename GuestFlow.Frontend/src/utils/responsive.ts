import { useMediaQuery, useTheme, Breakpoint } from '@mui/material'

/**
 * Hook to check if current screen size matches breakpoint
 */
export const useBreakpoint = (breakpoint: Breakpoint | number) => {
  const theme = useTheme()
  return useMediaQuery(theme.breakpoints.up(breakpoint))
}

/**
 * Hook to check if screen is mobile
 */
export const useIsMobile = () => {
  return useMediaQuery('(max-width:600px)')
}

/**
 * Hook to check if screen is tablet
 */
export const useIsTablet = () => {
  return useMediaQuery('(min-width:601px) and (max-width:960px)')
}

/**
 * Hook to check if screen is desktop
 */
export const useIsDesktop = () => {
  return useMediaQuery('(min-width:961px)')
}

/**
 * Responsive value helper
 * Returns different values based on screen size
 */
export const useResponsiveValue = <T,>(values: {
  mobile?: T
  tablet?: T
  desktop?: T
  default: T
}): T => {
  const isMobile = useIsMobile()
  const isTablet = useIsTablet()
  const isDesktop = useIsDesktop()

  if (isMobile && values.mobile !== undefined) return values.mobile
  if (isTablet && values.tablet !== undefined) return values.tablet
  if (isDesktop && values.desktop !== undefined) return values.desktop
  return values.default
}

/**
 * Get responsive spacing
 */
export const getResponsiveSpacing = (mobile: number, desktop: number) => {
  return { xs: mobile, md: desktop }
}

/**
 * Get responsive columns for grid
 */
export const getResponsiveColumns = (mobile: number, tablet: number, desktop: number) => {
  return { xs: mobile, sm: tablet, md: desktop }
}

