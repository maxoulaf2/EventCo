import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/events/CreateEvent.feature', { language: 'fr' })

async function fillTitleAndDate() {
  const user = userEvent.setup()
  await user.type(screen.getByTestId('create-event-title-input'), 'Repas de Noël')
  fireEvent.change(screen.getByTestId('create-event-date-input'), { target: { value: '2026-12-24' } })
}

async function chooseEventImage() {
  const user = userEvent.setup()
  const image = new File([new Uint8Array([0x89, 0x50, 0x4e, 0x47])], 'photo.png', { type: 'image/png' })
  await user.upload(screen.getByTestId('create-event-image-input'), image)
}

async function submitForm() {
  const user = userEvent.setup()
  await user.click(screen.getByTestId('create-event-submit-button'))
}

// Enregistre les créations d'événement et les envois d'image reçus par le serveur simulé. Les handlers
// sont installés par la première étape du scénario (install) : un server.use à la déclaration du
// scénario serait effacé par le resetHandlers des scénarios exécutés avant lui.
function createEventRequestsRecorder() {
  const requests = { createdEvents: 0, uploadedImages: [] as { eventId: string; contentType: string | null }[] }

  const install = () =>
    server.use(
      http.post('*/api/events', () => {
        requests.createdEvents++
        return HttpResponse.json(
          { id: 'event-new', title: 'Repas de Noël', description: null, eventDate: '2026-12-24T00:00:00Z', location: null, imageUrl: null, createdByUserId: 'user-1', status: 'Draft', createdAt: '2026-09-08T00:00:00Z' },
          { status: 201 },
        )
      }),
      http.put('*/api/events/:id/image', ({ request, params }) => {
        requests.uploadedImages.push({ eventId: String(params.id), contentType: request.headers.get('content-type') })
        return HttpResponse.json({ eventId: params.id, imageUrl: '/uploads/event-images/event-new/image-1.jpg' })
      }),
    )

  return { requests, install }
}

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

  Scenario('Création avec une image', ({ When, And, Then }) => {
    const { requests, install } = createEventRequestsRecorder()

    When('je saisis le titre "Repas de Noël" et la date "2026-12-24"', async () => {
      install()
      await fillTitleAndDate()
    })

    And('je choisis une image pour l\'événement', chooseEventImage)

    Then('je vois l\'aperçu de l\'image choisie', async () => {
      expect(await screen.findByTestId('create-event-image-preview')).toHaveAttribute('src', expect.stringMatching(/^data:image\/png;base64,/))
    })

    When('je valide le formulaire', submitForm)

    Then('l\'image est envoyée pour l\'événement créé', async () => {
      await waitFor(() => expect(requests.uploadedImages).toHaveLength(1))
      expect(requests.uploadedImages[0].eventId).toBe('event-new')
      expect(requests.uploadedImages[0].contentType).toMatch(/^multipart\/form-data/)
    })

    And('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
    })
  })

  Scenario('Image retirée avant la création', ({ When, And, Then }) => {
    const { requests, install } = createEventRequestsRecorder()

    When('je saisis le titre "Repas de Noël" et la date "2026-12-24"', async () => {
      install()
      await fillTitleAndDate()
    })

    And('je choisis une image pour l\'événement', chooseEventImage)

    And('je retire l\'image choisie', async () => {
      const user = userEvent.setup()
      await user.click(await screen.findByTestId('create-event-image-remove-button'))
    })

    And('je valide le formulaire', submitForm)

    Then('aucune image n\'est envoyée', async () => {
      await waitFor(() => expect(requests.createdEvents).toBe(1))
      expect(requests.uploadedImages).toHaveLength(0)
    })

    And('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
    })
  })

  Scenario('Le serveur refuse l\'image après la création de l\'événement', ({ Given, When, And, Then }) => {
    const { requests, install } = createEventRequestsRecorder()

    Given('le serveur refusera la prochaine image d\'événement', () => {
      install()
      // Prioritaire sur l'enregistrement pour un seul appel, qui reprend ensuite la main.
      server.use(
        http.put(
          '*/api/events/:id/image',
          () => HttpResponse.json({ detail: 'Format d\'image non supporté.' }, { status: 400 }),
          { once: true },
        ),
      )
    })

    When('je saisis le titre "Repas de Noël" et la date "2026-12-24"', fillTitleAndDate)

    And('je choisis une image pour l\'événement', chooseEventImage)

    And('je valide le formulaire', submitForm)

    Then('je vois que l\'événement a été créé mais pas son image', async () => {
      expect(await screen.findByTestId('create-event-image-error')).toHaveTextContent('Format d\'image non supporté.')
      expect(screen.getByTestId('create-event-submit-button')).toHaveTextContent('Réessayer l\'envoi de l\'image')
      expect(screen.getByTestId('create-event-skip-image-link')).toBeInTheDocument()
      expect(screen.getByTestId('create-event-title-input')).toBeDisabled()
    })

    When('je valide le formulaire', submitForm)

    Then('l\'image est envoyée pour l\'événement créé', async () => {
      await waitFor(() => expect(requests.uploadedImages).toHaveLength(1))
      expect(requests.uploadedImages[0].eventId).toBe('event-new')
    })

    And('l\'événement n\'a été créé qu\'une seule fois', () => {
      expect(requests.createdEvents).toBe(1)
    })

    And('je suis redirigé vers le tableau de bord', async () => {
      await screen.findByTestId('events-dashboard-title')
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
