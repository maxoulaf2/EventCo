import { useLocation } from 'react-router-dom'

/**
 * Lit le state de navigation de la page courante, typé par `T`. À utiliser avec le
 * même type que celui passé côté émetteur (ex: `CheckEmailNavigationState` de
 * `shared/lib/routes.ts`) pour garder l'écriture et la lecture du state cohérentes.
 * Retourne `undefined` si la page a été atteinte sans navigation (ex: rechargement direct de l'URL).
 */
export function useLocationState<T>(): T | undefined {
  return (useLocation().state as T | null) ?? undefined
}
