import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, within } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/items/ItemList.feature', { language: 'fr' })

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
      kind: 'ToBring',
      assignedToUserId: null as string | null,
      createdAt: '2026-09-01T00:00:00Z',
    },
    {
      id: 'item-2',
      eventId: 'event-1',
      title: 'Réserver la salle',
      quantity: null,
      kind: 'ToBring',
      assignedToUserId: null as string | null,
      createdAt: '2026-09-02T00:00:00Z',
    },
  ]
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let items = buildItems()

  BeforeEachScenario(() => {
    items = buildItems()
    server.use(http.get('*/api/events/:id/items', () => HttpResponse.json(items)))
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  function section(label: string) {
    return within(screen.getByTestId(`item-list-section-${SECTION_KEY[label]}`))
  }

  Scenario('Affichage des articles à prendre', ({ When, Then, And }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois l\'article "Bûche au chocolat" dans la section "À prendre"', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(section('À prendre').getByTestId('item-row-item-1')).toBeInTheDocument()
    })

    And('je vois l\'article "Réserver la salle" dans la section "À prendre"', () => {
      expect(section('À prendre').getByTestId('item-row-item-2')).toBeInTheDocument()
    })
  })

  Scenario('Articles à prendre et assignés affichés ensemble', ({ Given, When, And, Then }) => {
    Given('l\'article "Bûche au chocolat" est assigné à un participant', () => {
      items[0].assignedToUserId = 'user-2'
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois l\'article "Bûche au chocolat" dans la section "Assignés"', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(section('Assignés').getByTestId('item-row-item-1')).toBeInTheDocument()
    })

    And('je vois l\'article "Réserver la salle" dans la section "À prendre"', () => {
      expect(section('À prendre').getByTestId('item-row-item-2')).toBeInTheDocument()
    })
  })

  Scenario('Aucun article dans une section', ({ Given, When, Then }) => {
    Given('cet événement n\'a aucun article assigné', () => {
      // Les deux articles par défaut (buildItems) sont déjà non assignés.
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois un message indiquant qu\'il n\'y a aucun article dans la section "Assignés"', async () => {
      await screen.findByTestId('item-list-section-empty-message-assigned')
    })
  })

  Scenario('Événement sans article', ({ Given, When, Then }) => {
    Given('cet événement n\'a aucun article', () => {
      server.use(http.get('*/api/events/:id/items', () => HttpResponse.json([])))
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois un message indiquant qu\'il n\'y a aucun article pour le moment', async () => {
      await screen.findByTestId('item-list-empty-message')
    })
  })
})
