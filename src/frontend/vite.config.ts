import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT || '5173', 10),
    host: '0.0.0.0',
     proxy: {
      '/api': {
        target: 'http://localhost:5273',  // Все API идут через nginx
        changeOrigin: true,
      }



      // '/auth': {
      //   target: process.env.VITE_AUTH_API_URL || 'http://localhost:8080',
      //   changeOrigin: true,
      //   rewrite: (path) => path.replace(/^\/auth/, 'api/v1/auth'),
      // },
      // '/api/v1': {
      //   target: process.env.VITE_STOCKCARD_API_URL || 'http://localhost:8081',
      //   changeOrigin: true,
      // },
      // '/api/portfolio': {
      //   target: process.env.VITE_PORTFOLIO_API_URL || 'http://localhost:8082',
      //   changeOrigin: true,
      //   rewrite: (path) => path.replace(/^\/api\/portfolio/, ''),
      // },
      // '/api/analytics': {
      //   target: process.env.VITE_ANALYTICS_API_URL || 'http://localhost:8083',
      //   changeOrigin: true,
      //   rewrite: (path) => path.replace(/^\/api\/analytics/, ''),
      // },
    },
  }
});