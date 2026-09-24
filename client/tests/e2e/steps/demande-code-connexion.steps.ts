import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given, When, Then } = createBdd()

Given('je suis sur la page de connexion', async ({ page }) => {
  await page.goto('/')
})

When('je saisis un email valide et je valide le formulaire', async ({ page }) => {
  // Email unique par run pour rester indépendant des autres scénarios (pas de reset de base entre runs),
  // même convention que tests/EventCo.Api.Tests côté backend.
  const email = `e2e-${Date.now()}@example.com`
  await page.getByTestId('request-login-code-email-input').fill(email)
  await page.getByTestId('request-login-code-submit-button').click()
})

Then("je vois la page de confirmation d'envoi", async ({ page }) => {
  await expect(page.getByTestId('check-email-page-title')).toBeVisible()
})
