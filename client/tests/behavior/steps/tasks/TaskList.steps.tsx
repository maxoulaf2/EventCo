import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/tasks/TaskList.feature', { language: 'fr' })

const TAB_KEY: Record<string, string> = {
  'À prendre': 'todo',
  Assignées: 'assigned',
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
      createdAt: '2026-09-01T00:00:00Z',
    },
    {
      id: 'task-2',
      eventId: 'event-1',
      title: 'Réserver la salle',
      category: 'Logistique',
      quantity: null,
      assignedToUserId: null as string | null,
      createdAt: '2026-09-02T00:00:00Z',
    },
  ]
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let tasks = buildTasks()

  BeforeEachScenario(() => {
    tasks = buildTasks()
    server.use(http.get('*/api/events/:id/tasks', () => HttpResponse.json(tasks)))
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

  Scenario('Affichage des tâches à prendre', ({ When, Then, And }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois la tâche "Bûche au chocolat" sur l\'onglet "À prendre"', async () => {
      await screen.findByTestId('task-item-task-1')
    })

    And('je vois la tâche "Réserver la salle" sur l\'onglet "À prendre"', () => {
      expect(screen.getByTestId('task-item-task-2')).toBeInTheDocument()
    })
  })

  Scenario('Une tâche assignée apparaît sur l\'onglet "Assignées"', ({ Given, When, And, Then }) => {
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

    Then('je vois la tâche "Bûche au chocolat" sur l\'onglet "Assignées"', () => {
      expect(screen.getByTestId('task-item-task-1')).toBeInTheDocument()
    })

    And('je ne vois pas la tâche "Réserver la salle"', () => {
      expect(screen.queryByTestId('task-item-task-2')).not.toBeInTheDocument()
    })
  })

  Scenario('Aucune tâche sur un onglet', ({ Given, When, And, Then }) => {
    Given('cet événement n\'a aucune tâche assignée', () => {
      // Les deux tâches par défaut (buildTasks) sont déjà non assignées.
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je vais sur l\'onglet "Assignées"', async () => {
      await screen.findByTestId('task-item-task-1')
      await goToTab('Assignées')
    })

    Then('je vois un message indiquant qu\'il n\'y a aucune tâche sur cet onglet', async () => {
      await screen.findByTestId('task-list-empty-tab-message')
    })
  })

  Scenario('Événement sans tâche', ({ Given, When, Then }) => {
    Given('cet événement n\'a aucune tâche', () => {
      server.use(http.get('*/api/events/:id/tasks', () => HttpResponse.json([])))
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois un message indiquant qu\'il n\'y a aucune tâche pour le moment', async () => {
      await screen.findByTestId('task-list-empty-message')
    })
  })
})
