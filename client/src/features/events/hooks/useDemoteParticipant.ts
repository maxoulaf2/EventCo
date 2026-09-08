import { useMutation, useQueryClient } from '@tanstack/react-query'
import { demoteParticipant } from '../api'

export function useDemoteParticipant(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (userId: string) => demoteParticipant(eventId, userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['events', eventId] }),
  })
}
