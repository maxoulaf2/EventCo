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

  Scenario('Liste de mes événements, dont une invitation en attente', ({ When, Then, And }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('je vois l\'événement "Repas de Noël" sans badge d\'invitation en attente', async () => {
      await screen.findByText('Repas de Noël')
      const item = screen.getByText('Repas de Noël').closest('li')!
      expect(item).not.toHaveTextContent('Invitation en attente')
    })

    And('je vois l\'événement "Weekend au ski" avec un badge d\'invitation en attente', () => {
      const item = screen.getByText('Weekend au ski').closest('li')!
      expect(item).toHaveTextContent('Invitation en attente')
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
      await screen.findByText(/vous ne participez encore à aucun événement/i)
    })
  })

  Scenario('Session expirée', ({ Given, When, Then }) => {
    Given('ma session a expiré', () => {
      server.use(http.get('*/api/events', () => new HttpResponse(null, { status: 401 })))
    })

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('je suis redirigé vers la page de connexion', async () => {
      await screen.findByRole('heading', { name: 'EventCo' })
    })
  })
})
