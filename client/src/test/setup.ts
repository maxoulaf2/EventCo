import '@testing-library/jest-dom/vitest'
import { afterAll, beforeAll } from 'vitest'
import { server } from './mocks/server'

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterAll(() => server.close())

// Pas de `afterEach(() => server.resetHandlers())` global ici : avec vitest-cucumber,
// chaque étape Gherkin (Given/When/Then...) est son propre test vitest, donc un
// `afterEach` réinitialiserait un `server.use(...)` avant même l'étape qui en dépend.
// Le reset des handlers se fait par scénario, via `AfterEachScenario` dans chaque
// fichier `.steps.tsx` (cf. RequestMagicLink.steps.tsx).
