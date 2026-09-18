import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { logout } from '../api'

export function useLogout() {
  const queryClient = useQueryClient()
  const { toLogin } = useAppNavigate()

  return useMutation({
    mutationFn: logout,
    onSuccess: () => {
      // Retire (plutôt qu'invalider) le cache : un refetch garderait potentiellement l'ancien
      // utilisateur affiché le temps de la requête, alors que la session vient d'être fermée.
      queryClient.removeQueries({ queryKey: ['auth', 'me'] })
      toLogin()
    },
  })
}
