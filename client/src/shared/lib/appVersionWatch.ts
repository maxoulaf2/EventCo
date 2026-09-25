import { apiFetch } from './api'

/**
 * Interroge l'API pour sa version : l'en-tête de version de la réponse suffit à déclencher le rechargement
 * si besoin (cf. `apiFetch`). Une erreur (API en cours de redéploiement, hors ligne) est ignorée.
 */
export async function checkAppVersion(): Promise<void> {
  await apiFetch('/api/version', { cache: 'no-store' }).catch(() => undefined)
}

/**
 * Vérifie la version au retour sur l'onglet : cas typique d'une PWA restée ouverte en arrière-plan pendant
 * un déploiement, qui n'a fait aucun appel à l'API depuis. Retourne la fonction de désabonnement.
 */
export function watchAppVersion(): () => void {
  const onVisibilityChange = () => {
    if (document.visibilityState === 'visible') void checkAppVersion()
  }

  document.addEventListener('visibilitychange', onVisibilityChange)
  return () => document.removeEventListener('visibilitychange', onVisibilityChange)
}
