# Testing Guide

Bu klasörde frontend için unit, component ve integration testleri bulunmaktadır.

## Test Yapısı

```
src/__tests__/
├── components/          # Component testleri
├── hooks/               # Hook testleri
├── utils/               # Utility function testleri
└── integration/         # Integration testleri
```

## Test Komutları

```bash
# Tüm testleri çalıştır
npm test

# Watch mode
npm run test:watch

# Coverage raporu
npm run test:coverage

# CI için (coverage ile)
npm run test:ci
```

## Test Yazma Kuralları

### Unit Tests

Utility fonksiyonlar ve pure functions için:

```tsx
import { formatCurrency } from '../../utils/formatters'

describe('formatCurrency', () => {
  it('should format currency correctly', () => {
    expect(formatCurrency(1000, 'TRY')).toContain('1.000')
  })
})
```

### Component Tests

React component'leri için React Testing Library kullanın:

```tsx
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MyComponent from '../MyComponent'

describe('MyComponent', () => {
  it('should render correctly', () => {
    render(<MyComponent />)
    expect(screen.getByText('Hello')).toBeInTheDocument()
  })

  it('should handle user interaction', async () => {
    const user = userEvent.setup()
    render(<MyComponent />)
    
    const button = screen.getByRole('button')
    await user.click(button)
    
    expect(screen.getByText('Clicked')).toBeInTheDocument()
  })
})
```

### Integration Tests

Birden fazla component'in birlikte çalışmasını test edin:

```tsx
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'

describe('Auth Integration', () => {
  it('should handle login flow', async () => {
    // Test implementation
  })
})
```

## Mocking

### API Calls

```tsx
jest.mock('../../services/api', () => ({
  default: {
    get: jest.fn(),
    post: jest.fn(),
  },
}))
```

### Stores

```tsx
jest.mock('../../stores/authStore', () => ({
  useAuthStore: jest.fn(() => ({
    isAuthenticated: true,
    user: { id: 1, email: 'test@example.com' },
  })),
}))
```

## Best Practices

1. **Test Isolation**: Her test bağımsız olmalı
2. **Arrange-Act-Assert**: Test yapısını bu pattern'e göre yazın
3. **Descriptive Names**: Test isimleri ne test ettiğini açıklamalı
4. **Mock External Dependencies**: API calls, stores, vb. mock'layın
5. **Test User Behavior**: Implementation details değil, user behavior test edin
6. **Coverage Goals**: Minimum %50 coverage hedefleyin

## Coverage

Coverage raporu `coverage/` klasöründe oluşturulur:

- `coverage/lcov-report/index.html` - HTML rapor
- `coverage/lcov.info` - LCOV format
- `coverage/coverage-final.json` - JSON format

## E2E Tests

E2E testler `tests/e2e/` klasöründe Playwright ile yazılmıştır:

```bash
npm run test:e2e
```

