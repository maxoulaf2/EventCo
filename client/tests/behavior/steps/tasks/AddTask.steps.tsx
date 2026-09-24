import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/tasks/AddTask.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  BeforeEachScenario(() => {
    server.use(http.get('*/api/events/:id/tasks', () => HttpResponse.json([])))
  })

  async function addTask(title: string) {
    await screen.findByTestId('add-task-title-input')
    const user = userEvent.setup()
    await user.type(screen.getByTestId('add-task-title-input'), title)
    await user.click(screen.getByTestId('add-task-submit-button'))
  }

  Scenario('Ajout d\'une tâche avec succès', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      server.use(
        http.post('*/api/events/:id/tasks', async ({ request, params }) => {
          const body = (await request.json()) as { title: string; quantity: string | null }
          return HttpResponse.json(
            {
              id: 'task-new',
              eventId: params.id,
              title: body.title,
              quantity: body.quantity,
              assignedToUserId: null,
              createdAt: '2026-09-17T00:00:00Z',
            },
            { status: 201 },
          )
        }),
      )
      renderApp('/events/event-1')
    })

    And('j\'ajoute la tâche "Guirlandes"', async () => {
      await addTask('Guirlandes')
    })

    Then('je vois la tâche "Guirlandes" dans la section "À prendre"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('task-list-section-todo')).getByTestId('task-item-title-task-new')).toHaveTextContent(
          'Guirlandes',
        ),
      )
    })

    And('le formulaire d\'ajout de tâche est réinitialisé', () => {
      expect(screen.getByTestId('add-task-title-input')).toHaveValue('')
    })
  })

  Scenario('Erreur lors de l\'ajout d\'une tâche', ({ Given, When, And, Then }) => {
    Given('l\'ajout d\'une tâche échoue', () => {
      server.use(
        http.post('*/api/events/:id/tasks', () =>
          HttpResponse.json({ title: 'Impossible d\'ajouter cette tâche.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'ajoute la tâche "Guirlandes"', async () => {
      await addTask('Guirlandes')
    })

    Then('je vois un message d\'erreur pour l\'ajout de tâche', async () => {
      const errorMessage = await screen.findByTestId('add-task-error')
      expect(errorMessage).toHaveTextContent('Impossible d\'ajouter cette tâche.')
    })
  })
})
