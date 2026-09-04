import { useQuery } from '@tanstack/react-query'
import { getMyEvents } from '../api'

export function useMyEvents() {
  return useQuery({
    queryKey: ['events', 'mine'],
    queryFn: getMyEvents,
  })
}
