import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given, Then } = createBdd()

Given('je suis sur la page de connexion', async ({ page }) => {
  await page.goto('/')
})

Given("je suis sur la page de confirmation d'envoi", async ({ page }) => {
  await page.route('**/api/auth/request-link', (route) => route.fulfill({ status: 202 }))
  await page.goto('/')
  await page.getByLabel('Adresse email').fill('visual@example.com')
  await page.getByRole('button', { name: /recevoir un lien/i }).click()
  await expect(page.getByRole('heading', { name: /vérifiez votre boîte mail/i })).toBeVisible()
})

Then('son apparence correspond à la référence enregistrée', async ({ page }) => {
  // Pas de nom explicite : Playwright dérive le nom du screenshot du titre du
  // scénario (+ projet/viewport), ce qui suffit à distinguer les captures.
  await expect(page).toHaveScreenshot({ fullPage: true })
})
