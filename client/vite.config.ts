import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

// Cible de l'API proxifiée en dev : profil https par défaut (`dotnet run --launch-profile https`).
// Surchargeable via VITE_API_PROXY_TARGET (ex: http://localhost:5000 pour l'API lancée via docker compose).
// `.env` n'est pas spécifique à un mode donc peu importe le mode passé ici.
const env = loadEnv(process.env.NODE_ENV ?? 'development', process.cwd(), '')
const apiProxyTarget = env.VITE_API_PROXY_TARGET || 'https://localhost:7166'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.png'],
      manifest: {
        name: 'EventCo',
        short_name: 'EventCo',
        description: 'Co-organisez vos événements de groupe : invitations, répartition des tâches, suivi en temps réel.',
        lang: 'fr',
        start_url: '/',
        display: 'standalone',
        background_color: '#faf6e3',
        theme_color: '#bfaa5f',
        icons: [
          { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
          { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
          { src: 'pwa-maskable-512x512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
        ],
      },
    }),
  ],
  server: {
    proxy: {
      // Le frontend et l'API doivent rester same-origin du point de vue du navigateur : sinon le
      // cookie de session est traité comme cookie tiers et bloqué par défaut en navigation privée
      // (ITP Safari, ETP strict Firefox privé, Chrome Incognito). Cf. docs/suivi-todo.md, Lot 1.
      '/api': {
        target: apiProxyTarget,
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: apiProxyTarget,
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
