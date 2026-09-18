import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { screen } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/EventsDashboard.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
  })

  Scenario('Liste de mes événements, dont un que j\'organise', ({ When, Then, And }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('je vois l\'événement "Repas de Noël" avec un badge d\'organisateur·ice', async () => {
      await screen.findByTestId('event-list-item-event-1')
      expect(screen.getByTestId('event-list-item-event-1')).toHaveTextContent('Repas de Noël')
      expect(screen.getByTestId('event-list-item-organizer-badge-event-1')).toBeInTheDocument()
    })

    And('je vois l\'événement "Weekend au ski" sans badge d\'organisateur·ice', () => {
      expect(screen.getByTestId('event-list-item-event-2')).toHaveTextContent('Weekend au ski')
      expect(screen.queryByTestId('event-list-item-organizer-badge-event-2')).not.toBeInTheDocument()
    })
  })

  Scenario('Aucun événement', ({ Given, When, Then }) => {
    Given('je n\'ai encore aucun événement', () => {
      server.use(http.get('*/api/events', () => HttpResponse.json([])))
    })

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('je vois un message m\'indiquant que je ne participe à aucun événement', async () => {
      await screen.findByTestId('events-dashboard-empty-message')
    })
  })

  Scenario('Session expirée', ({ Given, When, Then }) => {
    Given('ma session a expiré', () => {
      server.use(
        http.get('*/api/events', () => new HttpResponse(null, { status: 401 })),
        http.get('*/api/auth/me', () => new HttpResponse(null, { status: 401 })),
      )
    })

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('je suis redirigé vers la page de connexion', async () => {
      await screen.findByTestId('login-page-title')
    })
  })
})
