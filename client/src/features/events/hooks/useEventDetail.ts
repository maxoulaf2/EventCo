import { useQuery } from '@tanstack/react-query'
import { getEventById } from '../api'

export function useEventDetail(eventId: string) {
  return useQuery({
    queryKey: ['events', eventId],
    queryFn: () => getEventById(eventId),
  })
}
