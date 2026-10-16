import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/items/CorrectItemCategory.feature', { language: 'fr' })

function buildItems() {
  return [
    {
      id: 'item-1',
      eventId: 'event-1',
      title: 'Réserver la salle',
      quantity: null,
      kind: 'ToBring',
      category: 'Other',
      // Déjà pris : un article pris est rangé dans sa catégorie (un article à prendre reste dans « À prendre »).
      assignedToUserId: 'user-3' as string | null,
      createdAt: '2026-09-01T00:00:00Z',
    },
    {
      id: 'item-2',
      eventId: 'event-1',
      title: 'Le truc de mamie',
      quantity: null,
      kind: 'Contribution',
      category: 'Other',
      assignedToUserId: 'user-2' as string | null,
      createdAt: '2026-09-02T00:00:00Z',
    },
  ]
}

function actAsSimpleParticipant() {
  server.use(
    http.get('*/api/auth/me', () => HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' })),
  )
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let sentCategory: { itemId: string; category: string } | undefined

  BeforeEachScenario(() => {
    sentCategory = undefined
    server.use(
      http.get('*/api/events/:id/items', () => HttpResponse.json(buildItems())),
      http.put('*/api/events/:id/items/:itemId/category', async ({ request, params }) => {
        const body = (await request.json()) as { category: string }
        sentCategory = { itemId: String(params.itemId), category: body.category }
        return new HttpResponse(null, { status: 204 })
      }),
    )
  })

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  async function moveToEquipment() {
    const select = await screen.findByTestId('item-row-category-select-item-1')
    await userEvent.setup().selectOptions(select, 'Equipment')
  }

  Scenario('Un organisateur corrige la catégorie d\'un article', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je range l\'article "Réserver la salle" dans la catégorie "Matériel et déco"', async () => {
      await moveToEquipment()
    })

    Then('la catégorie "Equipment" est envoyée pour l\'article "Réserver la salle"', async () => {
      await waitFor(() => expect(sentCategory).toEqual({ itemId: 'item-1', category: 'Equipment' }))
    })

    And('je vois l\'article "Réserver la salle" dans la catégorie "Matériel et déco"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('item-list-category-Equipment')).getByTestId('item-row-item-1')).toBeInTheDocument(),
      )
    })
  })

  Scenario('Un participant simple corrige la catégorie de ce qu\'il apporte', ({ Given, When, Then }) => {
    Given('je suis un simple participant qui apporte l\'article "Le truc de mamie"', () => {
      actAsSimpleParticipant()
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je peux corriger la catégorie de l\'article "Le truc de mamie"', async () => {
      await screen.findByTestId('item-row-category-select-item-2')
    })
  })

  Scenario('Un participant simple ne peut pas corriger la catégorie d\'un article à prendre', ({ Given, When, Then }) => {
    Given('je suis un simple participant', () => {
      actAsSimpleParticipant()
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    Then('je ne peux pas corriger la catégorie de l\'article "Réserver la salle"', async () => {
      await screen.findByTestId('item-row-item-1')
      expect(screen.queryByTestId('item-row-category-select-item-1')).not.toBeInTheDocument()
    })
  })

  Scenario('Erreur lors de la correction de la catégorie', ({ Given, When, And, Then }) => {
    Given('la correction de catégorie échoue', () => {
      server.use(
        http.put('*/api/events/:id/items/:itemId/category', () =>
          HttpResponse.json({ title: 'Impossible de corriger cette catégorie.' }, { status: 400 }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('je range l\'article "Réserver la salle" dans la catégorie "Matériel et déco"', async () => {
      await moveToEquipment()
    })

    Then('je vois l\'article "Réserver la salle" dans la catégorie "Autre"', async () => {
      await waitFor(() =>
        expect(within(screen.getByTestId('item-list-category-Other')).getByTestId('item-row-item-1')).toBeInTheDocument(),
      )
    })
  })
})
