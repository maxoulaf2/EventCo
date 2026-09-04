import { useNavigate } from 'react-router-dom'
import { routes, type CheckEmailNavigationState } from '../lib/routes'

/**
 * Point d'entrée unique pour naviguer entre les pages. Chaque route qui a besoin
 * d'un state de navigation expose ici une méthode dédiée et typée, pour ne jamais
 * avoir à passer un objet `state` non typé à `navigate()` ailleurs dans le code.
 */
export function useAppNavigate() {
  const navigate = useNavigate()

  return {
    toLogin: () => navigate(routes.login),
    toCheckEmail: (state: CheckEmailNavigationState) => navigate(routes.checkEmail, { state }),
    toVerifyMagicLink: () => navigate(routes.verifyMagicLink),
    toEvents: () => navigate(routes.events),
  }
}
