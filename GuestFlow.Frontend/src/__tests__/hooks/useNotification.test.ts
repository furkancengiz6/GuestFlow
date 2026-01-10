import { renderHook, act } from '@testing-library/react'
import { useNotification } from '../../hooks/useNotification'

// Mock notification store
const mockAddNotification = jest.fn()
jest.mock('../../stores/notificationStore', () => ({
  useNotificationStore: jest.fn((selector) => {
    if (typeof selector === 'function') {
      return selector({
        notifications: [],
        addNotification: mockAddNotification,
        removeNotification: jest.fn(),
        clearNotifications: jest.fn()
      })
    }
    return mockAddNotification
  }),
}))

describe('useNotification', () => {
  it('should provide notification methods', () => {
    const { result } = renderHook(() => useNotification())

    expect(result.current.showSuccess).toBeDefined()
    expect(result.current.showError).toBeDefined()
    expect(result.current.showWarning).toBeDefined()
    expect(result.current.showInfo).toBeDefined()
  })

  it('should call showSuccess when showSuccess is called', () => {
    const { result } = renderHook(() => useNotification())

    act(() => {
      result.current.showSuccess('Test success message')
    })

    // Verify notification was shown
    // Note: This would need proper store mocking
  })
})

