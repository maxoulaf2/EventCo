import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/items/UnassignItem.feature', { language: 'fr' })

const SECTION_KEY: Record<string, string> = {
  'À prendre': 'todo',
  Assignés: 'assigned',
}

function buildItems() {
  return [
    {
      id: 'item-1',
      eventId: 'event-1',
      title: 'Bûche au chocolat',
      quantity: '1',
      assignedToUserId: null as string | null,
      createdAt: '2026-09-01T00:00:00Z',
    },
    {
      id: 'item-2',
      eventId: 'event-1',
      title: 'Réserver la salle',
      quantity: null,
      assignedToUserId: null as string | null,
      createdAt: '2026-09-02T00:00:00Z',
    },
  ]
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let items = buildItems()

  BeforeEachScenario(() => {
    items = buildItems()
    server.use(
      http.get('*/api/events/:id/items', () => HttpResponse.json(items)),
      http.post('*/api/events/:id/items/:itemId/unassign', () => new HttpResponse(null, { status: 204 })),
    )
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  function section(label: string) {
    return within(screen.getByTestId(`item-list-section-${SECTION_KEY[label]}`))
  }

  async function unassign(itemId: string) {
    await screen.findByTestId(`item-row-unassign-button-${itemId}`)
    const user = userEvent.setup()
    await user.click(screen.getByTestId(`item-row-unassign-button-${itemId}`))
  }

  Scenario('Le créateur désassigne l\'article d\'un autre participant', ({ Given, When, And, Then }) => {
    Given('l\'article "Bûche au chocolat" est assigné à un participant', () => {
      items[0].assignedToUserId = 'user-2'
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je désassigne l\'article "Bûche au chocolat"', async () => {
      await unassign('item-1')
    })

    Then('je vois l\'article "Bûche au chocolat" dans la section "À prendre"', async () => {
      await waitFor(() => expect(section('À prendre').getByTestId('item-row-item-1')).toBeInTheDocument())
    })
  })

  Scenario('Un participant se désassigne de son propre article', ({ Given, When, And, Then }) => {
    Given('je suis un simple participant assigné à l\'article "Bûche au chocolat"', () => {
      items[0].assignedToUserId = 'user-2'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je désassigne l\'article "Bûche au chocolat"', async () => {
      await unassign('item-1')
    })

    Then('je vois l\'article "Bûche au chocolat" dans la section "À prendre"', async () => {
      await waitFor(() => expect(section('À prendre').getByTestId('item-row-item-1')).toBeInTheDocument())
    })
  })

  Scenario('Un participant ne peut pas désassigner l\'article d\'un autre', ({ Given, When, Then }) => {
    Given('je suis un simple participant et que l\'article "Réserver la salle" est assigné à quelqu\'un d\'autre', () => {
      items[1].assignedToUserId = 'user-3'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois pas de bouton de désassignation pour l\'article "Réserver la salle"', async () => {
      await screen.findByTestId('item-row-item-2')
      expect(screen.queryByTestId('item-row-unassign-button-item-2')).not.toBeInTheDocument()
    })
  })

  Scenario('Erreur lors de la désassignation d\'un article', ({ Given, And, When, Then }) => {
    Given('l\'article "Bûche au chocolat" est assigné à un participant', () => {
      items[0].assignedToUserId = 'user-2'
    })

    And('la désassignation de l\'article "Bûche au chocolat" échoue', () => {
      server.use(
        http.post('*/api/events/:id/items/:itemId/unassign', () =>
          HttpResponse.json({ title: 'Impossible de désassigner cet article.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je désassigne l\'article "Bûche au chocolat"', async () => {
      await unassign('item-1')
    })

    Then('je vois l\'article "Bûche au chocolat" dans la section "Assignés"', async () => {
      await waitFor(() => expect(section('Assignés').getByTestId('item-row-item-1')).toBeInTheDocument())
    })
  })
})
