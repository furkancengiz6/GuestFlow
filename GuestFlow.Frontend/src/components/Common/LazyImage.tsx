import { useState, useEffect, useRef, ImgHTMLAttributes } from 'react'
import { Box, Skeleton } from '@mui/material'

interface LazyImageProps extends ImgHTMLAttributes<HTMLImageElement> {
  src: string
  alt: string
  placeholder?: string
  errorPlaceholder?: string
  threshold?: number
  rootMargin?: string
  skeletonWidth?: number | string
  skeletonHeight?: number | string
}

/**
 * Lazy loading image component with intersection observer
 */
export const LazyImage = ({
  src,
  alt,
  placeholder,
  errorPlaceholder = '/placeholder-image.png',
  threshold = 0.1,
  rootMargin = '50px',
  skeletonWidth = '100%',
  skeletonHeight = 200,
  style,
  ...props
}: LazyImageProps) => {
  const [imageSrc, setImageSrc] = useState<string>(placeholder || '')
  const [isLoaded, setIsLoaded] = useState(false)
  const [isInView, setIsInView] = useState(false)
  const imgRef = useRef<HTMLImageElement>(null)

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setIsInView(true)
            observer.disconnect()
          }
        })
      },
      {
        threshold,
        rootMargin,
      }
    )

    if (imgRef.current) {
      observer.observe(imgRef.current)
    }

    return () => {
      observer.disconnect()
    }
  }, [threshold, rootMargin])

  useEffect(() => {
    if (isInView && src) {
      const img = new Image()
      img.onload = () => {
        setImageSrc(src)
        setIsLoaded(true)
      }
      img.onerror = () => {
        setImageSrc(errorPlaceholder)
        setIsLoaded(true)
      }
      img.src = src
    }
  }, [isInView, src, errorPlaceholder])

  if (!isInView) {
    return (
      <Box
        ref={imgRef}
        sx={{
          width: skeletonWidth,
          height: skeletonHeight,
          display: 'inline-block',
        }}
      >
        <Skeleton variant="rectangular" width="100%" height="100%" />
      </Box>
    )
  }

  if (!isLoaded) {
    return (
      <Box
        sx={{
          width: skeletonWidth,
          height: skeletonHeight,
          display: 'inline-block',
        }}
      >
        <Skeleton variant="rectangular" width="100%" height="100%" />
      </Box>
    )
  }

  return (
    <img
      ref={imgRef}
      src={imageSrc}
      alt={alt}
      style={{
        opacity: isLoaded ? 1 : 0,
        transition: 'opacity 0.3s ease-in-out',
        ...style,
      }}
      {...props}
    />
  )
}

export default LazyImage

