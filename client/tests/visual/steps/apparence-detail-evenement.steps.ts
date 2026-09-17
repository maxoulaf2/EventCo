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
    hasJoined: true,
  },
  {
    userId: 'user-2',
    email: 'ami@example.com',
    displayName: 'Ami',
    role: 'Participant',
    invitedAt: '2026-09-02T00:00:00Z',
    hasJoined: false,
  },
]

const tasks = [
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
    isDone: true,
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
  await page.route('**/api/events/event-1/tasks', (route) => route.fulfill({ json: tasks }))
}

Given("je suis sur le détail d'un événement en tant que créateur", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await expect(page.getByRole('button', { name: /promouvoir co-organisateur/i })).toBeVisible()
})

Given("je suis sur le détail d'un événement en tant que simple participant", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-2', email: 'ami@example.com', displayName: 'Ami' } }),
  )
  await page.goto('/events/event-1')
  await expect(page.getByText('Ami', { exact: true })).toBeVisible()
})

Given("je suis sur le détail d'un événement avec ses tâches filtrées par catégorie", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await page.getByLabel('Filtrer par catégorie').selectOption('Courses')
  await expect(page.getByText('Bûche au chocolat')).toBeVisible()
})

Given("je suis sur le détail d'un événement avec le formulaire d'ajout de tâche rempli", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await page.getByLabel('Titre').fill('Guirlandes')
  await page.getByLabel('Catégorie').selectOption('Logistique')
  await page.getByLabel('Quantité').fill('2')
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
  await page.getByLabel('Inviter un participant').fill('ami@example.com')
  await page.getByRole('button', { name: 'Inviter' }).click()
  await expect(page.getByText('Cette personne est déjà invitée à cet événement.')).toBeVisible()
})
