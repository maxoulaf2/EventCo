import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/tasks/ToggleTask.feature', { language: 'fr' })

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
      isDone: true,
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
      http.post('*/api/events/:id/tasks/:taskId/complete', () => new HttpResponse(null, { status: 204 })),
      http.post('*/api/events/:id/tasks/:taskId/reopen', () => new HttpResponse(null, { status: 204 })),
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

  async function toggle(taskId: string) {
    await screen.findByTestId(`task-item-checkbox-${taskId}`)
    const user = userEvent.setup()
    await user.click(screen.getByTestId(`task-item-checkbox-${taskId}`))
  }

  Scenario('Le créateur coche une tâche à prendre', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je coche la tâche "Bûche au chocolat"', async () => {
      await toggle('task-1')
    })

    And('je vais sur l\'onglet "Faites"', async () => {
      await goToTab('Faites')
    })

    Then('la tâche "Bûche au chocolat" est cochée', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-checkbox-task-1')).toBeChecked())
    })
  })

  Scenario('Le créateur décoche une tâche faite', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je vais sur l\'onglet "Faites"', async () => {
      await screen.findByTestId('task-item-task-1')
      await goToTab('Faites')
    })

    And('je décoche la tâche "Réserver la salle"', async () => {
      await toggle('task-2')
    })

    And('je vais sur l\'onglet "À prendre"', async () => {
      await goToTab('À prendre')
    })

    Then('la tâche "Réserver la salle" n\'est pas cochée', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-checkbox-task-2')).not.toBeChecked())
    })
  })

  Scenario('Un participant coche sa propre tâche assignée', ({ Given, When, And, Then }) => {
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
      await goToTab('Assignées')
    })

    And('je coche la tâche "Bûche au chocolat"', async () => {
      await toggle('task-1')
    })

    And('je vais sur l\'onglet "Faites"', async () => {
      await goToTab('Faites')
    })

    Then('la tâche "Bûche au chocolat" est cochée', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-checkbox-task-1')).toBeChecked())
    })
  })

  Scenario('Un participant ne peut pas cocher une tâche qui n\'est pas la sienne', ({ Given, When, And, Then }) => {
    Given('je suis un simple participant et que la tâche "Réserver la salle" est assignée à quelqu\'un d\'autre', () => {
      tasks[1].isDone = false
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

    Then('la case de la tâche "Réserver la salle" est désactivée', async () => {
      await screen.findByTestId('task-item-checkbox-task-2')
      expect(screen.getByTestId('task-item-checkbox-task-2')).toBeDisabled()
    })
  })

  Scenario('Erreur lors du basculement d\'une tâche', ({ Given, When, And, Then }) => {
    Given('le basculement de la tâche "Bûche au chocolat" échoue', () => {
      server.use(
        http.post('*/api/events/:id/tasks/:taskId/complete', () =>
          HttpResponse.json({ title: 'Impossible de mettre à jour cette tâche.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je coche la tâche "Bûche au chocolat"', async () => {
      await toggle('task-1')
    })

    Then('la tâche "Bûche au chocolat" n\'est pas cochée', async () => {
      await waitFor(() => expect(screen.getByTestId('task-item-checkbox-task-1')).not.toBeChecked())
    })
  })
})
