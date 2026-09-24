import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/items/AddItem.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  BeforeEachScenario(() => {
    server.use(http.get('*/api/events/:id/items', () => HttpResponse.json([])))
  })

  async function addItem(title: string) {
    await screen.findByTestId('add-item-title-input')
    const user = userEvent.setup()
    await user.type(screen.getByTestId('add-item-title-input'), title)
    await user.click(screen.getByTestId('add-item-submit-button'))
  }

  Scenario('Ajout d\'un article avec succès', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      server.use(
        http.post('*/api/events/:id/items', async ({ request, params }) => {
          const body = (await request.json()) as { title: string; quantity: string | null }
          return HttpResponse.json(
            {
              id: 'item-new',
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

    And('j\'ajoute l\'article "Guirlandes"', async () => {
      await addItem('Guirlandes')
    })

    Then('je vois l\'article "Guirlandes" dans la section "À prendre"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('item-list-section-todo')).getByTestId('item-row-title-item-new')).toHaveTextContent(
          'Guirlandes',
        ),
      )
    })

    And('le formulaire d\'ajout d\'article est réinitialisé', () => {
      expect(screen.getByTestId('add-item-title-input')).toHaveValue('')
    })
  })

  Scenario('Erreur lors de l\'ajout d\'un article', ({ Given, When, And, Then }) => {
    Given('l\'ajout d\'un article échoue', () => {
      server.use(
        http.post('*/api/events/:id/items', () =>
          HttpResponse.json({ title: 'Impossible d\'ajouter cet article.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'ajoute l\'article "Guirlandes"', async () => {
      await addItem('Guirlandes')
    })

    Then('je vois un message d\'erreur pour l\'ajout d\'article', async () => {
      const errorMessage = await screen.findByTestId('add-item-error')
      expect(errorMessage).toHaveTextContent('Impossible d\'ajouter cet article.')
    })
  })
})
