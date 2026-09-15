import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/tasks/TaskList.feature', { language: 'fr' })

const defaultTasks = [
  {
    id: 'task-1',
    eventId: 'event-1',
    title: 'Bûche au chocolat',
    category: 'Courses',
    quantity: '1',
    assignedToUserId: null,
    isDone: false,
    createdAt: '2026-09-01T00:00:00Z',
  },
  {
    id: 'task-2',
    eventId: 'event-1',
    title: 'Réserver la salle',
    category: 'Logistique',
    quantity: null,
    assignedToUserId: null,
    isDone: false,
    createdAt: '2026-09-02T00:00:00Z',
  },
]

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  BeforeEachScenario(() => {
    server.use(http.get('*/api/events/:id/tasks', () => HttpResponse.json(defaultTasks)))
  })

  function taskRow(title: string) {
    return screen.getByText(title).closest('li')!
  }

  Scenario('Affichage des tâches de toutes les catégories', ({ When, Then, And }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je vois la tâche "Bûche au chocolat" de catégorie "Courses"', async () => {
      await screen.findByText('Bûche au chocolat')
      expect(taskRow('Bûche au chocolat')).toHaveTextContent('Courses')
    })

    And('je vois la tâche "Réserver la salle" de catégorie "Logistique"', () => {
      expect(taskRow('Réserver la salle')).toHaveTextContent('Logistique')
    })
  })

  Scenario('Filtrage des tâches par catégorie', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je filtre les tâches par catégorie "Courses"', async () => {
      await screen.findByText('Bûche au chocolat')
      const user = userEvent.setup()
      await user.selectOptions(screen.getByLabelText('Filtrer par catégorie'), 'Courses')
    })

    Then('je vois la tâche "Bûche au chocolat" de catégorie "Courses"', () => {
      expect(taskRow('Bûche au chocolat')).toHaveTextContent('Courses')
    })

    And('je ne vois pas la tâche "Réserver la salle"', () => {
      expect(screen.queryByText('Réserver la salle')).not.toBeInTheDocument()
    })
  })

  Scenario('Retour au filtre "Toutes" après un filtrage', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je filtre les tâches par catégorie "Courses"', async () => {
      await screen.findByText('Bûche au chocolat')
      const user = userEvent.setup()
      await user.selectOptions(screen.getByLabelText('Filtrer par catégorie'), 'Courses')
    })

    And('je filtre les tâches par catégorie "Toutes"', async () => {
      const user = userEvent.setup()
      await user.selectOptions(screen.getByLabelText('Filtrer par catégorie'), 'Toutes')
    })

    Then('je vois la tâche "Bûche au chocolat" de catégorie "Courses"', () => {
      expect(taskRow('Bûche au chocolat')).toHaveTextContent('Courses')
    })

    And('je vois la tâche "Réserver la salle" de catégorie "Logistique"', () => {
      expect(taskRow('Réserver la salle')).toHaveTextContent('Logistique')
    })
  })

  Scenario('Aucune tâche dans la catégorie sélectionnée', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je filtre les tâches par catégorie "Autre"', async () => {
      await screen.findByText('Bûche au chocolat')
      const user = userEvent.setup()
      await user.selectOptions(screen.getByLabelText('Filtrer par catégorie'), 'Autre')
    })

    Then('je vois un message indiquant qu\'il n\'y a aucune tâche dans cette catégorie', async () => {
      await screen.findByText('Aucune tâche dans cette catégorie.')
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
      await screen.findByText('Aucune tâche pour le moment.')
    })
  })
})
