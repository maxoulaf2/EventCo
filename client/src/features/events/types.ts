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

export type ParticipantRole = 'Organizer' | 'Participant'

export interface EventParticipant {
  userId: string
  email: string
  displayName: string
  role: ParticipantRole
  invitedAt: string
  hasJoined: boolean
}

export interface EventDetail {
  id: string
  title: string
  description: string | null
  eventDate: string
  location: string | null
  createdByUserId: string
  status: EventStatus
  createdAt: string
  participants: EventParticipant[]
}
