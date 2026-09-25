import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given } = createBdd()

const participants = [
  {
    userId: 'user-1',
    email: 'organisateur@example.com',
    displayName: 'Organisateur',
    role: 'Organizer',
    invitedAt: '2026-09-01T00:00:00Z',
    participationStatus: 'Attending',
  },
  {
    userId: 'user-2',
    email: 'ami@example.com',
    displayName: 'Ami',
    role: 'Participant',
    invitedAt: '2026-09-02T00:00:00Z',
    participationStatus: 'Unknown',
  },
]

const items = [
  {
    id: 'item-1',
    eventId: 'event-1',
    title: 'Bûche au chocolat',
    quantity: '1',
    kind: 'ToBring',
    assignedToUserId: null,
    createdAt: '2026-09-01T00:00:00Z',
  },
  {
    id: 'item-2',
    eventId: 'event-1',
    title: 'Réserver la salle',
    quantity: null,
    kind: 'ToBring',
    assignedToUserId: null,
    createdAt: '2026-09-02T00:00:00Z',
  },
]

async function mockEventDetail(page: import('@playwright/test').Page) {
  await page.route('**/api/events/event-1', (route) =>
    route.fulfill({
      json: {
        id: 'event-1',
        title: 'Repas de Noël',
        description: 'Un bon repas de fêtes entre amis.',
        eventDate: '2026-12-24T00:00:00Z',
        location: 'Chez Alice',
        createdByUserId: 'user-1',
        status: 'Planned',
        createdAt: '2026-09-01T00:00:00Z',
        participants,
      },
    }),
  )
  await page.route('**/api/events/event-1/items', (route) => route.fulfill({ json: items }))
  // ItemList ouvre une connexion SignalR (`useItemRealtime`) vers `/hubs/events`, proxifiée par Vite
  // vers la vraie API (cf. vite.config.ts) : absente en test visuel, d'où un abort de la négociation
  // pour éviter que le navigateur tente de joindre un backend qui n'existe pas ici.
  await page.route('**/hubs/events/negotiate**', (route) => route.fulfill({ status: 404 }))
}

Given("je suis sur le détail d'un événement en tant que créateur", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await page.getByTestId('event-detail-participants-button').click()
  await expect(page.getByTestId('participant-promote-button-user-2')).toBeVisible()
})

Given("je suis sur le détail d'un événement en tant que simple participant", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' } }),
  )
  await page.goto('/events/event-1')
  await page.getByTestId('event-detail-participants-button').click()
  await expect(page.getByTestId('participant-row-user-2')).toBeVisible()
})

Given("je suis sur le détail d'un événement avec des articles à prendre et assignés", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/events/event-1/items', (route) =>
    route.fulfill({
      json: items.map((item) => (item.id === 'item-2' ? { ...item, assignedToUserId: 'user-2' } : item)),
    }),
  )
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await expect(page.getByTestId('item-row-item-2')).toBeVisible()
})

Given("je suis sur le détail d'un événement avec le formulaire d'ajout d'article rempli", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await page.getByTestId('add-item-title-input').fill('Guirlandes')
})

Given("je suis sur le détail d'un événement en tant que simple participant qui apporte un article", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/events/event-1/items', (route) =>
    route.fulfill({
      json: [
        ...items,
        {
          id: 'item-3',
          eventId: 'event-1',
          title: 'Chips',
          quantity: null,
          kind: 'Contribution',
          assignedToUserId: 'user-2',
          createdAt: '2026-09-03T00:00:00Z',
        },
      ],
    }),
  )
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' } }),
  )
  await page.goto('/events/event-1')
  await expect(page.getByTestId('item-row-cancel-button-item-3')).toBeVisible()
})

Given("je suis sur le détail d'un événement avec le statut de participation renseigné", async ({ page }) => {
  await mockEventDetail(page)
  // Copie locale au scénario : la route GET partagée (mockEventDetail) sert le tableau `participants` du
  // module tel quel, or ce scénario est le seul à faire évoluer un statut après un PUT — muter le tableau
  // partagé fuiterait vers les autres scénarios de ce fichier (mêmes objets, même worker Playwright).
  const localParticipants = participants.map((p) => ({ ...p }))
  await page.route('**/api/events/event-1', (route) =>
    route.fulfill({
      json: {
        id: 'event-1',
        title: 'Repas de Noël',
        description: 'Un bon repas de fêtes entre amis.',
        eventDate: '2026-12-24T00:00:00Z',
        location: 'Chez Alice',
        createdByUserId: 'user-1',
        status: 'Planned',
        createdAt: '2026-09-01T00:00:00Z',
        participants: localParticipants,
      },
    }),
  )
  // Le créateur (user-1) ne voit pas ce dropdown (statut "Attending" fixe côté Domain) : ce scénario
  // se place du point de vue d'un simple participant (user-2), seul à pouvoir indiquer son statut.
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' } }),
  )
  await page.route('**/api/events/event-1/participation-status', async (route) => {
    const { status } = route.request().postDataJSON() as { status: string }
    localParticipants[1].participationStatus = status
    await route.fulfill({ status: 204 })
  })
  await page.goto('/events/event-1')
  await page.getByTestId('event-detail-participation-status-select').click()
  await page.getByTestId('event-detail-participation-status-select-option-Attending').click()
  await expect(page.getByTestId('event-detail-participation-status-select')).toHaveText('Je viens !')
})

Given("je suis sur le détail d'un événement avec une erreur d'invitation", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.route('**/api/events/event-1/participants', (route) =>
    route.fulfill({ status: 400, json: { detail: 'Cette personne est déjà invitée à cet événement.' } }),
  )
  await page.goto('/events/event-1')
  await page.getByTestId('event-detail-participants-button').click()
  await page.getByTestId('invite-participant-email-input').fill('ami@example.com')
  await page.getByTestId('invite-participant-submit-button').click()
  await expect(page.getByTestId('invite-participant-error')).toBeVisible()
})
