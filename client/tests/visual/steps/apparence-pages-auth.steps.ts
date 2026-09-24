import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given, Then } = createBdd()

Given('je suis sur la page de connexion', async ({ page }) => {
  await page.goto('/')
})

Given("je suis sur la page de confirmation d'envoi", async ({ page }) => {
  await page.route('**/api/auth/request-code', (route) => route.fulfill({ status: 202 }))
  await page.goto('/')
  await page.getByTestId('request-login-code-email-input').fill('visual@example.com')
  await page.getByTestId('request-login-code-submit-button').click()
  await expect(page.getByTestId('check-email-page-title')).toBeVisible()
})

Then('son apparence correspond à la référence enregistrée', async ({ page }) => {
  // Pas de nom explicite : Playwright dérive le nom du screenshot du titre du
  // scénario (+ projet/viewport), ce qui suffit à distinguer les captures.
  await expect(page).toHaveScreenshot({ fullPage: true })
})
