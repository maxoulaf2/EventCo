import { useQuery } from '@tanstack/react-query'
import { getCurrentUser } from '../api'

export function useCurrentUser() {
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: getCurrentUser,
    // Un 401 est un état normal (pas encore/plus connecté), pas une erreur transitoire à réessayer :
    // sans ça, le comportement par défaut de React Query (3 tentatives avec backoff) retarde de
    // plusieurs secondes la détection de session absente/expirée.
    retry: false,
  })
}
