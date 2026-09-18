import { useMutation, useQueryClient } from '@tanstack/react-query'
import { setParticipationStatus } from '../api'
import type { ParticipationStatus } from '../types'

export function useSetParticipationStatus(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (status: ParticipationStatus) => setParticipationStatus(eventId, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['events', eventId] }),
  })
}
