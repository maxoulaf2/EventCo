import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, fireEvent, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/CreateEvent.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Background, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Background(({ Given }) => {
    Given('je suis sur la page de création d\'événement', () => {
      renderApp('/events/new')
    })
  })

  Scenario('Titre et date valides', ({ When, Then }) => {
    When(
      'je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire',
      async () => {
        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
    })
  })

  Scenario('Le serveur refuse la création', ({ And, When, Then }) => {
    And('le serveur refusera la prochaine création d\'événement', () => {
      server.use(
        http.post('*/api/events', () =>
          HttpResponse.json({ detail: 'La date est obligatoire.' }, { status: 400 }),
        ),
      )
    })

    When(
      'je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire',
      async () => {
        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('je vois un message d\'erreur sur le formulaire', async () => {
      const errorMessage = await screen.findByTestId('create-event-error')
      expect(errorMessage).toHaveTextContent('La date est obligatoire.')
    })

    And('je reste sur la page de création d\'événement', () => {
      expect(screen.getByTestId('create-event-submit-button')).toBeInTheDocument()
    })
  })

  Scenario('Création avec une image', ({ When, Then }) => {
    let requestBody: { imageUrl?: string } | undefined

    When(
      'je saisis le titre "Repas de Noël", la date "2026-12-24" et l\'image "https://example.com/photo.jpg" puis je valide le formulaire',
      async () => {
        server.use(
          http.post('*/api/events', async ({ request }) => {
            requestBody = (await request.json()) as { imageUrl?: string }
            return HttpResponse.json(
              { id: 'event-new', title: 'Repas de Noël', description: null, eventDate: '2026-12-24T00:00:00Z', location: null, imageUrl: requestBody.imageUrl ?? null, createdByUserId: 'user-1', status: 'Draft', createdAt: '2026-09-08T00:00:00Z' },
              { status: 201 },
            )
          }),
        )

        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        await user.type(screen.getByTestId('create-event-image-url-input'), 'https://example.com/photo.jpg')
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('la requête de création envoyée au serveur contient l\'image "https://example.com/photo.jpg"', async () => {
      await screen.findByTestId('events-dashboard-title')
      expect(requestBody?.imageUrl).toBe('https://example.com/photo.jpg')
    })
  })

  Scenario('Création avec une heure précisée', ({ When, Then }) => {
    let requestBody: { eventDate?: string } | undefined

    When(
      'je saisis le titre "Repas de Noël", la date "2026-12-24" et l\'heure "19:30" puis je valide le formulaire',
      async () => {
        server.use(
          http.post('*/api/events', async ({ request }) => {
            requestBody = (await request.json()) as { eventDate?: string }
            return HttpResponse.json(
              { id: 'event-new', title: 'Repas de Noël', description: null, eventDate: requestBody.eventDate, location: null, imageUrl: null, createdByUserId: 'user-1', status: 'Draft', createdAt: '2026-09-08T00:00:00Z' },
              { status: 201 },
            )
          }),
        )

        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        fireEvent.change(screen.getByTestId('create-event-time-input'), { target: { value: '19:30' } })
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('la requête de création envoyée au serveur contient la date "2026-12-24T18:30:00.000Z"', async () => {
      await screen.findByTestId('events-dashboard-title')
      expect(requestBody?.eventDate).toBe('2026-12-24T18:30:00.000Z')
    })
  })

  Scenario('Création sans heure précisée', ({ When, Then }) => {
    let requestBody: { eventDate?: string } | undefined

    When(
      'je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire',
      async () => {
        server.use(
          http.post('*/api/events', async ({ request }) => {
            requestBody = (await request.json()) as { eventDate?: string }
            return HttpResponse.json(
              { id: 'event-new', title: 'Repas de Noël', description: null, eventDate: requestBody.eventDate, location: null, imageUrl: null, createdByUserId: 'user-1', status: 'Draft', createdAt: '2026-09-08T00:00:00Z' },
              { status: 201 },
            )
          }),
        )

        const user = userEvent.setup()
        await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
        fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
        await user.click(screen.getByTestId('create-event-submit-button'))
      },
    )

    Then('la requête de création envoyée au serveur contient la date "2026-12-23T23:00:00.000Z"', async () => {
      await screen.findByTestId('events-dashboard-title')
      expect(requestBody?.eventDate).toBe('2026-12-23T23:00:00.000Z')
    })
  })
})
