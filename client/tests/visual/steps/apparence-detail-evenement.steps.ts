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
  // TaskList ouvre une connexion SignalR (`useTaskRealtime`) vers `/hubs/events`, proxifiée par Vite
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

Given("je suis sur le détail d'un événement avec l'onglet des tâches faites", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await page.getByTestId('task-list-tab-done').click()
  await expect(page.getByTestId('task-item-task-2')).toBeVisible()
})

Given("je suis sur le détail d'un événement avec le formulaire d'ajout de tâche rempli", async ({ page }) => {
  await mockEventDetail(page)
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur' } }),
  )
  await page.goto('/events/event-1')
  await page.getByTestId('add-task-title-input').fill('Guirlandes')
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
