/**
 * Chemins de routes de l'application. Source unique de vérité : à utiliser à la
 * fois pour déclarer les <Route> dans App.tsx et pour naviguer (via useAppNavigate),
 * afin d'éviter les chaînes de caractères "en dur" dispersées dans le code.
 */
export const routes = {
  login: '/',
  checkEmail: '/auth/check-email',
  verifyMagicLink: '/auth/verify',
  events: '/events',
} as const

/**
 * State de navigation attendu par la route `checkEmail`. Colocalisé avec `routes`
 * pour que l'émetteur (useAppNavigate) et le lecteur (useLocationState) partagent
 * le même contrat de type.
 */
export interface CheckEmailNavigationState {
  email: string
}
