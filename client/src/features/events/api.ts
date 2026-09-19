import { apiFetch } from '../../shared/lib/api'
import type { CreateEventInput, EventDetail, EventInvitePreview, MyEvent, ParticipationStatus } from './types'

export function getMyEvents(): Promise<MyEvent[]> {
  return apiFetch<MyEvent[]>('/api/events')
}

export function createEvent(input: CreateEventInput): Promise<{ id: string }> {
  const { eventTime, ...rest } = input
  return apiFetch<{ id: string }>('/api/events', {
    method: 'POST',
    body: JSON.stringify({ ...rest, eventDate: `${input.eventDate}T${eventTime || '00:00'}:00.000Z` }),
  })
}

export function getEventById(eventId: string): Promise<EventDetail> {
  return apiFetch<EventDetail>(`/api/events/${eventId}`)
}

export function promoteParticipant(eventId: string, userId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/participants/${userId}/promote`, { method: 'POST' })
}

export function demoteParticipant(eventId: string, userId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/participants/${userId}/demote`, { method: 'POST' })
}

export function inviteParticipant(eventId: string, email: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/participants`, {
    method: 'POST',
    body: JSON.stringify({ email }),
  })
}

export function setParticipationStatus(eventId: string, status: ParticipationStatus): Promise<void> {
  return apiFetch(`/api/events/${eventId}/participation-status`, {
    method: 'PUT',
    body: JSON.stringify({ status }),
  })
}

export function regenerateInviteLink(eventId: string): Promise<{ eventId: string; inviteLinkToken: string }> {
  return apiFetch(`/api/events/${eventId}/invite-link/regenerate`, { method: 'POST' })
}

export function getEventInvitePreview(token: string): Promise<EventInvitePreview> {
  return apiFetch<EventInvitePreview>(`/api/events/invite-links/${token}/preview`)
}

export function joinEventViaInviteLink(token: string): Promise<{ eventId: string }> {
  return apiFetch(`/api/events/invite-links/${token}/join`, { method: 'POST' })
}
