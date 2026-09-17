import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { fireEvent, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/CreateEvent.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Background, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
  })

  Background(({ Given }) => {
    Given('je suis sur la page de création d\'événement', () => {
      renderApp('/events/new')
    })
  })

  Scenario('Titre et date valides', ({ When, Then }) => {
    When(
      'je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire',
      async () => {
        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
    })
  })

  Scenario('Le serveur refuse la création', ({ And, When, Then }) => {
    And('le serveur refusera la prochaine création d\'événement', () => {
      server.use(
        http.post('*/api/events', () =>
          HttpResponse.json({ detail: 'La date est obligatoire.' }, { status: 400 }),
        ),
      )
    })

    When(
      'je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire',
      async () => {
        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('je vois un message d\'erreur sur le formulaire', async () => {
      const errorMessage = await screen.findByTestId('create-event-error')
      expect(errorMessage).toHaveTextContent('La date est obligatoire.')
    })

    And('je reste sur la page de création d\'événement', () => {
      expect(screen.getByTestId('create-event-submit-button')).toBeInTheDocument()
    })
  })
})
