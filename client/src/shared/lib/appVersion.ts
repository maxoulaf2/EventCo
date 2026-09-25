import { reloadToLatestVersion } from './reloadToLatestVersion'

/** En-tête posé par l'API sur toutes ses réponses (cf. `AppVersion` côté API). */
export const APP_VERSION_HEADER = 'X-App-Version'

// Valeur hors build Docker (dev local, tests) côté frontend comme côté API : désactive la détection.
const DEV_VERSION = 'dev'

const RELOAD_ATTEMPT_STORAGE_KEY = 'eventco:app-version-reload-attempt'

// Lue à chaque appel plutôt qu'en constante de module, pour que les tests puissent la simuler (vi.stubEnv).
function clientVersion(): string {
  return import.meta.env.VITE_APP_VERSION || DEV_VERSION
}

/**
 * Recharge silencieusement le frontend quand l'API répond dans une autre version que la sienne (déploiement
 * survenu pendant que l'onglet ou la PWA était ouvert). Frontend et API sont buildés dans la même image
 * Docker : une version différente signifie toujours que le bundle en cours d'exécution est périmé.
 */
export function checkServerVersion(serverVersion: string | null): void {
  const version = clientVersion()
  if (!serverVersion || serverVersion === DEV_VERSION || version === DEV_VERSION || serverVersion === version)
    return

  // Garde-fou contre une boucle de rechargement : si le bundle reste périmé après un premier rechargement
  // pour cette version (bascule du déploiement encore en cours, cache récalcitrant), on n'insiste pas —
  // le prochain déploiement (nouvelle version) retentera.
  if (readReloadAttempt() === serverVersion) return
  writeReloadAttempt(serverVersion)

  void reloadToLatestVersion()
}

function readReloadAttempt(): string | null {
  try {
    return sessionStorage.getItem(RELOAD_ATTEMPT_STORAGE_KEY)
  } catch {
    return null
  }
}

function writeReloadAttempt(serverVersion: string): void {
  try {
    sessionStorage.setItem(RELOAD_ATTEMPT_STORAGE_KEY, serverVersion)
  } catch {
    // sessionStorage indisponible (navigation privée stricte) : rechargement sans garde-fou.
  }
}
