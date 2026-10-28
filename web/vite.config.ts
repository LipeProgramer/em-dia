import { defineConfig, type Plugin } from 'vite'
import react from '@vitejs/plugin-react'

/**
 * Content-Security-Policy no HTML de produção (spec §4.5 · XSS). Fica fora do modo dev porque o
 * Vite injeta scripts inline para o recarregamento rápido.
 */
const csp: Plugin = {
  name: 'emdia-csp',
  apply: 'build',
  transformIndexHtml: () => [{
    tag: 'meta',
    attrs: {
      'http-equiv': 'Content-Security-Policy',
      content: "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; " +
        "connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'",
    },
    injectTo: 'head-prepend',
  }],
}

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), csp],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5196',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
