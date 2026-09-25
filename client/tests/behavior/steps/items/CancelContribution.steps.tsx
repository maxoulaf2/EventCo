import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/items/CancelContribution.feature', { language: 'fr' })

const CONTRIBUTION = {
  id: 'item-1',
  eventId: 'event-1',
  title: 'Chips',
  quantity: null,
  kind: 'Contribution',
  assignedToUserId: 'user-2',
  createdAt: '2026-09-01T00:00:00Z',
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  BeforeEachScenario(() => {
    server.use(
      http.get('*/api/events/:id/items', () => HttpResponse.json([CONTRIBUTION])),
      http.delete('*/api/events/:id/items/:itemId', () => new HttpResponse(null, { status: 204 })),
    )
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  function actAsContributor() {
    server.use(
      http.get('*/api/auth/me', () =>
        HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
      ),
    )
  }

  async function cancel() {
    const user = userEvent.setup()
    await user.click(await screen.findByTestId('item-row-cancel-button-item-1'))
  }

  Scenario('La personne qui apporte un article l\'annule', ({ Given, When, Then }) => {
    Given('je suis un simple participant qui apporte l\'article "Chips"', () => {
      actAsContributor()
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois pas de bouton de désassignation pour l\'article "Chips"', async () => {
      await screen.findByTestId('item-row-cancel-button-item-1')
      expect(screen.queryByTestId('item-row-unassign-button-item-1')).not.toBeInTheDocument()
    })

    When('j\'annule l\'article "Chips"', async () => {
      await cancel()
    })

    Then('je ne vois plus l\'article "Chips"', async () => {
      await waitFor(() => expect(screen.queryByTestId('item-row-item-1')).not.toBeInTheDocument())
    })
  })

  Scenario('Un organisateur ne peut ni désassigner ni annuler l\'article apporté par un participant', ({ Given, When, Then }) => {
    Given('l\'article "Chips" est apporté par un participant', () => {})

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois ni bouton de désassignation ni bouton d\'annulation pour l\'article "Chips"', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(screen.queryByTestId('item-row-unassign-button-item-1')).not.toBeInTheDocument()
      expect(screen.queryByTestId('item-row-cancel-button-item-1')).not.toBeInTheDocument()
    })
  })

  Scenario('Erreur lors de l\'annulation d\'un article apporté', ({ Given, And, When, Then }) => {
    Given('je suis un simple participant qui apporte l\'article "Chips"', () => {
      actAsContributor()
    })

    And('l\'annulation de l\'article "Chips" échoue', () => {
      server.use(
        http.delete('*/api/events/:id/items/:itemId', () =>
          HttpResponse.json({ title: 'Impossible d\'annuler cet article.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'annule l\'article "Chips"', async () => {
      await cancel()
    })

    Then('je vois toujours l\'article "Chips" dans la section "Assignés"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('item-list-section-assigned')).getByTestId('item-row-item-1')).toBeInTheDocument(),
      )
    })
  })
})
