import { apiFetch } from '../../shared/lib/api'
import type { CreateEventInput, DateOptionInput, EventDetail, EventInvitePreview, MyEvent, ParticipationStatus } from './types'

export function getMyEvents(): Promise<MyEvent[]> {
  return apiFetch<MyEvent[]>('/api/events')
}

// Date et heure sont saisies en heure locale : `new Date` sans suffixe les interprète comme telles,
// `toISOString` les convertit en instant UTC (sinon 16h saisi à Paris est affiché 18h).
function toUtcIso(date: string, time?: string): string {
  return new Date(`${date}T${time || '00:00'}`).toISOString()
}

export function createEvent(input: CreateEventInput): Promise<{ id: string }> {
  const { eventDate, eventTime, dateOptions, ...rest } = input
  const when = dateOptions
    ? { dateOptions: dateOptions.map((option) => toUtcIso(option.date, option.time)) }
    : { eventDate: toUtcIso(eventDate!, eventTime) }

  return apiFetch<{ id: string }>('/api/events', {
    method: 'POST',
    body: JSON.stringify({ ...rest, ...when }),
  })
}

export function addDateOption(eventId: string, option: DateOptionInput): Promise<void> {
  return apiFetch(`/api/events/${eventId}/date-options`, {
    method: 'POST',
    body: JSON.stringify({ date: toUtcIso(option.date, option.time) }),
  })
}

export function removeDateOption(eventId: string, dateOptionId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/date-options/${dateOptionId}`, { method: 'DELETE' })
}

export function voteForDateOption(eventId: string, dateOptionId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/date-options/${dateOptionId}/vote`, { method: 'PUT' })
}

export function withdrawDateVote(eventId: string, dateOptionId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/date-options/${dateOptionId}/vote`, { method: 'DELETE' })
}

export function chooseEventDate(eventId: string, dateOptionId: string): Promise<void> {
  return apiFetch(`/api/events/${eventId}/date-options/${dateOptionId}/choose`, { method: 'POST' })
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

export function updateEventImage(eventId: string, image: Blob): Promise<{ eventId: string; imageUrl: string }> {
  const formData = new FormData()
  formData.append('file', image, 'image.jpg')

  return apiFetch(`/api/events/${eventId}/image`, {
    method: 'PUT',
    body: formData,
  })
}
