import { useMutation, useQueryClient } from '@tanstack/react-query'
import { inviteParticipant } from '../api'

export function useInviteParticipant(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (email: string) => inviteParticipant(eventId, email),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['events', eventId] }),
  })
}
