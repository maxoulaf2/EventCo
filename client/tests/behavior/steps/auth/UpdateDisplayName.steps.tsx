import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/auth/UpdateDisplayName.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Scenario('Modification avec un nom d\'affichage valide', ({ When, And, Then }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('events-dashboard-account-button'))
    })

    Then('le champ nom d\'affichage est pré-rempli avec "Test"', () => {
      expect(screen.getByTestId('account-modal-display-name-input')).toHaveValue('Test')
    })

    When('je modifie le nom d\'affichage en "Alice B."', async () => {
      const user = userEvent.setup()
      await user.clear(screen.getByTestId('account-modal-display-name-input'))
      await user.type(screen.getByTestId('account-modal-display-name-input'), 'Alice B.')
    })

    And('je valide le formulaire de nom d\'affichage', async () => {
      const user = userEvent.setup()
      await user.click(screen.getByTestId('account-modal-display-name-submit-button'))
    })

    Then('je vois un message de succès', async () => {
      await screen.findByTestId('account-modal-display-name-success')
    })
  })

  Scenario('Le serveur refuse la modification', ({ When, And, Then }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('events-dashboard-account-button'))
    })

    And('le serveur refuse la modification du nom d\'affichage', () => {
      server.use(
        http.put('*/api/auth/me', () =>
          HttpResponse.json({ detail: 'Le nom d\'affichage est obligatoire.' }, { status: 400 }),
        ),
      )
    })

    And('je modifie le nom d\'affichage en "Alice B."', async () => {
      const user = userEvent.setup()
      await user.clear(screen.getByTestId('account-modal-display-name-input'))
      await user.type(screen.getByTestId('account-modal-display-name-input'), 'Alice B.')
    })

    And('je valide le formulaire de nom d\'affichage', async () => {
      const user = userEvent.setup()
      await user.click(screen.getByTestId('account-modal-display-name-submit-button'))
    })

    Then('je vois un message d\'erreur', async () => {
      const errorMessage = await screen.findByTestId('account-modal-display-name-error')
      expect(errorMessage).toHaveTextContent('Le nom d\'affichage est obligatoire.')
    })
  })
})
