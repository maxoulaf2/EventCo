import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config.ts'

// Fuseau fixe pour des tests déterministes quelle que soit la machine (poste local, CI en UTC) :
// les conversions heure locale <-> UTC (ex. date d'événement) en dépendent.
process.env.TZ = 'Europe/Paris'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      css: true,
      include: [
        '**/*.{test,spec}.?(c|m)[jt]s?(x)',
        'src/**/*.steps.{ts,tsx}',
        'tests/behavior/**/*.steps.{ts,tsx}',
      ],
      exclude: ['**/node_modules/**', '**/dist/**', 'tests/e2e/**', 'tests/visual/**'],
    },
  }),
)
