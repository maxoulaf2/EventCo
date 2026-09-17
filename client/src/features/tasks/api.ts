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

export function assignTask(eventId: string, taskId: string, userId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/tasks/${taskId}/assign/${userId}`, { method: 'POST' })
}

export function completeTask(eventId: string, taskId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/tasks/${taskId}/complete`, { method: 'POST' })
}

export function reopenTask(eventId: string, taskId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/tasks/${taskId}/reopen`, { method: 'POST' })
}
