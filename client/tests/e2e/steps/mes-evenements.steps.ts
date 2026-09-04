import { expect } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given, When, Then } = createBdd()

const apiUrl = process.env.VITE_API_URL ?? 'http://localhost:5001'
const mailpitUrl = process.env.MAILPIT_URL ?? 'http://localhost:8025'

interface MailpitMessageSummary {
  ID: string
}

interface MailpitMessage {
  HTML: string
}

// L'API e2e envoie réellement l'email (SmtpEmailSender, cf. docker-compose.e2e.yml) vers
// mailpit-e2e, un faux serveur SMTP dont on interroge l'API HTTP pour récupérer le lien de
// connexion — plus robuste qu'auparavant (lecture des logs Docker de l'API) : la recherche
// par destinataire isole correctement chaque scénario, y compris exécutés en parallèle
// (fullyParallel) contre le même conteneur.
async function extraireTokenDepuisMailpit(email: string): Promise<string> {
  const rechercheUrl = `${mailpitUrl}/api/v1/search?query=${encodeURIComponent(`to:${email}`)}`
  const recherche = await fetch(rechercheUrl)
  const { messages } = (await recherche.json()) as { messages: MailpitMessageSummary[] }
  const dernierMessage = messages[0]
  if (!dernierMessage) {
    throw new Error(`Aucun email de connexion trouvé dans Mailpit pour "${email}".`)
  }

  const messageRes = await fetch(`${mailpitUrl}/api/v1/message/${dernierMessage.ID}`)
  const message = (await messageRes.json()) as MailpitMessage
  const match = message.HTML.match(/token=([^"&\s]+)/)
  if (!match) {
    throw new Error(`Token de connexion introuvable dans l'email Mailpit pour "${email}".`)
  }
  return decodeURIComponent(match[1])
}

Given('je me connecte avec un lien magique', async ({ page }) => {
  // Email unique par run pour rester indépendant des autres scénarios, même convention
  // que demande-lien-connexion.steps.ts.
  const email = `e2e-events-${Date.now()}@example.com`

  await page.goto('/')
  await page.getByLabel('Adresse email').fill(email)
  await page.getByRole('button', { name: /recevoir un lien/i }).click()
  await page.getByRole('heading', { name: /vérifiez votre boîte mail/i }).waitFor()

  const token = await extraireTokenDepuisMailpit(email)
  await page.goto(`/auth/verify?token=${encodeURIComponent(token)}`)
  await page.waitForURL('**/events')
})

Given('un événement {string} créé via l\'API pour moi', async ({ page }, title: string) => {
  const sessionCookie = (await page.context().cookies()).find((c) => c.name === 'eventco_session')
  if (!sessionCookie) {
    throw new Error("Aucune session active : le step de connexion doit précéder celui-ci.")
  }

  const response = await fetch(`${apiUrl}/api/events`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Cookie: `eventco_session=${sessionCookie.value}` },
    body: JSON.stringify({
      title,
      description: null,
      eventDate: '2026-12-24T00:00:00Z',
      location: 'Chez Alice',
    }),
  })

  if (!response.ok) {
    throw new Error(`Création de l'événement "${title}" échouée : ${response.status} ${await response.text()}`)
  }
})

When('je retourne sur le tableau de bord', async ({ page }) => {
  await page.reload()
})

Then('je vois {string} dans la liste de mes événements', async ({ page }, title: string) => {
  await expect(page.getByText(title)).toBeVisible()
})

Then("je vois un message m'indiquant que je ne participe à aucun événement", async ({ page }) => {
  await expect(page.getByText(/vous ne participez encore à aucun événement/i)).toBeVisible()
})
