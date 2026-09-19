import { useMutation, useQueryClient } from '@tanstack/react-query'
import { regenerateInviteLink } from '../api'

export function useRegenerateInviteLink(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => regenerateInviteLink(eventId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['events', eventId] }),
  })
}
