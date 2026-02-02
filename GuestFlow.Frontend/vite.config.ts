import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { visualizer } from 'rollup-plugin-visualizer'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  // Load env file based on `mode` in the current working directory.
  // Set the third parameter to '' to load all env instead of just those starting with `VITE_`.
  const env = loadEnv(mode, process.cwd(), '')

  return {
    define: {
      'process.env': env
    },
    plugins: [
      react(),
      // Bundle analyzer - run with ANALYZE=true npm run build
      ...(env.ANALYZE === 'true'
        ? [
          visualizer({
            filename: './dist/stats.html',
            open: false,
            gzipSize: true,
            brotliSize: true,
          }),
        ]
        : []),
    ],
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: 'http://localhost:5146',
          changeOrigin: true,
        },
      },
    },
    build: {
      // Code splitting configuration
      rollupOptions: {
        output: {
          manualChunks: {
            // Vendor chunks
            'react-vendor': ['react', 'react-dom', 'react-router-dom'],
            'mui-vendor': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
            'query-vendor': ['@tanstack/react-query', '@tanstack/react-query-devtools'],
            'form-vendor': ['react-hook-form', '@hookform/resolvers', 'zod'],
            'date-vendor': ['date-fns', '@mui/x-date-pickers'],
            'chart-vendor': ['recharts'],
            'signalr-vendor': ['@microsoft/signalr'],
            'i18n-vendor': ['i18next', 'react-i18next'],
          },
          // Optimize chunk file names
          chunkFileNames: 'assets/js/[name]-[hash].js',
          entryFileNames: 'assets/js/[name]-[hash].js',
          assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
        },
      },
      // Chunk size warnings
      chunkSizeWarningLimit: 1000,
      // Source maps for production debugging (optional)
      sourcemap: false, // Disable source maps in production for security
      // Minification
      minify: 'terser',
      terserOptions: {
        compress: {
          drop_console: env.NODE_ENV === 'production',
          drop_debugger: true,
        },
      },
      // Build target
      target: 'es2015',
      // CSS code splitting
      cssCodeSplit: true,
      // Report compressed size
      reportCompressedSize: true,
      // Empty output directory
      emptyOutDir: true,
    },
    optimizeDeps: {
      exclude: [],
      include: [
        '@mui/material',
        '@mui/icons-material',
        '@mui/x-date-pickers',
        '@tanstack/react-query',
        'react',
        'react-dom',
        'react-router-dom',
        'axios',
        'zustand',
        'date-fns',
        'i18next',
        'react-i18next',
      ],
    },
  }
})
