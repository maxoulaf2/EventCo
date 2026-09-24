import { apiFetch } from '../../shared/lib/api'
import type { CreateItemInput, EventItem } from './types'

export function getEventItems(eventId: string): Promise<EventItem[]> {
  return apiFetch<EventItem[]>(`/api/events/${eventId}/items`)
}

export function createItem(eventId: string, input: CreateItemInput): Promise<EventItem> {
  return apiFetch<EventItem>(`/api/events/${eventId}/items`, {
    method: 'POST',
    body: JSON.stringify({ ...input, quantity: input.quantity || null }),
  })
}

export function assignItem(eventId: string, itemId: string, userId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/items/${itemId}/assign/${userId}`, { method: 'POST' })
}

export function unassignItem(eventId: string, itemId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/items/${itemId}/unassign`, { method: 'POST' })
}
