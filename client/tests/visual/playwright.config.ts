import path from 'node:path'
import { defineConfig } from '@playwright/test'
import { defineBddConfig } from 'playwright-bdd'

/**
 * Non-régression visuelle : comparaison de screenshots à plusieurs tailles d'écran.
 * Le back est mocké au niveau réseau (page.route) dans les steps — indépendant de
 * l'état de docker compose, seul `npm run dev` est nécessaire.
 */
const testDir = defineBddConfig({
  language: 'fr',
  features: 'features/**/*.feature',
  steps: 'steps/**/*.ts',
})

export default defineConfig({
  testDir,
  fullyParallel: true,
  // Chemins explicites (plutôt que la résolution par défaut de Playwright, pas toujours
  // relative au dossier du config) : la CI (.github/workflows/ci.yml) remonte ces dossiers
  // en artefact téléchargeable quand ce job échoue, elle a besoin d'un emplacement fiable.
  outputDir: path.join(import.meta.dirname, 'test-results'),
  reporter: [['html', { outputFolder: path.join(import.meta.dirname, 'playwright-report') }]],
  // Les specs sont générées (et régénérées) dans testDir (.features-gen, ignoré par git) :
  // les captures de référence doivent donc vivre en dehors, dans un dossier versionné stable.
  snapshotPathTemplate: '{testDir}/../__screenshots__/{testFileName}/{arg}-{projectName}{ext}',
  expect: {
    toHaveScreenshot: { maxDiffPixelRatio: 0.01 },
  },
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
  },
  projects: [
    { name: 'mobile', use: { viewport: { width: 375, height: 667 } } },
    { name: 'tablet', use: { viewport: { width: 768, height: 1024 } } },
    { name: 'desktop', use: { viewport: { width: 1440, height: 900 } } },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
})
