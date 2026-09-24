import { useQuery } from '@tanstack/react-query'
import { getEventItems } from '../api'

export function useEventItems(eventId: string) {
  return useQuery({
    queryKey: ['events', eventId, 'items'],
    queryFn: () => getEventItems(eventId),
  })
}
