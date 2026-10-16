import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/items/ItemList.feature', { language: 'fr' })

const CATEGORY_KEY: Record<string, string> = {
  Apéro: 'Aperitif',
  Desserts: 'Desserts',
  Boissons: 'Drinks',
  Autre: 'Other',
}

function buildItem(id: string, title: string, category: string, assignedToUserId: string | null = null) {
  return {
    id,
    eventId: 'event-1',
    title,
    quantity: null,
    kind: 'ToBring',
    category,
    assignedToUserId,
    createdAt: '2026-09-01T00:00:00Z',
  }
}

function buildItems() {
  return [
    // Dans l'ordre où le backend les renvoie (catégorie, à prendre d'abord, ordre alphabétique).
    buildItem('item-1', 'Bûche au chocolat', 'Desserts'),
    buildItem('item-3', 'Vin rouge', 'Drinks'),
    buildItem('item-2', 'Réserver la salle', 'Other'),
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

  function category(label: string) {
    return within(screen.getByTestId(`item-list-category-${CATEGORY_KEY[label]}`))
  }

  Scenario('Articles regroupés par catégorie, dans l\'ordre fourni par l\'API', ({ When, Then, And }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois l\'article "Bûche au chocolat" dans la catégorie "Desserts"', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(category('Desserts').getByTestId('item-row-item-1')).toBeInTheDocument()
    })

    And('je vois l\'article "Vin rouge" dans la catégorie "Boissons"', () => {
      expect(category('Boissons').getByTestId('item-row-item-3')).toBeInTheDocument()
    })

    And('je vois l\'article "Réserver la salle" dans la catégorie "Autre"', () => {
      expect(category('Autre').getByTestId('item-row-item-2')).toBeInTheDocument()
    })

    And('les catégories sont affichées dans l\'ordre "Desserts", "Boissons", "Autre"', () => {
      const displayed = screen.getAllByTestId(/^item-list-category-heading-/).map((heading) => heading.dataset.testid)
      expect(displayed).toEqual([
        'item-list-category-heading-Desserts',
        'item-list-category-heading-Drinks',
        'item-list-category-heading-Other',
      ])
    })

    And('la catégorie "Boissons" est affichée avec son libellé', async () => {
      await waitFor(() => expect(screen.getByTestId('item-list-category-heading-Drinks')).toHaveTextContent('Boissons · 1'))
    })
  })

  Scenario('Les catégories sans article ne sont pas affichées', ({ When, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne vois pas la catégorie "Apéro"', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(screen.queryByTestId('item-list-category-Aperitif')).not.toBeInTheDocument()
    })
  })


  Scenario('Nombre d\'articles restant à prendre', ({ Given, When, Then }) => {
    Given('l\'article "Bûche au chocolat" est assigné à un participant', () => {
      items[0].assignedToUserId = 'user-2'
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois qu\'il reste 2 articles à prendre', async () => {
      expect(await screen.findByTestId('item-list-to-take-count')).toHaveTextContent('2 à prendre')
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
