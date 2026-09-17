import { apiFetch } from '../../shared/lib/api'
import type { CreateTaskInput, EventTask } from './types'

export function getEventTasks(eventId: string): Promise<EventTask[]> {
  return apiFetch<EventTask[]>(`/api/events/${eventId}/tasks`)
}

export function createTask(eventId: string, input: CreateTaskInput): Promise<EventTask> {
  return apiFetch<EventTask>(`/api/events/${eventId}/tasks`, {
    method: 'POST',
    body: JSON.stringify({ ...input, quantity: input.quantity || null }),
  })
}
