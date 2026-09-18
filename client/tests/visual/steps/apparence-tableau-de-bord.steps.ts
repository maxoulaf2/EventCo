import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given } = createBdd()

Given(
  "je suis sur le tableau de bord avec des événements, dont un que j'organise",
  async ({ page }) => {
    await page.route('**/api/events', (route) =>
      route.fulfill({
        json: [
          {
            id: 'event-1',
            title: 'Repas de Noël',
            eventDate: '2026-12-24T00:00:00Z',
            location: 'Chez Alice',
            createdByUserId: 'user-1',
            status: 'Planned',
            role: 'Organizer',
          },
          {
            id: 'event-2',
            title: 'Weekend au ski',
            eventDate: '2027-01-10T00:00:00Z',
            location: null,
            createdByUserId: 'user-2',
            status: 'Planned',
            role: 'Participant',
          },
        ],
      }),
    )
    await page.goto('/events')
    await expect(page.getByTestId('event-list-item-event-1')).toBeVisible()
  },
)

Given('je suis sur le tableau de bord sans événement', async ({ page }) => {
  await page.route('**/api/events', (route) => route.fulfill({ json: [] }))
  await page.goto('/events')
  await expect(page.getByTestId('events-dashboard-empty-message')).toBeVisible()
})

Given("j'ai ouvert la modale de mon compte depuis le tableau de bord", async ({ page }) => {
  await page.route('**/api/events', (route) => route.fulfill({ json: [] }))
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({ json: { userId: 'user-1', email: 'test@example.com', displayName: 'Test' } }),
  )
  await page.goto('/events')
  await page.getByTestId('events-dashboard-account-button').click()
  await expect(page.getByTestId('account-modal-logout-button')).toBeVisible()
})
