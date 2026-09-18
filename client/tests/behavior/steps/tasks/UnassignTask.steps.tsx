import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/tasks/UnassignTask.feature', { language: 'fr' })

const TAB_KEY: Record<string, string> = {
  'À prendre': 'todo',
  Assignées: 'assigned',
  Faites: 'done',
}

function buildTasks() {
  return [
    {
      id: 'task-1',
      eventId: 'event-1',
      title: 'Bûche au chocolat',
      category: 'Courses',
      quantity: '1',
      assignedToUserId: null as string | null,
      isDone: false,
      createdAt: '2026-09-01T00:00:00Z',
    },
    {
      id: 'task-2',
      eventId: 'event-1',
      title: 'Réserver la salle',
      category: 'Logistique',
      quantity: null,
      assignedToUserId: null as string | null,
      isDone: false,
      createdAt: '2026-09-02T00:00:00Z',
    },
  ]
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let tasks = buildTasks()

  BeforeEachScenario(() => {
    tasks = buildTasks()
    server.use(
      http.get('*/api/events/:id/tasks', () => HttpResponse.json(tasks)),
      http.post('*/api/events/:id/tasks/:taskId/unassign', () => new HttpResponse(null, { status: 204 })),
    )
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  async function goToTab(label: string) {
    await screen.findByTestId(`task-list-tab-${TAB_KEY[label]}`)
    const user = userEvent.setup()
    await user.click(screen.getByTestId(`task-list-tab-${TAB_KEY[label]}`))
  }

  async function unassign(taskId: string) {
    await screen.findByTestId(`task-item-unassign-button-${taskId}`)
    const user = userEvent.setup()
    await user.click(screen.getByTestId(`task-item-unassign-button-${taskId}`))
  }

  Scenario('Le créateur désassigne la tâche d\'un autre participant', ({ Given, When, And, Then }) => {
    Given('la tâche "Bûche au chocolat" est assignée à un participant', () => {
      tasks[0].assignedToUserId = 'user-2'
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je vais sur l\'onglet "Assignées"', async () => {
      await screen.findByTestId('task-item-task-2')
      await goToTab('Assignées')
    })

    And('je désassigne la tâche "Bûche au chocolat"', async () => {
      await unassign('task-1')
    })

    And('je vais sur l\'onglet "À prendre"', async () => {
      await goToTab('À prendre')
    })

    Then('je vois la tâche "Bûche au chocolat" sur l\'onglet "À prendre"', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-task-1')).toBeInTheDocument())
    })
  })

  Scenario('Un participant se désassigne de sa propre tâche', ({ Given, When, And, Then }) => {
    Given('je suis un simple participant assigné à la tâche "Bûche au chocolat"', () => {
      tasks[0].assignedToUserId = 'user-2'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je vais sur l\'onglet "Assignées"', async () => {
      await screen.findByTestId('task-item-task-2')
      await goToTab('Assignées')
    })

    And('je désassigne la tâche "Bûche au chocolat"', async () => {
      await unassign('task-1')
    })

    And('je vais sur l\'onglet "À prendre"', async () => {
      await goToTab('À prendre')
    })

    Then('je vois la tâche "Bûche au chocolat" sur l\'onglet "À prendre"', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-task-1')).toBeInTheDocument())
    })
  })

  Scenario('Un participant ne peut pas désassigner la tâche d\'un autre', ({ Given, When, And, Then }) => {
    Given('je suis un simple participant et que la tâche "Réserver la salle" est assignée à quelqu\'un d\'autre', () => {
      tasks[1].assignedToUserId = 'user-3'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je vais sur l\'onglet "Assignées"', async () => {
      await screen.findByTestId('task-item-task-1')
      await goToTab('Assignées')
    })

    Then('je ne vois pas de bouton de désassignation pour la tâche "Réserver la salle"', async () => {
      await screen.findByTestId('task-item-task-2')
      expect(screen.queryByTestId('task-item-unassign-button-task-2')).not.toBeInTheDocument()
    })
  })

  Scenario('Erreur lors de la désassignation d\'une tâche', ({ Given, And, When, Then }) => {
    Given('la tâche "Bûche au chocolat" est assignée à un participant', () => {
      tasks[0].assignedToUserId = 'user-2'
    })

    And('la désassignation de la tâche "Bûche au chocolat" échoue', () => {
      server.use(
        http.post('*/api/events/:id/tasks/:taskId/unassign', () =>
          HttpResponse.json({ title: 'Impossible de désassigner cette tâche.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je vais sur l\'onglet "Assignées"', async () => {
      await screen.findByTestId('task-item-task-2')
      await goToTab('Assignées')
    })

    And('je désassigne la tâche "Bûche au chocolat"', async () => {
      await unassign('task-1')
    })

    Then('je vois la tâche "Bûche au chocolat" sur l\'onglet "Assignées"', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-task-1')).toBeInTheDocument())
    })
  })
})
