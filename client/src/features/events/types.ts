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
