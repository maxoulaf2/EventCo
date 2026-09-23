import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given } = createBdd()

Given("je suis sur la page d'administration des événements", async ({ page }) => {
  await page.route('**/api/admin/events', (route) =>
    route.fulfill({
      json: [
        {
          id: 'event-1',
          title: 'Repas de Noël',
          eventDate: '2026-12-24T00:00:00Z',
          location: 'Chez Alice',
          createdByUserId: 'user-1',
          status: 'Planned',
          participantCount: 2,
        },
        {
          id: 'event-3',
          title: 'Anniversaire de Bob',
          eventDate: '2027-02-10T00:00:00Z',
          location: null,
          createdByUserId: 'user-3',
          status: 'Planned',
          participantCount: 1,
        },
      ],
    }),
  )
  await page.goto('/admin/events')
  await expect(page.getByTestId('admin-event-list-item-event-1')).toBeVisible()
})
