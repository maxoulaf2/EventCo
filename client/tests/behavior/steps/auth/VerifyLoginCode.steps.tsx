import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/auth/VerifyLoginCode.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Background, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Background(({ Given }) => {
    Given('j\'ai demandé un code de connexion pour "test@example.com"', async () => {
      // Non connecté jusqu'à la validation du code : le 401 initial de GET /api/auth/me est mis en
      // cache par la page de connexion, la validation doit le purger pour que la suite voie la session.
      let isAuthenticated = false
      server.use(
        http.get('*/api/auth/me', () =>
          isAuthenticated
            ? HttpResponse.json({ userId: 'user-1', email: 'test@example.com', displayName: 'Test', isAdmin: false })
            : new HttpResponse(null, { status: 401 }),
        ),
        http.post('*/api/auth/verify', () => {
          isAuthenticated = true
          return HttpResponse.json({ userId: 'user-1', email: 'test@example.com', displayName: 'Test', eventId: null })
        }),
      )
      renderApp('/')

      const user = userEvent.setup()
      await user.type(await screen.findByTestId('request-login-code-email-input'), 'test@example.com')
      await user.click(screen.getByTestId('request-login-code-submit-button'))
      await screen.findByTestId('check-email-page')
    })
  })

  Scenario('Code valide', ({ When, Then }) => {
    When('je saisis le code "123456" et je valide', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByTestId('verify-login-code-input'), '123456')
      await user.click(screen.getByTestId('verify-login-code-submit-button'))
    })

    Then('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-page')
    })
  })

  Scenario('Code refusé par le serveur', ({ And, When, Then }) => {
    And('le serveur refusera le code saisi', () => {
      server.use(
        http.post('*/api/auth/verify', () =>
          HttpResponse.json({ title: 'Code invalide', detail: 'Ce code est invalide ou a expiré.' }, { status: 400 }),
        ),
      )
    })

    When('je saisis le code "999999" et je valide', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByTestId('verify-login-code-input'), '999999')
      await user.click(screen.getByTestId('verify-login-code-submit-button'))
    })

    Then('je vois le message d\'erreur "Ce code est invalide ou a expiré."', async () => {
      expect(await screen.findByTestId('verify-login-code-error')).toHaveTextContent('Ce code est invalide ou a expiré.')
    })

    And('je reste sur la page de saisie du code', () => {
      expect(screen.getByTestId('check-email-page')).toBeInTheDocument()
    })

    And('le champ du code est vidé', () => {
      expect(screen.getByTestId('verify-login-code-input')).toHaveValue('')
    })
  })

  Scenario('Code incomplet', ({ When, Then }) => {
    When('je saisis le code "123"', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByTestId('verify-login-code-input'), '123')
    })

    Then('je ne peux pas valider le code', () => {
      expect(screen.getByTestId('verify-login-code-submit-button')).toBeDisabled()
    })
  })

  Scenario('Les caractères autres que des chiffres sont ignorés', ({ When, Then }) => {
    When('je saisis le code "123 456"', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByTestId('verify-login-code-input'), '123 456')
    })

    Then('le champ du code contient "123456"', () => {
      expect(screen.getByTestId('verify-login-code-input')).toHaveValue('123456')
    })
  })
})
