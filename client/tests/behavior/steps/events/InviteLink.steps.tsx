import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/InviteLink.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Scenario('Un utilisateur déjà connecté rejoint automatiquement l\'événement', ({ Given, When, Then }) => {
    Given('je suis connecté', () => {
      // Le handler par défaut de GET /api/auth/me renvoie déjà un utilisateur connecté.
    })

    When('j\'ouvre le lien d\'invitation "invite-token-1"', () => {
      renderApp('/invite/invite-token-1')
    })

    Then('je suis redirigé vers la page de l\'événement', async () => {
      await screen.findByTestId('event-detail-page')
    })
  })

  Scenario('Un utilisateur non connecté voit un aperçu, se connecte avec le code reçu et rejoint l\'événement', ({ Given, When, Then }) => {
    Given('je ne suis pas connecté', () => {
      let isAuthenticated = false
      server.use(
        http.get('*/api/auth/me', () =>
          isAuthenticated
            ? HttpResponse.json({ userId: 'user-1', email: 'invite@example.com', displayName: 'invite', isAdmin: false })
            : new HttpResponse(null, { status: 401 }),
        ),
        // L'intention de rejoindre l'événement est résolue côté serveur à la validation du code,
        // qui renvoie alors l'événement rejoint.
        http.post('*/api/auth/verify', () => {
          isAuthenticated = true
          return HttpResponse.json({ userId: 'user-1', email: 'invite@example.com', displayName: 'invite', eventId: 'event-1' })
        }),
      )
    })

    When('j\'ouvre le lien d\'invitation "invite-token-1"', () => {
      renderApp('/invite/invite-token-1')
    })

    Then('je vois l\'aperçu de l\'événement "Repas de Noël"', async () => {
      const title = await screen.findByTestId('invite-link-preview-title')
      expect(title).toHaveTextContent('Repas de Noël')
    })

    When('je saisis l\'email "invite@example.com" et je valide le formulaire de connexion', async () => {
      const user = userEvent.setup()
      await user.type(await screen.findByTestId('request-magic-link-email-input'), 'invite@example.com')
      await user.click(screen.getByTestId('request-magic-link-submit-button'))
    })

    Then('je suis redirigé vers la page de confirmation', async () => {
      await screen.findByTestId('check-email-page')
    })

    When('je saisis le code reçu et je valide', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByTestId('verify-login-code-input'), '123456')
      await user.click(screen.getByTestId('verify-login-code-submit-button'))
    })

    Then('je suis redirigé vers la page de l\'événement rejoint', async () => {
      await screen.findByTestId('event-detail-page')
    })
  })

  Scenario('Lien d\'invitation invalide pour un utilisateur non connecté', ({ Given, And, When, Then }) => {
    Given('je ne suis pas connecté', () => {
      server.use(http.get('*/api/auth/me', () => new HttpResponse(null, { status: 401 })))
    })

    And('le lien d\'invitation n\'est plus valide', () => {
      server.use(
        http.get('*/api/events/invite-links/:token/preview', () =>
          HttpResponse.json({ detail: 'Aucun événement ne correspond à ce lien d\'invitation.' }, { status: 404 }),
        ),
      )
    })

    When('j\'ouvre le lien d\'invitation "token-invalide"', () => {
      renderApp('/invite/token-invalide')
    })

    Then('je vois un message indiquant que le lien n\'est plus valide', async () => {
      await screen.findByTestId('invite-link-invalid')
    })
  })
})
