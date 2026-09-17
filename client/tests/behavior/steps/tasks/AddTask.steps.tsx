import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
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

  function taskRow(title: string) {
    return screen.getByText(title).closest('li')!
  }

  async function fillAndSubmit(title: string, category: string, quantity?: string) {
    await screen.findByLabelText('Titre')
    const user = userEvent.setup()
    await user.type(screen.getByLabelText('Titre'), title)
    await user.selectOptions(screen.getByLabelText('Catégorie'), category)
    if (quantity) {
      await user.type(screen.getByLabelText('Quantité'), quantity)
    }
    await user.click(screen.getByRole('button', { name: /ajouter la tâche/i }))
  }

  Scenario('Ajout d\'une tâche avec succès', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      server.use(
        http.post('*/api/events/:id/tasks', async ({ request, params }) => {
          const body = (await request.json()) as { title: string; category: string; quantity: string | null }
          return HttpResponse.json(
            {
              id: 'task-new',
              eventId: params.id,
              title: body.title,
              category: body.category,
              quantity: body.quantity,
              assignedToUserId: null,
              isDone: false,
              createdAt: '2026-09-17T00:00:00Z',
            },
            { status: 201 },
          )
        }),
      )
      renderApp('/events/event-1')
    })

    And('j\'ajoute la tâche "Guirlandes" de catégorie "Logistique" et de quantité "2"', async () => {
      await fillAndSubmit('Guirlandes', 'Logistique', '2')
    })

    Then('je vois la tâche "Guirlandes" de catégorie "Logistique"', async () => {
      await waitFor(() => expect(taskRow('Guirlandes')).toHaveTextContent('Logistique'))
    })

    And('le formulaire d\'ajout de tâche est réinitialisé', () => {
      expect(screen.getByLabelText('Titre')).toHaveValue('')
      expect(screen.getByLabelText('Quantité')).toHaveValue('')
    })
  })

  Scenario('Ajout d\'une tâche sans quantité', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      server.use(
        http.post('*/api/events/:id/tasks', async ({ request, params }) => {
          const body = (await request.json()) as { title: string; category: string; quantity: string | null }
          return HttpResponse.json(
            {
              id: 'task-new',
              eventId: params.id,
              title: body.title,
              category: body.category,
              quantity: body.quantity,
              assignedToUserId: null,
              isDone: false,
              createdAt: '2026-09-17T00:00:00Z',
            },
            { status: 201 },
          )
        }),
      )
      renderApp('/events/event-1')
    })

    And('j\'ajoute la tâche "Réserver le DJ" de catégorie "Autre" sans quantité', async () => {
      await fillAndSubmit('Réserver le DJ', 'Autre')
    })

    Then('je vois la tâche "Réserver le DJ" de catégorie "Autre"', async () => {
      await waitFor(() => expect(taskRow('Réserver le DJ')).toHaveTextContent('Autre'))
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

    And('j\'ajoute la tâche "Guirlandes" de catégorie "Logistique" et de quantité "2"', async () => {
      await fillAndSubmit('Guirlandes', 'Logistique', '2')
    })

    Then('je vois un message d\'erreur pour l\'ajout de tâche', async () => {
      await screen.findByText('Impossible d\'ajouter cette tâche.')
    })
  })
})
