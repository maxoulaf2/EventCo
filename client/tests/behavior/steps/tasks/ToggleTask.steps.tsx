import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/tasks/ToggleTask.feature', { language: 'fr' })

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

  function taskCheckbox(title: string) {
    return screen.getByRole('checkbox', { name: new RegExp(title, 'i') })
  }

  async function toggle(title: string) {
    await screen.findByText(title)
    const user = userEvent.setup()
    await user.click(taskCheckbox(title))
  }

  Scenario('Le créateur coche une tâche à faire', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je coche la tâche "Bûche au chocolat"', async () => {
      await toggle('Bûche au chocolat')
    })

    Then('la tâche "Bûche au chocolat" est cochée', async () => {
      await waitFor(() => expect(taskCheckbox('Bûche au chocolat')).toBeChecked())
    })
  })

  Scenario('Le créateur décoche une tâche faite', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je décoche la tâche "Réserver la salle"', async () => {
      await toggle('Réserver la salle')
    })

    Then('la tâche "Réserver la salle" n\'est pas cochée', async () => {
      await waitFor(() => expect(taskCheckbox('Réserver la salle')).not.toBeChecked())
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

    And('je coche la tâche "Bûche au chocolat"', async () => {
      await toggle('Bûche au chocolat')
    })

    Then('la tâche "Bûche au chocolat" est cochée', async () => {
      await waitFor(() => expect(taskCheckbox('Bûche au chocolat')).toBeChecked())
    })
  })

  Scenario('Un participant ne peut pas cocher une tâche qui n\'est pas la sienne', ({ Given, When, Then }) => {
    Given('je suis un simple participant non assigné à la tâche "Réserver la salle"', () => {
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('la case de la tâche "Réserver la salle" est désactivée', async () => {
      await screen.findByText('Réserver la salle')
      expect(taskCheckbox('Réserver la salle')).toBeDisabled()
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
      await toggle('Bûche au chocolat')
    })

    Then('la tâche "Bûche au chocolat" n\'est pas cochée', async () => {
      await waitFor(() => expect(taskCheckbox('Bûche au chocolat')).not.toBeChecked())
    })
  })
})
