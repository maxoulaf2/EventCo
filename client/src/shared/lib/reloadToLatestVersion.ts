// Au-delà, on recharge quand même : au pire le service worker resservira l'ancien bundle, et le garde-fou
// de `checkServerVersion` évitera la boucle.
const SERVICE_WORKER_UPDATE_TIMEOUT_MS = 10_000

/**
 * Recharge la page sur le dernier build déployé. Un simple `location.reload()` ne suffit pas en PWA : le
 * service worker resservirait l'ancien `index.html` depuis son précache. On lui demande donc d'abord de
 * vérifier s'il existe une nouvelle version, et on attend qu'elle prenne le contrôle de la page
 * (`skipWaiting`/`clientsClaim` via `registerType: 'autoUpdate'`, cf. vite.config.ts).
 */
export async function reloadToLatestVersion(): Promise<void> {
  try {
    await updateServiceWorker()
  } finally {
    window.location.reload()
  }
}

async function updateServiceWorker(): Promise<void> {
  if (!('serviceWorker' in navigator)) return

  const registration = await navigator.serviceWorker.getRegistration()
  if (!registration) return

  // Écouté avant `update()` : le nouveau service worker peut prendre le contrôle avant qu'elle ne se résolve.
  const controllerChanged = new Promise<void>((resolve) => {
    navigator.serviceWorker.addEventListener('controllerchange', () => resolve(), { once: true })
    window.setTimeout(resolve, SERVICE_WORKER_UPDATE_TIMEOUT_MS)
  })

  await registration.update()

  if (registration.installing || registration.waiting) await controllerChanged
}
