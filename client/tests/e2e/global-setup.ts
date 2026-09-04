import { execSync } from 'node:child_process'
import path from 'node:path'

const repoRoot = path.resolve(import.meta.dirname, '../../..')
const composeFile = path.join(repoRoot, 'docker-compose.e2e.yml')
const apiUrl = process.env.VITE_API_URL ?? 'http://localhost:5001'
const mailpitUrl = process.env.MAILPIT_URL ?? 'http://localhost:8025'
const e2eConnectionString =
  'Host=localhost;Port=5433;Database=eventco_e2e;Username=eventco;Password=eventco'

/**
 * Démarre une stack dédiée (postgres-e2e + mailpit-e2e + api-e2e, ports et volume propres)
 * avant l'E2E, pour ne jamais toucher à la base de dev (`docker compose up -d postgres api`).
 * `--build`
 * garantit une image API à jour (piège rencontré manuellement en mettant en place l'E2E :
 * une image obsolète répond 404 sur des routes pourtant existantes dans le code).
 *
 * La migration se fait ici (`dotnet ef database update`, même commande que celle documentée
 * dans CLAUDE.md pour la base de dev) plutôt que dans `Program.cs` : la base `eventco_e2e`
 * étant neuve à chaque run, il fallait bien la migrer quelque part, mais pas question d'ajouter
 * une branche « migre-toi au démarrage » au code de prod pour un besoin qui n'appartient qu'aux
 * tests.
 */
export default async function globalSetup() {
  execSync(`docker compose -f "${composeFile}" up -d --build --wait`, { stdio: 'inherit' })
  execSync(
    `dotnet ef database update --project src/EventCo.Infrastructure --startup-project src/EventCo.Api --connection "${e2eConnectionString}"`,
    { cwd: repoRoot, stdio: 'inherit' },
  )
  await waitForHttp(apiUrl)
  await waitForHttp(mailpitUrl)
}

async function waitForHttp(baseUrl: string, timeoutMs = 30000) {
  const deadline = Date.now() + timeoutMs
  while (Date.now() < deadline) {
    try {
      await fetch(baseUrl)
      return
    } catch {
      await new Promise((resolve) => setTimeout(resolve, 500))
    }
  }
  throw new Error(`${baseUrl} n'a pas répondu dans le délai imparti.`)
}
