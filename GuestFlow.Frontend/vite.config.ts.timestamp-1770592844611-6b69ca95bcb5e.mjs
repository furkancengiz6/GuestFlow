// vite.config.ts
import { defineConfig, loadEnv } from "file:///C:/GuestFlow/GuestFlow.Frontend/node_modules/vite/dist/node/index.js";
import react from "file:///C:/GuestFlow/GuestFlow.Frontend/node_modules/@vitejs/plugin-react/dist/index.js";
import { visualizer } from "file:///C:/GuestFlow/GuestFlow.Frontend/node_modules/rollup-plugin-visualizer/dist/plugin/index.js";
var vite_config_default = defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  return {
    define: {
      "process.env": env
    },
    plugins: [
      react(),
      // Bundle analyzer - run with ANALYZE=true npm run build
      ...env.ANALYZE === "true" ? [
        visualizer({
          filename: "./dist/stats.html",
          open: false,
          gzipSize: true,
          brotliSize: true
        })
      ] : []
    ],
    server: {
      port: 5173,
      proxy: {
        "/api": {
          target: "http://localhost:5146",
          changeOrigin: true
        }
      }
    },
    build: {
      // Code splitting configuration
      rollupOptions: {
        output: {
          manualChunks: {
            // Vendor chunks
            "react-vendor": ["react", "react-dom", "react-router-dom"],
            "mui-vendor": ["@mui/material", "@mui/icons-material", "@emotion/react", "@emotion/styled"],
            "query-vendor": ["@tanstack/react-query", "@tanstack/react-query-devtools"],
            "form-vendor": ["react-hook-form", "@hookform/resolvers", "zod"],
            "date-vendor": ["date-fns", "@mui/x-date-pickers"],
            "chart-vendor": ["recharts"],
            "signalr-vendor": ["@microsoft/signalr"],
            "i18n-vendor": ["i18next", "react-i18next"]
          },
          // Optimize chunk file names
          chunkFileNames: "assets/js/[name]-[hash].js",
          entryFileNames: "assets/js/[name]-[hash].js",
          assetFileNames: "assets/[ext]/[name]-[hash].[ext]"
        }
      },
      // Chunk size warnings
      chunkSizeWarningLimit: 1e3,
      // Source maps for production debugging (optional)
      sourcemap: false,
      // Disable source maps in production for security
      // Minification
      minify: "terser",
      terserOptions: {
        compress: {
          drop_console: env.NODE_ENV === "production",
          drop_debugger: true
        }
      },
      // Build target
      target: "es2015",
      // CSS code splitting
      cssCodeSplit: true,
      // Report compressed size
      reportCompressedSize: true,
      // Empty output directory
      emptyOutDir: true
    },
    optimizeDeps: {
      exclude: [],
      include: [
        "@mui/material",
        "@mui/icons-material",
        "@mui/x-date-pickers",
        "@tanstack/react-query",
        "react",
        "react-dom",
        "react-router-dom",
        "axios",
        "zustand",
        "date-fns",
        "i18next",
        "react-i18next"
      ]
    }
  };
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxHdWVzdEZsb3dcXFxcR3Vlc3RGbG93LkZyb250ZW5kXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ZpbGVuYW1lID0gXCJDOlxcXFxHdWVzdEZsb3dcXFxcR3Vlc3RGbG93LkZyb250ZW5kXFxcXHZpdGUuY29uZmlnLnRzXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ltcG9ydF9tZXRhX3VybCA9IFwiZmlsZTovLy9DOi9HdWVzdEZsb3cvR3Vlc3RGbG93LkZyb250ZW5kL3ZpdGUuY29uZmlnLnRzXCI7aW1wb3J0IHsgZGVmaW5lQ29uZmlnLCBsb2FkRW52IH0gZnJvbSAndml0ZSdcclxuaW1wb3J0IHJlYWN0IGZyb20gJ0B2aXRlanMvcGx1Z2luLXJlYWN0J1xyXG5pbXBvcnQgeyB2aXN1YWxpemVyIH0gZnJvbSAncm9sbHVwLXBsdWdpbi12aXN1YWxpemVyJ1xyXG5cclxuLy8gaHR0cHM6Ly92aXRlanMuZGV2L2NvbmZpZy9cclxuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKCh7IG1vZGUgfSkgPT4ge1xyXG4gIC8vIExvYWQgZW52IGZpbGUgYmFzZWQgb24gYG1vZGVgIGluIHRoZSBjdXJyZW50IHdvcmtpbmcgZGlyZWN0b3J5LlxyXG4gIC8vIFNldCB0aGUgdGhpcmQgcGFyYW1ldGVyIHRvICcnIHRvIGxvYWQgYWxsIGVudiBpbnN0ZWFkIG9mIGp1c3QgdGhvc2Ugc3RhcnRpbmcgd2l0aCBgVklURV9gLlxyXG4gIGNvbnN0IGVudiA9IGxvYWRFbnYobW9kZSwgcHJvY2Vzcy5jd2QoKSwgJycpXHJcblxyXG4gIHJldHVybiB7XHJcbiAgICBkZWZpbmU6IHtcclxuICAgICAgJ3Byb2Nlc3MuZW52JzogZW52XHJcbiAgICB9LFxyXG4gICAgcGx1Z2luczogW1xyXG4gICAgICByZWFjdCgpLFxyXG4gICAgICAvLyBCdW5kbGUgYW5hbHl6ZXIgLSBydW4gd2l0aCBBTkFMWVpFPXRydWUgbnBtIHJ1biBidWlsZFxyXG4gICAgICAuLi4oZW52LkFOQUxZWkUgPT09ICd0cnVlJ1xyXG4gICAgICAgID8gW1xyXG4gICAgICAgICAgdmlzdWFsaXplcih7XHJcbiAgICAgICAgICAgIGZpbGVuYW1lOiAnLi9kaXN0L3N0YXRzLmh0bWwnLFxyXG4gICAgICAgICAgICBvcGVuOiBmYWxzZSxcclxuICAgICAgICAgICAgZ3ppcFNpemU6IHRydWUsXHJcbiAgICAgICAgICAgIGJyb3RsaVNpemU6IHRydWUsXHJcbiAgICAgICAgICB9KSxcclxuICAgICAgICBdXHJcbiAgICAgICAgOiBbXSksXHJcbiAgICBdLFxyXG4gICAgc2VydmVyOiB7XHJcbiAgICAgIHBvcnQ6IDUxNzMsXHJcbiAgICAgIHByb3h5OiB7XHJcbiAgICAgICAgJy9hcGknOiB7XHJcbiAgICAgICAgICB0YXJnZXQ6ICdodHRwOi8vbG9jYWxob3N0OjUxNDYnLFxyXG4gICAgICAgICAgY2hhbmdlT3JpZ2luOiB0cnVlLFxyXG4gICAgICAgIH0sXHJcbiAgICAgIH0sXHJcbiAgICB9LFxyXG4gICAgYnVpbGQ6IHtcclxuICAgICAgLy8gQ29kZSBzcGxpdHRpbmcgY29uZmlndXJhdGlvblxyXG4gICAgICByb2xsdXBPcHRpb25zOiB7XHJcbiAgICAgICAgb3V0cHV0OiB7XHJcbiAgICAgICAgICBtYW51YWxDaHVua3M6IHtcclxuICAgICAgICAgICAgLy8gVmVuZG9yIGNodW5rc1xyXG4gICAgICAgICAgICAncmVhY3QtdmVuZG9yJzogWydyZWFjdCcsICdyZWFjdC1kb20nLCAncmVhY3Qtcm91dGVyLWRvbSddLFxyXG4gICAgICAgICAgICAnbXVpLXZlbmRvcic6IFsnQG11aS9tYXRlcmlhbCcsICdAbXVpL2ljb25zLW1hdGVyaWFsJywgJ0BlbW90aW9uL3JlYWN0JywgJ0BlbW90aW9uL3N0eWxlZCddLFxyXG4gICAgICAgICAgICAncXVlcnktdmVuZG9yJzogWydAdGFuc3RhY2svcmVhY3QtcXVlcnknLCAnQHRhbnN0YWNrL3JlYWN0LXF1ZXJ5LWRldnRvb2xzJ10sXHJcbiAgICAgICAgICAgICdmb3JtLXZlbmRvcic6IFsncmVhY3QtaG9vay1mb3JtJywgJ0Bob29rZm9ybS9yZXNvbHZlcnMnLCAnem9kJ10sXHJcbiAgICAgICAgICAgICdkYXRlLXZlbmRvcic6IFsnZGF0ZS1mbnMnLCAnQG11aS94LWRhdGUtcGlja2VycyddLFxyXG4gICAgICAgICAgICAnY2hhcnQtdmVuZG9yJzogWydyZWNoYXJ0cyddLFxyXG4gICAgICAgICAgICAnc2lnbmFsci12ZW5kb3InOiBbJ0BtaWNyb3NvZnQvc2lnbmFsciddLFxyXG4gICAgICAgICAgICAnaTE4bi12ZW5kb3InOiBbJ2kxOG5leHQnLCAncmVhY3QtaTE4bmV4dCddLFxyXG4gICAgICAgICAgfSxcclxuICAgICAgICAgIC8vIE9wdGltaXplIGNodW5rIGZpbGUgbmFtZXNcclxuICAgICAgICAgIGNodW5rRmlsZU5hbWVzOiAnYXNzZXRzL2pzL1tuYW1lXS1baGFzaF0uanMnLFxyXG4gICAgICAgICAgZW50cnlGaWxlTmFtZXM6ICdhc3NldHMvanMvW25hbWVdLVtoYXNoXS5qcycsXHJcbiAgICAgICAgICBhc3NldEZpbGVOYW1lczogJ2Fzc2V0cy9bZXh0XS9bbmFtZV0tW2hhc2hdLltleHRdJyxcclxuICAgICAgICB9LFxyXG4gICAgICB9LFxyXG4gICAgICAvLyBDaHVuayBzaXplIHdhcm5pbmdzXHJcbiAgICAgIGNodW5rU2l6ZVdhcm5pbmdMaW1pdDogMTAwMCxcclxuICAgICAgLy8gU291cmNlIG1hcHMgZm9yIHByb2R1Y3Rpb24gZGVidWdnaW5nIChvcHRpb25hbClcclxuICAgICAgc291cmNlbWFwOiBmYWxzZSwgLy8gRGlzYWJsZSBzb3VyY2UgbWFwcyBpbiBwcm9kdWN0aW9uIGZvciBzZWN1cml0eVxyXG4gICAgICAvLyBNaW5pZmljYXRpb25cclxuICAgICAgbWluaWZ5OiAndGVyc2VyJyxcclxuICAgICAgdGVyc2VyT3B0aW9uczoge1xyXG4gICAgICAgIGNvbXByZXNzOiB7XHJcbiAgICAgICAgICBkcm9wX2NvbnNvbGU6IGVudi5OT0RFX0VOViA9PT0gJ3Byb2R1Y3Rpb24nLFxyXG4gICAgICAgICAgZHJvcF9kZWJ1Z2dlcjogdHJ1ZSxcclxuICAgICAgICB9LFxyXG4gICAgICB9LFxyXG4gICAgICAvLyBCdWlsZCB0YXJnZXRcclxuICAgICAgdGFyZ2V0OiAnZXMyMDE1JyxcclxuICAgICAgLy8gQ1NTIGNvZGUgc3BsaXR0aW5nXHJcbiAgICAgIGNzc0NvZGVTcGxpdDogdHJ1ZSxcclxuICAgICAgLy8gUmVwb3J0IGNvbXByZXNzZWQgc2l6ZVxyXG4gICAgICByZXBvcnRDb21wcmVzc2VkU2l6ZTogdHJ1ZSxcclxuICAgICAgLy8gRW1wdHkgb3V0cHV0IGRpcmVjdG9yeVxyXG4gICAgICBlbXB0eU91dERpcjogdHJ1ZSxcclxuICAgIH0sXHJcbiAgICBvcHRpbWl6ZURlcHM6IHtcclxuICAgICAgZXhjbHVkZTogW10sXHJcbiAgICAgIGluY2x1ZGU6IFtcclxuICAgICAgICAnQG11aS9tYXRlcmlhbCcsXHJcbiAgICAgICAgJ0BtdWkvaWNvbnMtbWF0ZXJpYWwnLFxyXG4gICAgICAgICdAbXVpL3gtZGF0ZS1waWNrZXJzJyxcclxuICAgICAgICAnQHRhbnN0YWNrL3JlYWN0LXF1ZXJ5JyxcclxuICAgICAgICAncmVhY3QnLFxyXG4gICAgICAgICdyZWFjdC1kb20nLFxyXG4gICAgICAgICdyZWFjdC1yb3V0ZXItZG9tJyxcclxuICAgICAgICAnYXhpb3MnLFxyXG4gICAgICAgICd6dXN0YW5kJyxcclxuICAgICAgICAnZGF0ZS1mbnMnLFxyXG4gICAgICAgICdpMThuZXh0JyxcclxuICAgICAgICAncmVhY3QtaTE4bmV4dCcsXHJcbiAgICAgIF0sXHJcbiAgICB9LFxyXG4gIH1cclxufSlcclxuIl0sCiAgIm1hcHBpbmdzIjogIjtBQUFxUixTQUFTLGNBQWMsZUFBZTtBQUMzVCxPQUFPLFdBQVc7QUFDbEIsU0FBUyxrQkFBa0I7QUFHM0IsSUFBTyxzQkFBUSxhQUFhLENBQUMsRUFBRSxLQUFLLE1BQU07QUFHeEMsUUFBTSxNQUFNLFFBQVEsTUFBTSxRQUFRLElBQUksR0FBRyxFQUFFO0FBRTNDLFNBQU87QUFBQSxJQUNMLFFBQVE7QUFBQSxNQUNOLGVBQWU7QUFBQSxJQUNqQjtBQUFBLElBQ0EsU0FBUztBQUFBLE1BQ1AsTUFBTTtBQUFBO0FBQUEsTUFFTixHQUFJLElBQUksWUFBWSxTQUNoQjtBQUFBLFFBQ0EsV0FBVztBQUFBLFVBQ1QsVUFBVTtBQUFBLFVBQ1YsTUFBTTtBQUFBLFVBQ04sVUFBVTtBQUFBLFVBQ1YsWUFBWTtBQUFBLFFBQ2QsQ0FBQztBQUFBLE1BQ0gsSUFDRSxDQUFDO0FBQUEsSUFDUDtBQUFBLElBQ0EsUUFBUTtBQUFBLE1BQ04sTUFBTTtBQUFBLE1BQ04sT0FBTztBQUFBLFFBQ0wsUUFBUTtBQUFBLFVBQ04sUUFBUTtBQUFBLFVBQ1IsY0FBYztBQUFBLFFBQ2hCO0FBQUEsTUFDRjtBQUFBLElBQ0Y7QUFBQSxJQUNBLE9BQU87QUFBQTtBQUFBLE1BRUwsZUFBZTtBQUFBLFFBQ2IsUUFBUTtBQUFBLFVBQ04sY0FBYztBQUFBO0FBQUEsWUFFWixnQkFBZ0IsQ0FBQyxTQUFTLGFBQWEsa0JBQWtCO0FBQUEsWUFDekQsY0FBYyxDQUFDLGlCQUFpQix1QkFBdUIsa0JBQWtCLGlCQUFpQjtBQUFBLFlBQzFGLGdCQUFnQixDQUFDLHlCQUF5QixnQ0FBZ0M7QUFBQSxZQUMxRSxlQUFlLENBQUMsbUJBQW1CLHVCQUF1QixLQUFLO0FBQUEsWUFDL0QsZUFBZSxDQUFDLFlBQVkscUJBQXFCO0FBQUEsWUFDakQsZ0JBQWdCLENBQUMsVUFBVTtBQUFBLFlBQzNCLGtCQUFrQixDQUFDLG9CQUFvQjtBQUFBLFlBQ3ZDLGVBQWUsQ0FBQyxXQUFXLGVBQWU7QUFBQSxVQUM1QztBQUFBO0FBQUEsVUFFQSxnQkFBZ0I7QUFBQSxVQUNoQixnQkFBZ0I7QUFBQSxVQUNoQixnQkFBZ0I7QUFBQSxRQUNsQjtBQUFBLE1BQ0Y7QUFBQTtBQUFBLE1BRUEsdUJBQXVCO0FBQUE7QUFBQSxNQUV2QixXQUFXO0FBQUE7QUFBQTtBQUFBLE1BRVgsUUFBUTtBQUFBLE1BQ1IsZUFBZTtBQUFBLFFBQ2IsVUFBVTtBQUFBLFVBQ1IsY0FBYyxJQUFJLGFBQWE7QUFBQSxVQUMvQixlQUFlO0FBQUEsUUFDakI7QUFBQSxNQUNGO0FBQUE7QUFBQSxNQUVBLFFBQVE7QUFBQTtBQUFBLE1BRVIsY0FBYztBQUFBO0FBQUEsTUFFZCxzQkFBc0I7QUFBQTtBQUFBLE1BRXRCLGFBQWE7QUFBQSxJQUNmO0FBQUEsSUFDQSxjQUFjO0FBQUEsTUFDWixTQUFTLENBQUM7QUFBQSxNQUNWLFNBQVM7QUFBQSxRQUNQO0FBQUEsUUFDQTtBQUFBLFFBQ0E7QUFBQSxRQUNBO0FBQUEsUUFDQTtBQUFBLFFBQ0E7QUFBQSxRQUNBO0FBQUEsUUFDQTtBQUFBLFFBQ0E7QUFBQSxRQUNBO0FBQUEsUUFDQTtBQUFBLFFBQ0E7QUFBQSxNQUNGO0FBQUEsSUFDRjtBQUFBLEVBQ0Y7QUFDRixDQUFDOyIsCiAgIm5hbWVzIjogW10KfQo=
