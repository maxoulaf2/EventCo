import { defineConfig, devices } from '@playwright/test'
import { defineBddConfig } from 'playwright-bdd'

/**
 * E2E nominal : contre la vraie stack, sans aucun mock. `globalSetup`/`globalTeardown`
 * démarrent et arrêtent une stack API + PostgreSQL dédiée à l'E2E (`docker-compose.e2e.yml`,
 * ports 5433/5001, volume jetable) pour ne jamais toucher à la base de dev (`docker compose
 * up -d postgres api`, ports 5432/5000).
 */
const testDir = defineBddConfig({
  language: 'fr',
  features: 'features/**/*.feature',
  steps: 'steps/**/*.ts',
})

const apiUrl = process.env.VITE_API_URL ?? 'http://localhost:5001'

export default defineConfig({
  testDir,
  fullyParallel: true,
  reporter: 'html',
  globalSetup: './global-setup.ts',
  globalTeardown: './global-teardown.ts',
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    env: { VITE_API_URL: apiUrl },
  },
})
