import type { EventStatus } from '../events/types'

export interface AdminEventSummary {
  id: string
  title: string
  eventDate: string
  location: string | null
  createdByUserId: string
  status: EventStatus
  participantCount: number
}
