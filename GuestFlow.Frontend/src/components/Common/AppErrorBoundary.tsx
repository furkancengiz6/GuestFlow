import React, { Component, ReactNode } from 'react'

type Props = {
  children: ReactNode
  onReset?: () => void
  fallback: (error: Error, reset: () => void) => ReactNode
}

type State = {
  error: Error | null
}

class AppErrorBoundary extends Component<Props, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    // Log the error for diagnostics; replace with a remote logger if available
    console.error('Unhandled UI error caught by AppErrorBoundary:', error, info)
  }

  reset = () => {
    this.props.onReset?.()
    this.setState({ error: null })
  }

  render() {
    const { error } = this.state
    const { children, fallback } = this.props

    if (error) {
      return fallback(error, this.reset)
    }

    return children
  }
}

export default AppErrorBoundary

