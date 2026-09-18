export type EventStatus = 'Draft' | 'Planned' | 'Completed' | 'Cancelled'

export interface MyEvent {
  id: string
  title: string
  eventDate: string
  location: string | null
  createdByUserId: string
  status: EventStatus
  role: 'Organizer' | 'Participant'
}

export interface CreateEventInput {
  title: string
  description?: string
  eventDate: string
  location?: string
  imageUrl?: string
}

export type ParticipantRole = 'Organizer' | 'Participant'

export type ParticipationStatus = 'Unknown' | 'Attending' | 'NotAttending'

export interface EventParticipant {
  userId: string
  email: string
  displayName: string
  role: ParticipantRole
  invitedAt: string
  participationStatus: ParticipationStatus
}

export interface EventDetail {
  id: string
  title: string
  description: string | null
  eventDate: string
  location: string | null
  imageUrl: string | null
  createdByUserId: string
  status: EventStatus
  createdAt: string
  participants: EventParticipant[]
}
