import { useQuery } from '@tanstack/react-query'
import { getEventInvitePreview } from '../api'

export function useEventInvitePreview(token: string) {
  return useQuery({
    queryKey: ['invite-preview', token],
    queryFn: () => getEventInvitePreview(token),
    // Un lien invalide/régénéré est un état normal à distinguer immédiatement (cf. useCurrentUser) :
    // pas de retries automatiques.
    retry: false,
  })
}
