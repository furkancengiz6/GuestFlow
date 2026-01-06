/**
 * Accessibility utilities and helpers
 */

/**
 * Generate accessible label for form fields
 */
export const getAccessibleLabel = (label: string, required?: boolean): string => {
  return required ? `${label} (Zorunlu)` : label
}

/**
 * Generate accessible description for form fields
 */
export const getAccessibleDescription = (description: string, error?: string): string => {
  if (error) return error
  return description
}

/**
 * ARIA attributes helper
 */
export const getAriaAttributes = (options: {
  label?: string
  describedBy?: string
  required?: boolean
  invalid?: boolean
  errorMessage?: string
}) => {
  const { label, describedBy, required, invalid, errorMessage } = options

  return {
    'aria-label': label,
    'aria-describedby': describedBy,
    'aria-required': required,
    'aria-invalid': invalid,
    'aria-errormessage': errorMessage,
  }
}

/**
 * Keyboard navigation helpers
 */
export const handleKeyDown = (
  event: React.KeyboardEvent,
  callbacks: {
    onEnter?: () => void
    onEscape?: () => void
    onArrowUp?: () => void
    onArrowDown?: () => void
    onTab?: () => void
  }
) => {
  switch (event.key) {
    case 'Enter':
      callbacks.onEnter?.()
      break
    case 'Escape':
      callbacks.onEscape?.()
      break
    case 'ArrowUp':
      callbacks.onArrowUp?.()
      break
    case 'ArrowDown':
      callbacks.onArrowDown?.()
      break
    case 'Tab':
      callbacks.onTab?.()
      break
  }
}

/**
 * Focus management
 */
export const focusElement = (element: HTMLElement | null) => {
  if (element) {
    element.focus()
    element.scrollIntoView({ behavior: 'smooth', block: 'center' })
  }
}

/**
 * Skip to main content link (for screen readers)
 */
export const skipToMainContent = () => {
  const mainContent = document.getElementById('main-content')
  if (mainContent) {
    mainContent.focus()
    mainContent.scrollIntoView({ behavior: 'smooth' })
  }
}

