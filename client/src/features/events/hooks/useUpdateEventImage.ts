import { useMutation, useQueryClient } from '@tanstack/react-query'
import { updateEventImage } from '../api'

export function useUpdateEventImage() {
  const queryClient = useQueryClient()

  return useMutation({
    // Image déjà réduite par prepareEventImage au moment du choix du fichier (cf. CreateEventPage).
    mutationFn: ({ eventId, image }: { eventId: string; image: Blob }) => updateEventImage(eventId, image),
    onSuccess: (_, { eventId }) => queryClient.invalidateQueries({ queryKey: ['events', eventId] }),
  })
}
