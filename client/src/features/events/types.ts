export type EventStatus = 'Draft' | 'Planned' | 'Completed' | 'Cancelled'

export interface MyEvent {
  id: string
  title: string
  eventDate: string
  location: string | null
  createdByUserId: string
  status: EventStatus
  role: 'Organizer' | 'Participant'
  hasJoined: boolean
}

export interface CreateEventInput {
  title: string
  description?: string
  eventDate: string
  location?: string
}
