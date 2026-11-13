import { expect, type Page } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given } = createBdd()

async function mockEventUnderVote(page: Page, currentUserId: 'user-1' | 'user-2') {
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: currentUserId, email: 'moi@example.com', displayName: 'Moi' } }),
  )
  await page.route('**/api/events/event-1', (route) =>
    route.fulfill({
      json: {
        id: 'event-1',
        title: 'Week-end à la mer',
        description: 'On choisit la date ensemble.',
        eventDate: null,
        location: 'La Rochelle',
        createdByUserId: 'user-1',
        status: 'Planned',
        createdAt: '2026-09-01T00:00:00Z',
        participants: [
          { userId: 'user-1', email: 'organisateur@example.com', displayName: 'Organisateur', role: 'Organizer', invitedAt: '2026-09-01T00:00:00Z', participationStatus: 'Attending' },
          { userId: 'user-2', email: 'ami@example.com', displayName: 'Ami', role: 'Participant', invitedAt: '2026-09-02T00:00:00Z', participationStatus: 'Unknown' },
        ],
        inviteLinkToken: null,
        dateOptions: [
          { id: 'option-1', date: '2027-06-05T10:00:00Z', voterUserIds: ['user-1', 'user-2'] },
          { id: 'option-2', date: '2027-06-12T10:00:00Z', voterUserIds: ['user-2'] },
          { id: 'option-3', date: '2027-06-19T10:00:00Z', voterUserIds: [] },
        ],
      },
    }),
  )
  await page.route('**/api/events/event-1/items', (route) => route.fulfill({ json: [] }))
  await page.route('**/api/item-categories', (route) => route.fulfill({ json: [] }))
  // Pas de backend en test visuel : cf. apparence-detail-evenement.steps.ts.
  await page.route('**/hubs/events/negotiate**', (route) => route.fulfill({ status: 404 }))

  await page.goto('/events/event-1')
  await expect(page.getByTestId('date-poll')).toBeVisible()
}

Given("je suis sur le formulaire de création d'événement avec plusieurs dates proposées", async ({ page }) => {
  await page.goto('/events/new')
  await page.getByTestId('create-event-date-mode-poll').check({ force: true })
  await page.getByTestId('create-event-title-input').fill('Week-end à la mer')
  await page.getByTestId('create-event-date-option-date-input-0').fill('2027-06-05')
  await page.getByTestId('create-event-date-option-date-input-1').fill('2027-06-12')
  await page.getByTestId('create-event-date-option-add-button').click()
  await expect(page.getByTestId('create-event-date-option-date-input-2')).toBeVisible()
})

Given('je suis sur le tableau de bord avec un événement en cours de vote', async ({ page }) => {
  await page.route('**/api/events', (route) =>
    route.fulfill({
      json: [
        { id: 'event-1', title: 'Week-end à la mer', eventDate: null, location: 'La Rochelle', createdByUserId: 'user-1', status: 'Planned', role: 'Organizer' },
        { id: 'event-2', title: 'Repas de Noël', eventDate: '2026-12-24T00:00:00Z', location: 'Chez Alice', createdByUserId: 'user-2', status: 'Planned', role: 'Participant' },
      ],
    }),
  )
  await page.goto('/events')
  await expect(page.getByTestId('event-list-item-date-poll-badge-event-1')).toBeVisible()
})

Given("je suis sur le détail d'un événement en cours de vote en tant qu'organisateur", async ({ page }) => {
  await mockEventUnderVote(page, 'user-1')
})

Given("je suis sur le détail d'un événement en cours de vote en tant que simple participant", async ({ page }) => {
  await mockEventUnderVote(page, 'user-2')
})
