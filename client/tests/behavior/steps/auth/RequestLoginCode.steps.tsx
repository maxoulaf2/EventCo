import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/auth/RequestLoginCode.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Background, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    // Sans démontage explicite entre scénarios (aucun cleanup RTL global, cf. src/test/setup.ts),
    // le DOM du scénario précédent reste présent en plus de celui du scénario suivant.
    cleanup()
  })

  Background(({ Given }) => {
    Given('je suis sur la page de connexion', () => {
      server.use(http.get('*/api/auth/me', () => new HttpResponse(null, { status: 401 })))
      renderApp('/')
    })
  })

  Scenario('Email valide', ({ When, Then, And }) => {
    When('je saisis l\'email "test@example.com" et je valide le formulaire', async () => {
      const user = userEvent.setup()
      await user.type(await screen.findByTestId('request-login-code-email-input'), 'test@example.com')
      await user.click(screen.getByTestId('request-login-code-submit-button'))
    })

    Then('je suis redirigé vers la page de confirmation', async () => {
      await screen.findByTestId('check-email-page')
    })

    And('je vois l\'email "test@example.com" affiché', () => {
      expect(screen.getByTestId('check-email-page-email')).toHaveTextContent('test@example.com')
    })
  })

  Scenario('Le serveur refuse la demande', ({ And, When, Then }) => {
    And('le serveur refusera la prochaine demande de lien', () => {
      server.use(
        http.post('*/api/auth/request-code', () =>
          HttpResponse.json({ detail: 'Adresse email invalide.' }, { status: 400 }),
        ),
      )
    })

    When('je saisis l\'email "test@example.com" et je valide le formulaire', async () => {
      const user = userEvent.setup()
      await user.type(await screen.findByTestId('request-login-code-email-input'), 'test@example.com')
      await user.click(screen.getByTestId('request-login-code-submit-button'))
    })

    Then('je vois un message d\'erreur sur le formulaire', async () => {
      const errorMessage = await screen.findByTestId('request-login-code-error')
      expect(errorMessage).toHaveTextContent('Adresse email invalide.')
    })

    And('je reste sur la page de connexion', () => {
      expect(screen.getByTestId('request-login-code-submit-button')).toBeInTheDocument()
    })
  })
})
