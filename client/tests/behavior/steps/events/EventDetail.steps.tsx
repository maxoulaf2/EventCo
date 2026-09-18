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
  participationStatus: 'Unknown' | 'Attending' | 'NotAttending'
}

function eventDetailHandlers(participants: MockParticipant[], currentUserId: { value: string }) {
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
    http.put('*/api/events/:id/participation-status', async ({ request }) => {
      const { status } = (await request.json()) as { status: MockParticipant['participationStatus'] }
      const participant = participants.find((p) => p.userId === currentUserId.value)
      if (participant) {
        participant.participationStatus = status
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
        participationStatus: 'Unknown',
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
  let currentUserId: { value: string }

  AfterEachScenario(() => {
    server.resetHandlers()
    // Sans démontage explicite entre scénarios (aucun cleanup RTL global, cf. src/test/setup.ts),
    // le DOM du scénario précédent reste présent en plus de celui du scénario suivant.
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
        // Le créateur vient par définition (cf. Event.Create côté Domain) : jamais "Unknown" pour lui.
        participationStatus: 'Attending',
      },
      {
        userId: 'user-2',
        email: 'ami@example.com',
        displayName: 'Ami',
        role: 'Participant',
        invitedAt: '2026-09-02T00:00:00Z',
        participationStatus: 'Unknown',
      },
    ]
    currentUserId = { value: 'user-1' }
    server.use(...eventDetailHandlers(participants, currentUserId))
  })

  Scenario('Affichage des informations et des participants', ({ When, Then, And }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois le titre "Repas de Noël" et le lieu "Chez Alice"', async () => {
      await screen.findByTestId('event-detail-title')
      expect(screen.getByTestId('event-detail-title')).toHaveTextContent('Repas de Noël')
      expect(screen.getByTestId('event-detail-date-location')).toHaveTextContent('Chez Alice')
    })

    And('je vois le participant "Test" avec le rôle "Co-organisateur"', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      expect(screen.getByTestId('participant-role-badge-user-1')).toHaveTextContent('Co-organisateur')
    })

    And('je vois le participant "Ami" avec le rôle "Participant" et le statut de participation "Sans réponse"', () => {
      expect(screen.getByTestId('participant-role-badge-user-2')).toHaveTextContent('Participant')
      expect(screen.getByTestId('participant-participation-badge-user-2')).toHaveTextContent('Sans réponse')
    })
  })

  Scenario('La liste des participants est masquée tant que la modale n\'est pas ouverte', ({ When, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois pas la liste des participants', async () => {
      await screen.findByTestId('event-detail-participants-button')
      expect(screen.queryByTestId('event-detail-participants-list')).not.toBeInTheDocument()
    })
  })

  Scenario('Ouverture et fermeture de la modale des participants', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'ouvre la modale des participants', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
    })

    Then('je vois la liste des participants', async () => {
      await screen.findByTestId('event-detail-participants-list')
    })

    When('je ferme la modale des participants', async () => {
      const user = userEvent.setup()
      await user.click(screen.getByTestId('participants-modal-close-button'))
    })

    Then('je ne vois pas la liste des participants', () => {
      expect(screen.queryByTestId('event-detail-participants-list')).not.toBeInTheDocument()
    })
  })

  Scenario('Le créateur promeut un participant en co-organisateur', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je clique sur "Promouvoir co-organisateur" pour "Ami"', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      const promoteButton = await screen.findByTestId('participant-promote-button-user-2')
      await user.click(promoteButton)
    })

    Then('je vois le participant "Ami" avec le rôle "Co-organisateur"', async () => {
      await waitFor(() => expect(screen.getByTestId('participant-role-badge-user-2')).toHaveTextContent('Co-organisateur'))
    })
  })

  Scenario('Un participant qui n\'est pas le créateur ne voit aucune action', ({ Given, When, Then }) => {
    Given('je ne suis pas le créateur de cet événement', () => {
      currentUserId.value = 'user-2'
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
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      await screen.findByTestId('participant-row-user-2')
      expect(screen.queryByTestId('participant-promote-button-user-2')).not.toBeInTheDocument()
      expect(screen.queryByTestId('participant-demote-button-user-2')).not.toBeInTheDocument()
    })
  })

  Scenario('Le créateur invite un nouveau participant', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'invite "nouveau@example.com" comme participant', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      await screen.findByTestId('invite-participant-email-input')
      await user.type(screen.getByTestId('invite-participant-email-input'), 'nouveau@example.com')
      await user.click(screen.getByTestId('invite-participant-submit-button'))
    })

    Then(
      'je vois le participant "nouveau" avec le rôle "Participant" et le statut de participation "Sans réponse"',
      async () => {
        await waitFor(() => {
          expect(screen.getByTestId('participant-role-badge-user-3')).toHaveTextContent('Participant')
          expect(screen.getByTestId('participant-participation-badge-user-3')).toHaveTextContent('Sans réponse')
        })
      },
    )
  })

  Scenario('Invitation d\'une personne déjà invitée', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'invite "ami@example.com" comme participant', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      await screen.findByTestId('invite-participant-email-input')
      await user.type(screen.getByTestId('invite-participant-email-input'), 'ami@example.com')
      await user.click(screen.getByTestId('invite-participant-submit-button'))
    })

    Then('je vois un message d\'erreur pour l\'invitation', async () => {
      const errorMessage = await screen.findByTestId('invite-participant-error')
      expect(errorMessage).toHaveTextContent('Cette personne est déjà invitée à cet événement.')
    })
  })

  Scenario('Un co-organisateur non créateur peut aussi inviter un participant', ({ Given, When, Then }) => {
    Given('je suis un co-organisateur non créateur de cet événement', () => {
      participants[1].role = 'Organizer'
      currentUserId.value = 'user-2'
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
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      await screen.findByTestId('invite-participant-form')
    })
  })

  Scenario('Un simple participant ne voit pas le formulaire d\'invitation', ({ Given, When, Then }) => {
    Given('je ne suis pas le créateur de cet événement', () => {
      currentUserId.value = 'user-2'
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
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participants-button'))
      await screen.findByTestId('participant-row-user-2')
      expect(screen.queryByTestId('invite-participant-form')).not.toBeInTheDocument()
    })
  })

  Scenario('Session expirée', ({ Given, When, Then }) => {
    Given('ma session a expiré', () => {
      server.use(
        http.get('*/api/events/:id', () => new HttpResponse(null, { status: 401 })),
        http.get('*/api/auth/me', () => new HttpResponse(null, { status: 401 })),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je suis redirigé vers la page de connexion', async () => {
      await screen.findByTestId('login-page-title')
    })
  })

  Scenario('Statut de participation par défaut pour un simple participant', ({ Given, When, Then }) => {
    Given('je ne suis pas le créateur de cet événement', () => {
      currentUserId.value = 'user-2'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('mon statut de participation affiché est "Je ne sais pas encore si je viens"', async () => {
      const trigger = await screen.findByTestId('event-detail-participation-status-select')
      expect(trigger).toHaveTextContent('Je ne sais pas encore si je viens')
    })
  })

  Scenario('J\'indique que je viens', ({ Given, When, And, Then }) => {
    Given('je ne suis pas le créateur de cet événement', () => {
      currentUserId.value = 'user-2'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'indique le statut de participation "Je viens !"', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('event-detail-participation-status-select'))
      await user.click(await screen.findByTestId('event-detail-participation-status-select-option-Attending'))
    })

    Then('mon statut de participation affiché est "Je viens !"', async () => {
      await waitFor(() => {
        const trigger = screen.getByTestId('event-detail-participation-status-select')
        expect(trigger).toHaveTextContent('Je viens !')
      })
    })
  })

  Scenario('Le formulaire de participation n\'est pas affiché pour un utilisateur non participant', ({ Given, When, Then }) => {
    Given('je ne participe pas à cet événement', () => {
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-3', email: 'exterieur@example.com', displayName: 'Exterieur' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois pas de formulaire de participation', async () => {
      await screen.findByTestId('event-detail-title')
      expect(screen.queryByTestId('event-detail-participation-status-select')).not.toBeInTheDocument()
    })
  })

  Scenario('Le créateur ne voit pas le formulaire de participation', ({ When, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois pas de formulaire de participation', async () => {
      await screen.findByTestId('event-detail-title')
      expect(screen.queryByTestId('event-detail-participation-status-select')).not.toBeInTheDocument()
    })
  })
})
