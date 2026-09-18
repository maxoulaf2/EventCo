import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { screen } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/auth/AutoRedirectWhenAuthenticated.feature', {
  language: 'fr',
})

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
  })

  Scenario('Session valide', ({ Given, When, Then }) => {
    Given('ma session est valide', () => {
      // Comportement par défaut du mock : GET /api/auth/me répond déjà 200.
    })

    When('j\'arrive sur la page de connexion', () => {
      renderApp('/')
    })

    Then('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
    })
  })

  Scenario('Pas de session', ({ Given, When, Then }) => {
    Given('je n\'ai pas de session', () => {
      server.use(http.get('*/api/auth/me', () => new HttpResponse(null, { status: 401 })))
    })

    When('j\'arrive sur la page de connexion', () => {
      renderApp('/')
    })

    Then('je vois le formulaire de connexion', async () => {
      await screen.findByTestId('login-page-title')
    })
  })
})
