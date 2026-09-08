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

function mockEventDetail(page: import('@playwright/test').Page) {
  return page.route('**/api/events/event-1', (route) =>
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
