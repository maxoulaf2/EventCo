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
    http.post('*/api/events/:id/participants', async ({ request, params }) => {
      const { email } = (await request.json()) as { email: string }

      if (participants.some((p) => p.email === email)) {
        return HttpResponse.json(
          { title: 'Cette personne est déjà invitée à cet événement.' },
          { status: 400 },
        )
      }

      const newParticipant: MockParticipant = {
        userId: `user-${participants.length + 1}`,
        email,
        displayName: email.slice(0, email.indexOf('@')),
        role: 'Participant',
        invitedAt: '2026-09-03T00:00:00Z',
        hasJoined: false,
      }
      participants.push(newParticipant)

      return HttpResponse.json(
        { eventId: params.id, ...newParticipant },
        { status: 201 },
      )
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

  Scenario('Le créateur invite un nouveau participant', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'invite "nouveau@example.com" comme participant', async () => {
      await screen.findByLabelText('Inviter un participant')
      const user = userEvent.setup()
      await user.type(screen.getByLabelText('Inviter un participant'), 'nouveau@example.com')
      await user.click(screen.getByRole('button', { name: 'Inviter' }))
    })

    Then(
      'je vois le participant "nouveau" avec le rôle "Participant" et un badge d\'invitation en attente',
      async () => {
        await waitFor(() => {
          const row = participantRow('nouveau')
          expect(row).toHaveTextContent('Participant')
          expect(row).toHaveTextContent('Invitation en attente')
        })
      },
    )
  })

  Scenario('Invitation d\'une personne déjà invitée', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'invite "ami@example.com" comme participant', async () => {
      await screen.findByLabelText('Inviter un participant')
      const user = userEvent.setup()
      await user.type(screen.getByLabelText('Inviter un participant'), 'ami@example.com')
      await user.click(screen.getByRole('button', { name: 'Inviter' }))
    })

    Then('je vois un message d\'erreur pour l\'invitation', async () => {
      await screen.findByText('Cette personne est déjà invitée à cet événement.')
    })
  })

  Scenario('Un co-organisateur non créateur peut aussi inviter un participant', ({ Given, When, Then }) => {
    Given('je suis un co-organisateur non créateur de cet événement', () => {
      participants[1].role = 'Organizer'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois le formulaire d\'invitation', async () => {
      await screen.findByLabelText('Inviter un participant')
    })
  })

  Scenario('Un simple participant ne voit pas le formulaire d\'invitation', ({ Given, When, Then }) => {
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

    Then('je ne vois pas de formulaire d\'invitation', async () => {
      await screen.findByText('Ami')
      expect(screen.queryByLabelText('Inviter un participant')).not.toBeInTheDocument()
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
