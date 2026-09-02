import { execSync } from 'node:child_process'
import path from 'node:path'

const composeFile = path.resolve(import.meta.dirname, '../../../docker-compose.e2e.yml')

/** Arrête la stack e2e et supprime son volume (`-v`) : prochain run = base vierge, migrée à froid. */
export default function globalTeardown() {
  execSync(`docker compose -f "${composeFile}" down -v`, { stdio: 'inherit' })
}
