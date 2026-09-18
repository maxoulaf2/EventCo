import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/auth/Logout.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Scenario('Ouverture et fermeture de la modale de mon compte', ({ When, And, Then }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('events-dashboard-account-button'))
    })

    Then('je vois le bouton de déconnexion', () => {
      expect(screen.getByTestId('account-modal-logout-button')).toBeInTheDocument()
    })

    When('je ferme la modale de mon compte', async () => {
      const user = userEvent.setup()
      await user.click(screen.getByTestId('account-modal-close-button'))
    })

    Then('je ne vois plus le bouton de déconnexion', () => {
      expect(screen.queryByTestId('account-modal-logout-button')).not.toBeInTheDocument()
    })
  })

  Scenario('Déconnexion depuis la modale de mon compte', ({ When, And, Then }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('events-dashboard-account-button'))
    })

    And('je clique sur le bouton de déconnexion', async () => {
      // Simule la session fermée côté back (cookie supprimé) : le refetch déclenché après la
      // déconnexion doit échouer pour que la page de connexion ne redirige pas aussitôt vers le
      // tableau de bord (cf. AutoRedirectWhenAuthenticated).
      server.use(http.get('*/api/auth/me', () => new HttpResponse(null, { status: 401 })))

      const user = userEvent.setup()
      await user.click(screen.getByTestId('account-modal-logout-button'))
    })

    Then('je suis redirigé vers la page de connexion', async () => {
      await screen.findByTestId('login-page-title')
    })
  })
})
