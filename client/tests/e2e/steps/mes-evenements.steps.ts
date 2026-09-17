import { expect, type Page } from '@playwright/test'
import { createBdd } from 'playwright-bdd'

const { Given, When, Then } = createBdd()

// Le titre d'un événement créé via l'API n'est pas connu du DOM tant que son id ne l'est pas (le
// test-id du composant `EventsDashboardPage`, `event-list-item-{id}`, est construit sur l'id, pas
// le titre) : on retient l'id retourné par l'API de création, par page, pour le retrouver ensuite.
const createdEventIdsByPage = new WeakMap<Page, string>()

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
  await page.getByTestId('request-magic-link-email-input').fill(email)
  await page.getByTestId('request-magic-link-submit-button').click()
  await page.getByTestId('check-email-page-title').waitFor()

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

  const { id } = (await response.json()) as { id: string }
  createdEventIdsByPage.set(page, id)
})

When('je retourne sur le tableau de bord', async ({ page }) => {
  await page.reload()
})

Then('je vois {string} dans la liste de mes événements', async ({ page }, title: string) => {
  const eventId = createdEventIdsByPage.get(page)
  if (!eventId) {
    throw new Error("Aucun événement créé via l'API pour cette page : le step de création doit précéder celui-ci.")
  }
  const eventItem = page.getByTestId(`event-list-item-${eventId}`)
  await expect(eventItem).toBeVisible()
  await expect(eventItem).toContainText(title)
})

Then("je vois un message m'indiquant que je ne participe à aucun événement", async ({ page }) => {
  await expect(page.getByTestId('events-dashboard-empty-message')).toBeVisible()
})
