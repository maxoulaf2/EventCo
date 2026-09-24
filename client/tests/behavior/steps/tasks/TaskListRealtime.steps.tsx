import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { act, cleanup, screen, waitFor, within } from '@testing-library/react'
import { HttpResponse, http } from 'msw'
import { expect, vi } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

// Le Hub SignalR (websocket) n'est pas interceptable par MSW comme le sont les appels `fetch` des
// autres tests de comportement : on simule ici directement la connexion (`@microsoft/signalr`) pour
// déclencher les messages du Hub `EventHub` (cf. `useTaskRealtime`) sans back réel.
const registeredHandlers = new Map<string, (payload: unknown) => void>()
const invokeMock = vi.fn().mockResolvedValue(undefined)

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: vi.fn().mockImplementation(function HubConnectionBuilder(this: unknown) {
    return {
      withUrl: vi.fn().mockReturnThis(),
      withAutomaticReconnect: vi.fn().mockReturnThis(),
      configureLogging: vi.fn().mockReturnThis(),
      build: vi.fn().mockReturnValue({
        on: (event: string, callback: (payload: unknown) => void) => registeredHandlers.set(event, callback),
        start: vi.fn().mockResolvedValue(undefined),
        stop: vi.fn().mockResolvedValue(undefined),
        invoke: invokeMock,
      }),
    }
  }),
  LogLevel: { Warning: 2 },
}))

const feature = await loadFeature('tests/behavior/features/tasks/TaskListRealtime.feature', { language: 'fr' })

const defaultTasks = [
  {
    id: 'task-1',
    eventId: 'event-1',
    title: 'Bûche au chocolat',
    quantity: '1',
    assignedToUserId: null,
    createdAt: '2026-09-01T00:00:00Z',
  },
  {
    id: 'task-2',
    eventId: 'event-1',
    title: 'Réserver la salle',
    quantity: null,
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
  BeforeEachScenario(() => {
    registeredHandlers.clear()
    invokeMock.mockClear()
    server.use(http.get('*/api/events/:id/tasks', () => HttpResponse.json(defaultTasks)))
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Scenario('Une tâche créée par un autre participant apparaît automatiquement', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('un autre participant crée la tâche "Acheter des bougies"', async () => {
      await screen.findByTestId('task-item-task-1')
      emit('TaskCreated', {
        taskId: 'task-3',
        eventId: 'event-1',
        title: 'Acheter des bougies',
        quantity: null,
        assignedToUserId: null,
        createdAt: '2026-09-03T00:00:00Z',
      })
    })

    Then('je vois la tâche "Acheter des bougies"', async () => {
      await screen.findByTestId('task-item-task-3')
      expect(screen.getByTestId('task-item-title-task-3')).toHaveTextContent('Acheter des bougies')
    })
  })

  Scenario('L\'assignation d\'une tâche se met à jour automatiquement', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('un autre participant assigne la tâche "Bûche au chocolat"', async () => {
      await screen.findByTestId('task-item-task-1')
      emit('TaskAssigned', { ...defaultTasks[0], taskId: defaultTasks[0].id, assignedToUserId: 'user-2' })
    })

    Then('je vois la tâche "Bûche au chocolat" dans la section "Assignées"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('task-list-section-assigned')).getByTestId('task-item-task-1')).toBeInTheDocument(),
      )
    })
  })

  Scenario('Une tâche supprimée par un autre participant disparaît automatiquement', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('un autre participant supprime la tâche "Réserver la salle"', async () => {
      await screen.findByTestId('task-item-task-2')
      emit('TaskDeleted', { eventId: 'event-1', taskId: 'task-2' })
    })

    Then('je ne vois plus la tâche "Réserver la salle"', async () => {
      await waitFor(() => expect(screen.queryByTestId('task-item-task-2')).not.toBeInTheDocument())
    })
  })
})
