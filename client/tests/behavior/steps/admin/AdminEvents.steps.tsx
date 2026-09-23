import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/admin/AdminEvents.feature', { language: 'fr' })

const allEventsHandler = http.get('*/api/admin/events', () =>
  HttpResponse.json([
    {
      id: 'event-1',
      title: 'Repas de Noël',
      eventDate: '2026-12-24T00:00:00Z',
      location: 'Chez Alice',
      createdByUserId: 'user-1',
      status: 'Planned',
      participantCount: 2,
    },
    {
      id: 'event-3',
      title: 'Anniversaire de Bob',
      eventDate: '2027-02-10T00:00:00Z',
      location: null,
      createdByUserId: 'user-3',
      status: 'Planned',
      participantCount: 1,
    },
  ]),
)

describeFeature(feature, ({ BeforeEachScenario, AfterEachScenario, Scenario }) => {
  BeforeEachScenario(() => {
    server.use(allEventsHandler)
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    // Sans démontage explicite entre scénarios (aucun cleanup RTL global, cf. src/test/setup.ts),
    // le DOM du scénario précédent reste présent en plus de celui du scénario suivant.
    cleanup()
  })

  Scenario('Liste de tous les événements', ({ When, Then, And }) => {
    When('j\'arrive sur la page d\'administration des événements', () => {
      renderApp('/admin/events')
    })

    Then('je vois l\'événement "Repas de Noël" avec 2 participants', async () => {
      expect(await screen.findByTestId('admin-event-list-item-event-1')).toHaveTextContent('Repas de Noël')
      expect(screen.getByTestId('admin-event-list-item-participant-count-event-1')).toHaveTextContent('2 participants')
    })

    And('je vois l\'événement "Anniversaire de Bob" avec 1 participant', () => {
      expect(screen.getByTestId('admin-event-list-item-event-3')).toHaveTextContent('Anniversaire de Bob')
      expect(screen.getByTestId('admin-event-list-item-participant-count-event-3')).toHaveTextContent('1 participant')
    })
  })

  Scenario('Ouverture du détail d\'un événement depuis la liste', ({ When, And, Then }) => {
    When('j\'arrive sur la page d\'administration des événements', () => {
      renderApp('/admin/events')
    })

    And('je clique sur l\'événement "Repas de Noël"', async () => {
      await userEvent.click(await screen.findByTestId('admin-event-list-item-event-1'))
    })

    Then('je vois le détail de l\'événement', async () => {
      expect(await screen.findByTestId('event-detail-title')).toHaveTextContent('Repas de Noël')
    })
  })

  Scenario('Aucun événement existant', ({ Given, When, Then }) => {
    Given('aucun événement n\'existe', () => {
      server.use(http.get('*/api/admin/events', () => HttpResponse.json([])))
    })

    When('j\'arrive sur la page d\'administration des événements', () => {
      renderApp('/admin/events')
    })

    Then('je vois un message m\'indiquant qu\'aucun événement n\'existe', async () => {
      await screen.findByTestId('admin-events-empty-message')
    })
  })

  Scenario('Accès refusé à un utilisateur non administrateur', ({ Given, When, Then }) => {
    Given('je ne suis pas administrateur', () => {
      server.use(
        http.get('*/api/admin/events', () =>
          HttpResponse.json({ title: 'Action non autorisée' }, { status: 403 }),
        ),
      )
    })

    When('j\'arrive sur la page d\'administration des événements', () => {
      renderApp('/admin/events')
    })

    Then('je vois un message m\'indiquant que la page est réservée aux administrateurs', async () => {
      await screen.findByTestId('admin-events-forbidden')
    })
  })
})
