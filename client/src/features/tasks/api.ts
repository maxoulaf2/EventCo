import { apiFetch } from '../../shared/lib/api'
import type { EventTask } from './types'

export function getEventTasks(eventId: string): Promise<EventTask[]> {
  return apiFetch<EventTask[]>(`/api/events/${eventId}/tasks`)
}
