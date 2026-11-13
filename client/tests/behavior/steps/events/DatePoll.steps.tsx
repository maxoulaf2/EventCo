import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect, vi } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/DatePoll.feature', { language: 'fr' })

interface PollOption {
  id: string
  date: string
  voterUserIds: string[]
}

// État du serveur simulé pour un scénario : l'événement en cours de vote et les requêtes reçues.
function createPollServer(currentUserId: 'user-1' | 'user-2') {
  const options: PollOption[] = [
    { id: 'option-1', date: '2027-06-01T17:00:00Z', voterUserIds: ['user-2'] },
    { id: 'option-2', date: '2027-06-02T17:00:00Z', voterUserIds: [] },
  ]
  const state = { eventDate: null as string | null, options, requests: [] as string[] }
  const noContent = () => new HttpResponse(null, { status: 204 })
  const option = (id: unknown) => state.options.find((o) => o.id === id)!

  // Installé par la première étape du scénario : un server.use à la déclaration du scénario serait
  // effacé par le resetHandlers des scénarios exécutés avant lui.
  const install = () =>
    server.use(
      http.get('*/api/auth/me', () =>
        HttpResponse.json({ userId: currentUserId, email: 'moi@example.com', displayName: 'Moi', isAdmin: false, avatarUrl: null }),
      ),
      http.get('*/api/events/event-1', () =>
        HttpResponse.json({
          id: 'event-1',
          title: 'Week-end',
          description: null,
          eventDate: state.eventDate,
          location: 'Lyon',
          imageUrl: null,
          createdByUserId: 'user-1',
          status: 'Planned',
          createdAt: '2026-09-01T00:00:00Z',
          participants: [
            { userId: 'user-1', email: 'orga@example.com', displayName: 'Orga', role: 'Organizer', invitedAt: '2026-09-01T00:00:00Z', participationStatus: 'Attending', avatarUrl: null },
            { userId: 'user-2', email: 'ami@example.com', displayName: 'Ami', role: 'Participant', invitedAt: '2026-09-02T00:00:00Z', participationStatus: 'Unknown', avatarUrl: null },
          ],
          inviteLinkToken: null,
          dateOptions: state.options,
        }),
      ),
      http.put('*/api/events/event-1/date-options/:optionId/vote', ({ params }) => {
        state.requests.push(`vote ${String(params.optionId)}`)
        option(params.optionId).voterUserIds.push(currentUserId)
        return noContent()
      }),
      http.delete('*/api/events/event-1/date-options/:optionId/vote', ({ params }) => {
        state.requests.push(`withdraw ${String(params.optionId)}`)
        const voted = option(params.optionId)
        voted.voterUserIds = voted.voterUserIds.filter((id) => id !== currentUserId)
        return noContent()
      }),
      http.post('*/api/events/event-1/date-options/:optionId/choose', ({ params }) => {
        state.requests.push(`choose ${String(params.optionId)}`)
        state.eventDate = option(params.optionId).date
        return noContent()
      }),
      http.post('*/api/events/event-1/date-options', async ({ request }) => {
        const { date } = (await request.json()) as { date: string }
        state.requests.push(`add ${date}`)
        const added = { id: 'option-3', date, voterUserIds: [] }
        state.options.push(added)
        return HttpResponse.json(added, { status: 201 })
      }),
    )

  return { state, install }
}

async function openEventDetail() {
  renderApp('/events/event-1')
  await screen.findByTestId('date-poll')
}

