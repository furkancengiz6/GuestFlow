// Performance utilities for GuestFlow Frontend

// Lazy load wrapper with error boundary
import { ComponentType, lazy } from 'react';

export function lazyLoad<T extends ComponentType<any>>(
  importFunc: () => Promise<{ default: T }>,
  fallbackName?: string
) {
  return lazy(() =>
    importFunc().catch((error) => {
      console.error(`Failed to load component${fallbackName ? ` ${fallbackName}` : ''}:`, error);
      // Return a fallback component
      return {
        default: () => (
          <div className="error-fallback">
            <p>Failed to load component. Please refresh the page.</p>
          </div>
        )
      } as { default: ComponentType<any> };
    })
  );
}

// Image lazy loading utility
export const lazyLoadImage = (
  src: string,
  alt: string,
  className?: string,
  onLoad?: () => void
) => {
  return (
    <img
      src={src}
      alt={alt}
      className={className}
      loading="lazy"
      decoding="async"
      onLoad={onLoad}
      style={{ opacity: 0, transition: 'opacity 0.3s' }}
      onError={(e) => {
        const target = e.target as HTMLImageElement;
        target.style.display = 'none';
      }}
    />
  );
};

// Bundle size monitoring
export const logBundleSize = () => {
  if (process.env.NODE_ENV === 'development') {
    // This will be replaced by webpack during build
    console.log('Bundle size monitoring enabled');
  }
};

// Memory usage monitoring
export const monitorMemoryUsage = () => {
  if (process.env.NODE_ENV === 'development' && 'memory' in performance) {
    const memInfo = (performance as any).memory;
    console.log('Memory usage:', {
      used: Math.round(memInfo.usedJSHeapSize / 1048576 * 100) / 100 + ' MB',
      total: Math.round(memInfo.totalJSHeapSize / 1048576 * 100) / 100 + ' MB',
      limit: Math.round(memInfo.jsHeapSizeLimit / 1048576 * 100) / 100 + ' MB'
    });
  }
};

// Debounce utility for search inputs
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout;
  return (...args: Parameters<T>) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), wait);
  };
}

// Intersection Observer for lazy loading
export const createIntersectionObserver = (
  callback: IntersectionObserverCallback,
  options?: IntersectionObserverInit
) => {
  if ('IntersectionObserver' in window) {
    return new IntersectionObserver(callback, {
      root: null,
      rootMargin: '50px',
      threshold: 0.1,
      ...options
    });
  }
  return null;
};

// Web Vitals monitoring
export const reportWebVitals = (metric: any) => {
  if (process.env.NODE_ENV === 'development') {
    console.log('Web Vital:', metric);
  }
  // In production, send to analytics service
  // analytics.track('web_vital', metric);
};

