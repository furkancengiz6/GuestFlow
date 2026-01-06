# Performance Optimization Components

Bu klasörde performans optimizasyonu için kullanılan component'ler bulunmaktadır.

## LazyImage

Lazy loading image component. Intersection Observer API kullanarak görüntü yalnızca görünür olduğunda yüklenir.

### Kullanım

```tsx
import { LazyImage } from '../../components/Common/LazyImage'

<LazyImage
  src="/path/to/image.jpg"
  alt="Description"
  skeletonWidth={300}
  skeletonHeight={200}
/>
```

### Props

- `src`: Image source URL
- `alt`: Alt text
- `placeholder`: Optional placeholder image
- `errorPlaceholder`: Error durumunda gösterilecek görsel
- `threshold`: Intersection threshold (default: 0.1)
- `rootMargin`: Root margin for intersection (default: '50px')
- `skeletonWidth`: Loading skeleton width
- `skeletonHeight`: Loading skeleton height

## VirtualizedList

Büyük listeler için virtual scrolling component. Yalnızca görünür öğeler render edilir.

### Kullanım

```tsx
import { VirtualizedList } from '../../components/Common/VirtualizedList'

<VirtualizedList
  items={largeArray}
  renderItem={(item, index) => <div>{item.name}</div>}
  itemHeight={50}
  containerHeight={400}
  overscan={5}
/>
```

### Props

- `items`: Array of items to render
- `renderItem`: Function to render each item
- `itemHeight`: Height of each item in pixels
- `containerHeight`: Height of the container
- `overscan`: Number of items to render outside visible area
- `onScroll`: Optional scroll handler

## MemoizedComponent

Component memoization için HOC ve wrapper component.

### Kullanım

```tsx
import { withMemo } from '../../components/Common/MemoizedComponent'

const MyComponent = ({ data }) => {
  return <div>{data}</div>
}

export default withMemo(MyComponent)
```

## Performance Hooks

### useMemoization

Memoization utilities hook.

```tsx
import { useMemoizedArray, useMemoizedFilter, useMemoizedSort } from '../../hooks/useMemoization'

const MyComponent = ({ items }) => {
  const filtered = useMemoizedFilter(items, (item) => item.active)
  const sorted = useMemoizedSort(filtered, (a, b) => a.name.localeCompare(b.name))
  
  return <div>{sorted.map(...)}</div>
}
```

## Code Splitting

Tüm sayfalar `App.tsx` içinde lazy loading ile yüklenir:

```tsx
const DashboardPage = lazy(() => import('./pages/Dashboard/DashboardPage'))
```

Bu sayede her sayfa ayrı bir chunk olarak bundle edilir ve yalnızca gerektiğinde yüklenir.

## Bundle Analysis

Bundle analizi için:

```bash
npm run build:analyze
```

Bu komut `dist/stats.html` dosyası oluşturur ve bundle boyutlarını görselleştirir.