async function click(testId: string) {
  await userEvent.setup().click(screen.getByTestId(testId))
}

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    vi.restoreAllMocks()
    cleanup()
  })

  Scenario('Création d\'un événement avec plusieurs dates proposées', ({ Given, When, And, Then }) => {
    let requestBody: { eventDate?: string; dateOptions?: string[] } | undefined

    Given('la page de création d\'événement', () => {
      server.use(
        http.post('*/api/events', async ({ request }) => {
          requestBody = (await request.json()) as typeof requestBody
          return HttpResponse.json({ id: 'event-new' }, { status: 201 })
        }),
      )
      renderApp('/events/new')
    })

    When('je choisis de proposer plusieurs dates', () => click('create-event-date-mode-poll'))

    And('je saisis le titre "Week-end" et les dates "2026-12-24" et "2026-12-27" à "19:30"', async () => {
      await userEvent.setup().type(screen.getByTestId('create-event-title-input'), 'Week-end')
      fireEvent.change(screen.getByTestId('create-event-date-option-date-input-0'), { target: { value: '2026-12-24' } })
      fireEvent.change(screen.getByTestId('create-event-date-option-date-input-1'), { target: { value: '2026-12-27' } })
      fireEvent.change(screen.getByTestId('create-event-date-option-time-input-1'), { target: { value: '19:30' } })
    })

    And('je valide la création', () => click('create-event-submit-button'))

    Then(
      'la requête de création contient les dates proposées "2026-12-23T23:00:00.000Z" et "2026-12-27T18:30:00.000Z" sans date fixe',
      async () => {
        await waitFor(() => expect(requestBody).toBeDefined())
        expect(requestBody!.dateOptions).toEqual(['2026-12-23T23:00:00.000Z', '2026-12-27T18:30:00.000Z'])
        expect(requestBody!.eventDate).toBeUndefined()
      },
    )

    And('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
    })
  })

  Scenario('Le formulaire de création propose une date fixe par défaut', ({ Given, Then }) => {
    Given('la page de création d\'événement', () => {
      renderApp('/events/new')
    })

    Then('je vois le champ de date fixe et aucune date proposée', () => {
      expect(screen.getByTestId('create-event-date-input')).toBeInTheDocument()
      expect(screen.queryByTestId('create-event-date-option-date-input-0')).not.toBeInTheDocument()
    })
  })

  Scenario('Un événement en cours de vote sur le tableau de bord', ({ Given, When, Then }) => {
    Given('un de mes événements en cours de vote', () => {
      server.use(
        http.get('*/api/events', () =>
          HttpResponse.json([
            { id: 'event-poll', title: 'Week-end', eventDate: null, location: null, createdByUserId: 'user-2', status: 'Planned', role: 'Participant' },
          ]),
        ),
      )
    })

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    Then('l\'événement en cours de vote porte la mention "Date en cours de vote"', async () => {
      expect(await screen.findByTestId('event-list-item-date-poll-badge-event-poll')).toHaveTextContent('Date en cours de vote')
      // Listé avec les événements à venir, jamais avec les événements passés.
      expect(screen.getByTestId('events-dashboard-list')).toContainElement(screen.getByTestId('event-list-item-event-poll'))
    })
  })

  Scenario('Un participant voit le sondage à la place de sa réponse de présence', ({ Given, When, Then, And }) => {
    const { install } = createPollServer('user-2')

    Given('un événement en cours de vote où je suis simple participant', install)

    When('j\'ouvre le détail de l\'événement', openEventDetail)

    Then('je vois "Date en cours de vote" à la place de la date', () => {
      expect(screen.getByTestId('event-detail-date')).toHaveTextContent('Date en cours de vote')
    })

    And('je vois les 2 dates proposées avec leur nombre de votes', () => {
      expect(screen.getByTestId('date-poll-vote-count-option-1')).toHaveTextContent('1 vote')
      expect(screen.getByTestId('date-poll-vote-count-option-2')).toHaveTextContent('0 vote')
      expect(screen.getByTestId('date-poll-vote-checkbox-option-1')).toBeChecked()
      expect(screen.getByTestId('date-poll-vote-checkbox-option-2')).not.toBeChecked()
    })

    And('la date qui a le plus de votes est mise en avant', () => {
      expect(screen.getByTestId('date-poll-leading-badge-option-1')).toBeInTheDocument()
      expect(screen.queryByTestId('date-poll-leading-badge-option-2')).not.toBeInTheDocument()
    })

    And('je ne peux pas indiquer ma présence', () => {
      expect(screen.queryByTestId('event-detail-participation-status-select')).not.toBeInTheDocument()
    })

    And('je ne vois aucune action d\'organisateur sur le sondage', () => {
      expect(screen.queryByTestId('date-poll-choose-button-option-1')).not.toBeInTheDocument()
      expect(screen.queryByTestId('date-poll-add-form')).not.toBeInTheDocument()
    })
  })

  Scenario('Un participant vote pour une date', ({ Given, When, And, Then }) => {
    const { state, install } = createPollServer('user-2')

    Given('un événement en cours de vote où je suis simple participant', install)

    When('j\'ouvre le détail de l\'événement', openEventDetail)

    And('je coche la deuxième date', () => click('date-poll-vote-checkbox-option-2'))

    Then('mon vote pour la deuxième date est envoyé', async () => {
      await waitFor(() => expect(state.requests).toEqual(['vote option-2']))
    })

    And('la deuxième date compte 1 vote', async () => {
      await waitFor(() => expect(screen.getByTestId('date-poll-vote-count-option-2')).toHaveTextContent('1 vote'))
    })
  })

  Scenario('Un participant retire son vote', ({ Given, When, And, Then }) => {
    const { state, install } = createPollServer('user-2')

    Given('un événement en cours de vote où je suis simple participant', install)

    When('j\'ouvre le détail de l\'événement', openEventDetail)

    And('je décoche la première date', () => click('date-poll-vote-checkbox-option-1'))

    Then('le retrait de mon vote pour la première date est envoyé', async () => {
      await waitFor(() => expect(state.requests).toEqual(['withdraw option-1']))
    })

    And('la première date compte 0 vote', async () => {
      await waitFor(() => expect(screen.getByTestId('date-poll-vote-count-option-1')).toHaveTextContent('0 vote'))
    })
  })

  Scenario('Un organisateur retient une date', ({ Given, When, And, Then }) => {
    const { state, install } = createPollServer('user-1')

    Given('un événement en cours de vote que j\'organise', install)

    When('j\'ouvre le détail de l\'événement', openEventDetail)

    And('je retiens la première date en confirmant', async () => {
      vi.spyOn(window, 'confirm').mockReturnValue(true)
      await click('date-poll-choose-button-option-1')
    })

    Then('la clôture du sondage sur la première date est envoyée', async () => {
      await waitFor(() => expect(state.requests).toEqual(['choose option-1']))
    })

    And('le sondage disparaît au profit de la date retenue', async () => {
      await waitFor(() => expect(screen.queryByTestId('date-poll')).not.toBeInTheDocument())
      expect(screen.getByTestId('event-detail-date')).toHaveTextContent('1 juin')
    })
  })

  Scenario('Un organisateur renonce à retenir une date', ({ Given, When, And, Then }) => {
    const { state, install } = createPollServer('user-1')

    Given('un événement en cours de vote que j\'organise', install)

    When('j\'ouvre le détail de l\'événement', openEventDetail)

    And('je retiens la première date sans confirmer', async () => {
      vi.spyOn(window, 'confirm').mockReturnValue(false)
      await click('date-poll-choose-button-option-1')
    })

    Then('aucune clôture du sondage n\'est envoyée', () => {
      expect(state.requests).toEqual([])
      expect(screen.getByTestId('date-poll')).toBeInTheDocument()
    })
  })

  Scenario('Un organisateur ajoute une date au sondage', ({ Given, When, And, Then }) => {
    const { state, install } = createPollServer('user-1')

    Given('un événement en cours de vote que j\'organise', install)

    When('j\'ouvre le détail de l\'événement', async () => {
      await openEventDetail()
      // Avec seulement deux dates, aucune ne peut être supprimée.
      expect(screen.queryByTestId('date-poll-remove-button-option-1')).not.toBeInTheDocument()
    })

    And('j\'ajoute la date "2027-06-03" à "20:00" au sondage', async () => {
      fireEvent.change(screen.getByTestId('date-poll-add-date-input'), { target: { value: '2027-06-03' } })
      fireEvent.change(screen.getByTestId('date-poll-add-time-input'), { target: { value: '20:00' } })
      await click('date-poll-add-button')
    })

    Then('l\'ajout de la date "2027-06-03T18:00:00.000Z" est envoyé', async () => {
      await waitFor(() => expect(state.requests).toEqual(['add 2027-06-03T18:00:00.000Z']))
    })

    And('je vois 3 dates proposées', async () => {
      await screen.findByTestId('date-poll-option-option-3')
    })

    And('je peux supprimer une date proposée', () => {
      expect(screen.getByTestId('date-poll-remove-button-option-3')).toBeInTheDocument()
    })
  })

  Scenario('Le serveur refuse une action sur le sondage', ({ Given, And, When, Then }) => {
    const { install } = createPollServer('user-1')

    Given('un événement en cours de vote que j\'organise', install)

    And('le serveur refusera le prochain vote', () => {
      server.use(
        http.put('*/api/events/event-1/date-options/:optionId/vote', () =>
          HttpResponse.json({ detail: 'Cet événement n\'a pas de sondage de date ouvert.' }, { status: 400 }),
        ),
      )
    })

    When('j\'ouvre le détail de l\'événement', openEventDetail)

    And('je coche la deuxième date', () => click('date-poll-vote-checkbox-option-2'))

    Then('je vois le message d\'erreur du sondage', async () => {
      expect(await screen.findByTestId('date-poll-error')).toHaveTextContent('Cet événement n\'a pas de sondage de date ouvert.')
    })
  })
})
