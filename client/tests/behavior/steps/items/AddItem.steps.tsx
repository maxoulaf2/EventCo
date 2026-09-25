import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'
import type { EventItemKind } from '../../../../src/features/items/types'

const feature = await loadFeature('tests/behavior/features/items/AddItem.feature', { language: 'fr' })

const SECTION_KEY: Record<string, string> = {
  'À prendre': 'todo',
  Assignés: 'assigned',
}

const KIND_LABEL: Record<string, EventItemKind> = {
  'à prendre': 'ToBring',
  apporté: 'Contribution',
}

describeFeature(feature, ({ AfterEachScenario, BeforeEachScenario, Scenario }) => {
  let currentUserId = 'user-1'
  let sentKind: EventItemKind | undefined

  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  BeforeEachScenario(() => {
    currentUserId = 'user-1'
    sentKind = undefined
    server.use(
      http.get('*/api/events/:id/items', () => HttpResponse.json([])),
      http.post('*/api/events/:id/items', async ({ request, params }) => {
        const body = (await request.json()) as { title: string; quantity: string | null; kind: EventItemKind }
        sentKind = body.kind
        return HttpResponse.json(
          {
            id: 'item-new',
            eventId: params.id,
            title: body.title,
            quantity: body.quantity,
            kind: body.kind,
            assignedToUserId: body.kind === 'Contribution' ? currentUserId : null,
            createdAt: '2026-09-17T00:00:00Z',
          },
          { status: 201 },
        )
      }),
    )
  })

  async function addItem(title: string) {
    await screen.findByTestId('add-item-title-input')
    const user = userEvent.setup()
    await user.type(screen.getByTestId('add-item-title-input'), title)
    await user.click(screen.getByTestId('add-item-submit-button'))
  }

  async function expectItemInSection(sectionLabel: string) {
    await waitFor(() =>
      expect(
        within(screen.getByTestId(`item-list-section-${SECTION_KEY[sectionLabel]}`)).getByTestId('item-row-title-item-new'),
      ).toHaveTextContent('Guirlandes'),
    )
  }

  Scenario('Un organisateur ajoute un article à prendre', ({ When, And, Then }) => {
    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'ajoute l\'article "Guirlandes"', async () => {
      await addItem('Guirlandes')
    })

    Then('l\'article est envoyé comme "à prendre"', async () => {
      await waitFor(() => expect(sentKind).toBe(KIND_LABEL['à prendre']))
    })

    And('je vois l\'article "Guirlandes" dans la section "À prendre"', async () => {
      await expectItemInSection('À prendre')
    })

    And('le formulaire d\'ajout d\'article est réinitialisé', () => {
      expect(screen.getByTestId('add-item-title-input')).toHaveValue('')
    })
  })

  Scenario('Un participant simple ajoute ce qu\'il apporte', ({ Given, When, Then, And }) => {
    Given('je suis un simple participant', () => {
      currentUserId = 'user-2'
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({ userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' }),
        ),
      )
    })

    When('j\'arrive sur le détail de l\'événement', () => {
      renderApp('/events/event-1')
    })

    And('j\'ajoute l\'article "Guirlandes"', async () => {
      await addItem('Guirlandes')
    })

    Then('l\'article est envoyé comme "apporté"', async () => {
      await waitFor(() => expect(sentKind).toBe(KIND_LABEL['apporté']))
    })

    And('je vois l\'article "Guirlandes" dans la section "Assignés"', async () => {
      await expectItemInSection('Assignés')
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
