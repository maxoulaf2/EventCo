export type EventStatus = 'Draft' | 'Planned' | 'Completed' | 'Cancelled'

// Affiché à la place de la date tant que le sondage de date d'un événement est ouvert (eventDate null).
export const DATE_UNDER_VOTE_LABEL = 'Date en cours de vote'

export interface MyEvent {
  id: string
  title: string
  eventDate: string | null
  location: string | null
  createdByUserId: string
  status: EventStatus
  role: 'Organizer' | 'Participant'
}

// Date (AAAA-MM-JJ) et heure (HH:MM, vide = minuit) telles que saisies, en heure locale.
export interface DateOptionInput {
  date: string
  time: string
}

// Soit une date fixe (eventDate, eventTime), soit un sondage de date (dateOptions).
export interface CreateEventInput {
  title: string
  description?: string
  eventDate?: string
  eventTime?: string
  dateOptions?: DateOptionInput[]
  location?: string
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
  avatarUrl: string | null
}

export interface EventDateOption {
  id: string
  date: string
  voterUserIds: string[]
}

export interface EventDetail {
  id: string
  title: string
  description: string | null
  eventDate: string | null
  location: string | null
  imageUrl: string | null
  createdByUserId: string
  status: EventStatus
  createdAt: string
  participants: EventParticipant[]
  inviteLinkToken: string | null
  dateOptions: EventDateOption[]
}

export interface EventInvitePreview {
  eventId: string
  title: string
  eventDate: string | null
  location: string | null
  createdByDisplayName: string
}
