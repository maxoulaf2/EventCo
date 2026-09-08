import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/EventDetail.feature', { language: 'fr' })

interface MockParticipant {
  userId: string
  email: string
  displayName: string
  role: 'Organizer' | 'Participant'
  invitedAt: string
  hasJoined: boolean
}

function eventDetailHandlers(participants: MockParticipant[]) {
  return [
    http.get('*/api/events/:id', ({ params }) =>
      HttpResponse.json({
        id: params.id,
        title: 'Repas de Noël',
        description: null,
        eventDate: '2026-12-24T00:00:00Z',
        location: 'Chez Alice',
        createdByUserId: 'user-1',
        status: 'Planned',
        createdAt: '2026-09-01T00:00:00Z',
        participants,
      }),
    ),
    http.post('*/api/events/:id/participants/:userId/promote', ({ params }) => {
      const participant = participants.find((p) => p.userId === params.userId)
      if (participant) {
        participant.role = 'Organizer'
      }
      return new HttpResponse(null, { status: 204 })
    }),
  ]
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let participants: MockParticipant[]

  AfterEachScenario(() => {
    server.resetHandlers()
    // Plusieurs scénarios de ce fichier affichent tous un participant "Ami" : sans
    // démontage explicite entre scénarios (aucun cleanup RTL global, cf. src/test/setup.ts),
    // le DOM du scénario précédent reste présent et rend "Ami" ambigu pour `findByText`.
    cleanup()
  })

  BeforeEachScenario(() => {
    participants = [
      {
        userId: 'user-1',
        email: 'test@example.com',
        displayName: 'Test',
        role: 'Organizer',
        invitedAt: '2026-09-01T00:00:00Z',
        hasJoined: true,
      },
      {
        userId: 'user-2',
        email: 'ami@example.com',
        displayName: 'Ami',
        role: 'Participant',
        invitedAt: '2026-09-02T00:00:00Z',
        hasJoined: false,
      },
    ]
    server.use(...eventDetailHandlers(participants))
  })

  function participantRow(displayName: string) {
    return screen.getByText(displayName).closest('li')!
  }

  Scenario('Affichage des informations et des participants', ({ When, Then, And }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois le titre "Repas de Noël" et le lieu "Chez Alice"', async () => {
      await screen.findByRole('heading', { name: 'Repas de Noël' })
      expect(screen.getByText(/Chez Alice/)).toBeInTheDocument()
    })

    And('je vois le participant "Test" avec le rôle "Co-organisateur"', () => {
      expect(participantRow('Test')).toHaveTextContent('Co-organisateur')
    })

    And('je vois le participant "Ami" avec le rôle "Participant" et un badge d\'invitation en attente', () => {
      const row = participantRow('Ami')
      expect(row).toHaveTextContent('Participant')
      expect(row).toHaveTextContent('Invitation en attente')
    })
  })

  Scenario('Le créateur promeut un participant en co-organisateur', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je clique sur "Promouvoir co-organisateur" pour "Ami"', async () => {
      const row = await screen.findByText('Ami').then((el) => el.closest('li')!)
      const user = userEvent.setup()
      await user.click(row.querySelector('button')!)
    })

    Then('je vois le participant "Ami" avec le rôle "Co-organisateur"', async () => {
      await waitFor(() => expect(participantRow('Ami')).toHaveTextContent('Co-organisateur'))
    })
  })

  Scenario('Un participant qui n\'est pas le créateur ne voit aucune action', ({ Given, When, Then }) => {
    Given('je ne suis pas le créateur de cet événement', () => {
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois aucun bouton pour promouvoir ou rétrograder un participant', async () => {
      await screen.findByText('Ami')
      expect(screen.queryByRole('button', { name: /promouvoir|rétrograder/i })).not.toBeInTheDocument()
    })
  })

  Scenario('Session expirée', ({ Given, When, Then }) => {
    Given('ma session a expiré', () => {
      server.use(http.get('*/api/events/:id', () => new HttpResponse(null, { status: 401 })))
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je suis redirigé vers la page de connexion', async () => {
      await screen.findByRole('heading', { name: 'EventCo' })
    })
  })
})
