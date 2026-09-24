import { useMutation, useQueryClient } from '@tanstack/react-query'
import { verifyLoginCode } from '../api'

export function useVerifyLoginCode() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ email, code }: { email: string; code: string }) => verifyLoginCode(email, code),
    // La page de connexion a mis en cache le 401 de GET /api/auth/me : sans ce retrait, les pages
    // suivantes (même session SPA, sans rechargement de page) verraient
    // encore l'utilisateur comme non connecté.
    onSuccess: () => queryClient.removeQueries({ queryKey: ['auth', 'me'] }),
  })
}
