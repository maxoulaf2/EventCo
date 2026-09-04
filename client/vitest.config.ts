import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config.ts'

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
