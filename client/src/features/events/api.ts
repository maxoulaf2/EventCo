import { apiFetch } from '../../shared/lib/api'
import type { CreateEventInput, MyEvent } from './types'

export function getMyEvents(): Promise<MyEvent[]> {
  return apiFetch<MyEvent[]>('/api/events')
}

export function createEvent(input: CreateEventInput): Promise<{ id: string }> {
  return apiFetch<{ id: string }>('/api/events', {
    method: 'POST',
    body: JSON.stringify({ ...input, eventDate: `${input.eventDate}T00:00:00.000Z` }),
  })
}
