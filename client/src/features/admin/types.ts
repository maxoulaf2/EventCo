import type { EventStatus } from '../events/types'

export interface AdminEventSummary {
  id: string
  title: string
  eventDate: string | null
  location: string | null
  createdByUserId: string
  status: EventStatus
  participantCount: number
}
