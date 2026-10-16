import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { act, cleanup, screen, waitFor, within } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { expect, vi } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { expectItemAssigned } from '../../../../src/test/items'
import { renderApp } from '../../../../src/test/render'

// Le Hub SignalR (websocket) n'est pas interceptable par MSW comme le sont les appels `fetch` des
// autres tests de comportement : on simule ici directement la connexion (`@microsoft/signalr`) pour
// déclencher les messages du Hub `EventHub` (cf. `useItemRealtime`) sans back réel.
const registeredHandlers = new Map<string, (payload: unknown) => void>()
const invokeMock = vi.fn().mockResolvedValue(undefined)
let reconnectedCallback: (() => void) | null = null

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: vi.fn().mockImplementation(function HubConnectionBuilder(this: unknown) {
    return {
      withUrl: vi.fn().mockReturnThis(),
      withAutomaticReconnect: vi.fn().mockReturnThis(),
      configureLogging: vi.fn().mockReturnThis(),
      build: vi.fn().mockReturnValue({
        on: (event: string, callback: (payload: unknown) => void) => registeredHandlers.set(event, callback),
        onreconnected: (callback: () => void) => {
          reconnectedCallback = callback
        },
        start: vi.fn().mockResolvedValue(undefined),
        stop: vi.fn().mockResolvedValue(undefined),
        invoke: invokeMock,
      }),
    }
  }),
  LogLevel: { Warning: 2 },
}))

const feature = await loadFeature('tests/behavior/features/items/ItemListRealtime.feature', { language: 'fr' })

const defaultItems = [
  {
    id: 'item-1',
    eventId: 'event-1',
    title: 'Bûche au chocolat',
    quantity: '1',
    kind: 'ToBring',
    category: 'Desserts',
    assignedToUserId: null,
    createdAt: '2026-09-01T00:00:00Z',
  },
  {
    id: 'item-2',
    eventId: 'event-1',
    title: 'Réserver la salle',
    quantity: null,
    kind: 'ToBring',
    category: 'Other',
    assignedToUserId: null,
    createdAt: '2026-09-02T00:00:00Z',
  },
]

function emit(event: string, payload: unknown) {
  const handler = registeredHandlers.get(event)
  if (!handler) {
    throw new Error(`Aucun abonnement au message Hub "${event}"`)
  }
  act(() => handler(payload))
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  // Un message du Hub fait recharger la liste : chaque scénario modifie d'abord ce que renvoie le « serveur »,
  // comme l'aurait fait l'autre participant, puis émet le message.
  let serverItems: object[] = defaultItems

  BeforeEachScenario(() => {
    registeredHandlers.clear()
    reconnectedCallback = null
    invokeMock.mockClear()
    serverItems = defaultItems
    server.use(http.get('*/api/events/:id/items', () => HttpResponse.json(serverItems)))
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Scenario('Un article créé par un autre participant apparaît automatiquement', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('un autre participant crée l\'article "Acheter des bougies"', async () => {
      await screen.findByTestId('item-row-item-1')
      const created = {
        id: 'item-3',
        eventId: 'event-1',
        title: 'Acheter des bougies',
        quantity: null,
        kind: 'ToBring',
        category: 'Equipment',
        assignedToUserId: null,
        createdAt: '2026-09-03T00:00:00Z',
      }
      serverItems = [defaultItems[0], created, defaultItems[1]]
      emit('ItemCreated', { ...created, itemId: created.id })
    })

    Then('je vois l\'article "Acheter des bougies"', async () => {
      await screen.findByTestId('item-row-item-3')
      expect(screen.getByTestId('item-row-title-item-3')).toHaveTextContent('Acheter des bougies')
    })
  })

  Scenario('L\'assignation d\'un article se met à jour automatiquement', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('un autre participant assigne l\'article "Bûche au chocolat"', async () => {
      await screen.findByTestId('item-row-item-1')
      serverItems = [{ ...defaultItems[0], assignedToUserId: 'user-2' }, defaultItems[1]]
      emit('ItemAssigned', { ...serverItems[0], itemId: defaultItems[0].id })
    })

    Then('je vois l\'article "Bûche au chocolat" assigné', async () => {
      await expectItemAssigned('item-1')
    })
  })

  Scenario('La correction de la catégorie d\'un article se met à jour automatiquement', ({ When, And, Then }) => {
    // Article déjà pris : seul un article pris est rangé dans sa catégorie.
    const takenItem = { ...defaultItems[1], assignedToUserId: 'user-2' }

    When('j\'arrive sur le détail de l\'événement', () => {
      serverItems = [defaultItems[0], takenItem]
      renderApp('/events/event-1')
    })

    And('un autre participant range l\'article "Réserver la salle" dans la catégorie "Matériel et déco"', async () => {
      await screen.findByTestId('item-row-item-2')
      serverItems = [defaultItems[0], { ...takenItem, category: 'Equipment' }]
      emit('ItemCategoryCorrected', { ...serverItems[1], itemId: defaultItems[1].id })
    })

    Then('je vois l\'article "Réserver la salle" dans la catégorie "Matériel et déco"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('item-list-category-Equipment')).getByTestId('item-row-item-2')).toBeInTheDocument(),
      )
    })
  })

  Scenario('Un article supprimé par un autre participant disparaît automatiquement', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('un autre participant supprime l\'article "Réserver la salle"', async () => {
      await screen.findByTestId('item-row-item-2')
      serverItems = [defaultItems[0]]
      emit('ItemDeleted', { eventId: 'event-1', itemId: 'item-2' })
    })

    Then('je ne vois plus l\'article "Réserver la salle"', async () => {
      await waitFor(() => expect(screen.queryByTestId('item-row-item-2')).not.toBeInTheDocument())
    })
  })

  Scenario('La version de l\'API est vérifiée à la reconnexion temps réel', ({ When, And, Then }) => {
    let versionRequested = false

    When('j\'arrive sur le détail de l\'événement', () => {
      server.use(
        http.get('*/api/version', () => {
          versionRequested = true
          return HttpResponse.json({ version: 'dev' })
        }),
      )
      renderApp('/events/event-1')
    })

    And('la connexion temps réel est rétablie après une coupure', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(reconnectedCallback).not.toBeNull()
      reconnectedCallback!()
    })

    Then('la version de l\'API est vérifiée', async () => {
      await waitFor(() => expect(versionRequested).toBe(true))
    })
  })
})
