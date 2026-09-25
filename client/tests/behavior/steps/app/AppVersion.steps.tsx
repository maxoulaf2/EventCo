import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, waitFor } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { expect, vi } from 'vitest'
import { watchAppVersion } from '../../../../src/shared/lib/appVersionWatch'
import { reloadToLatestVersion } from '../../../../src/shared/lib/reloadToLatestVersion'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

// Le rechargement réel (mise à jour du service worker puis `location.reload()`) n'a pas de sens sous
// jsdom : seul le fait qu'il soit déclenché est vérifié.
vi.mock('../../../../src/shared/lib/reloadToLatestVersion', () => ({
  reloadToLatestVersion: vi.fn().mockResolvedValue(undefined),
}))

const feature = await loadFeature('tests/behavior/features/app/AppVersion.feature', { language: 'fr' })

let versionRequests = 0
let stopWatchingAppVersion: (() => void) | null = null

function appIsInVersion(_: unknown, version: string) {
  vi.stubEnv('VITE_APP_VERSION', version)
}

function apiIsDeployedInVersion(_: unknown, version: string) {
  const headers = { 'X-App-Version': version }
  server.use(
    http.get('*/api/version', () => {
      versionRequests++
      return HttpResponse.json({ version }, { headers })
    }),
    http.get('*/api/auth/me', () =>
      HttpResponse.json(
        { userId: 'user-1', email: 'test@example.com', displayName: 'Test', isAdmin: false, avatarUrl: null },
        { headers },
      ),
    ),
  )
}

async function returnToTab() {
  const requestsBefore = versionRequests
  document.dispatchEvent(new Event('visibilitychange'))
  await waitFor(() => expect(versionRequests).toBe(requestsBefore + 1))
  // La comparaison des versions a lieu à la réception de la réponse, juste après son interception par MSW.
  await new Promise((resolve) => setTimeout(resolve, 20))
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  BeforeEachScenario(() => {
    versionRequests = 0
    sessionStorage.clear()
    vi.mocked(reloadToLatestVersion).mockClear()
    stopWatchingAppVersion = watchAppVersion()
  })

  AfterEachScenario(() => {
    stopWatchingAppVersion?.()
    vi.unstubAllEnvs()
    server.resetHandlers()
    cleanup()
  })

  Scenario('Rechargement quand une réponse de l\'API vient d\'une autre version', ({ Given, And, When, Then }) => {
    Given('l\'application est en version {string}', appIsInVersion)
    And('l\'API a été déployée en version {string}', apiIsDeployedInVersion)

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('l\'application est rechargée', async () => {
      await waitFor(() => expect(reloadToLatestVersion).toHaveBeenCalled())
    })
  })

  Scenario('Rechargement au retour sur l\'onglet quand l\'API a été redéployée', ({ Given, And, When, Then }) => {
    Given('l\'application est en version {string}', appIsInVersion)
    And('l\'API a été déployée en version {string}', apiIsDeployedInVersion)
    When('je reviens sur l\'onglet de l\'application', returnToTab)

    Then('l\'application est rechargée', () => {
      expect(reloadToLatestVersion).toHaveBeenCalledTimes(1)
    })
  })

  Scenario('Pas de rechargement quand l\'API est dans la même version', ({ Given, And, When, Then }) => {
    Given('l\'application est en version {string}', appIsInVersion)
    And('l\'API a été déployée en version {string}', apiIsDeployedInVersion)
    When('je reviens sur l\'onglet de l\'application', returnToTab)

    Then('l\'application n\'est pas rechargée', () => {
      expect(reloadToLatestVersion).not.toHaveBeenCalled()
    })
  })

  Scenario('Pas de rechargement hors build déployé (développement local)', ({ Given, And, When, Then }) => {
    Given('l\'application est en version {string}', appIsInVersion)
    And('l\'API a été déployée en version {string}', apiIsDeployedInVersion)
    When('je reviens sur l\'onglet de l\'application', returnToTab)

    Then('l\'application n\'est pas rechargée', () => {
      expect(reloadToLatestVersion).not.toHaveBeenCalled()
    })
  })

  Scenario('Un seul rechargement par version, même si l\'application reste périmée', ({ Given, And, When, Then }) => {
    Given('l\'application est en version {string}', appIsInVersion)
    And('l\'API a été déployée en version {string}', apiIsDeployedInVersion)
    When('je reviens sur l\'onglet de l\'application', returnToTab)
    And('je reviens à nouveau sur l\'onglet de l\'application', returnToTab)

    Then('l\'application est rechargée une seule fois', () => {
      expect(reloadToLatestVersion).toHaveBeenCalledTimes(1)
    })
  })
})
