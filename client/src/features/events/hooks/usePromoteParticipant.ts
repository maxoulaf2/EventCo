import { useMutation, useQueryClient } from '@tanstack/react-query'
import { promoteParticipant } from '../api'

export function usePromoteParticipant(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (userId: string) => promoteParticipant(eventId, userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['events', eventId] }),
  })
}
