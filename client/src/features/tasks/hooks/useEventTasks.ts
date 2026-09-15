import { useQuery } from '@tanstack/react-query'
import { getEventTasks } from '../api'

export function useEventTasks(eventId: string) {
  return useQuery({
    queryKey: ['events', eventId, 'tasks'],
    queryFn: () => getEventTasks(eventId),
  })
}
