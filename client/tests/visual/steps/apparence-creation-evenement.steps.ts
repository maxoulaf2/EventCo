import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given } = createBdd()

Given("je suis sur le formulaire de création d'événement", async ({ page }) => {
  await page.goto('/events/new')
})

Given("je suis sur le formulaire de création d'événement avec un message d'erreur", async ({ page }) => {
  await page.route('**/api/events', (route) =>
    route.fulfill({ status: 400, json: { detail: 'La date est obligatoire.' } }),
  )
  await page.goto('/events/new')
  await page.getByLabel('Titre').fill('Repas de Noël')
  await page.getByLabel('Date').fill('2026-12-24')
  await page.getByRole('button', { name: /créer l'événement/i }).click()
  await expect(page.getByText('La date est obligatoire.')).toBeVisible()
})
