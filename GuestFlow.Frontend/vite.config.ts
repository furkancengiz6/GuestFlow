import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5146',
        changeOrigin: true,
      },
    },
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
    ],
  },
})

