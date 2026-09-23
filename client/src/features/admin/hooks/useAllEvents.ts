import { useQuery } from '@tanstack/react-query'
import { getAllEvents } from '../api'

export function useAllEvents() {
  return useQuery({
    queryKey: ['admin', 'events'],
    queryFn: getAllEvents,
  })
}
