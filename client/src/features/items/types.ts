// « À prendre » : demande d'un (co-)organisateur, que n'importe quel participant peut prendre.
// « Contribution » : ce qu'un participant déclare apporter, attribué à lui définitivement (annulable seulement).
export type EventItemKind = 'ToBring' | 'Contribution'

export interface CreateItemInput {
  title: string
  quantity?: string
  kind: EventItemKind
}

export interface EventItem {
  id: string
  eventId: string
  title: string
  quantity: string | null
  kind: EventItemKind
  assignedToUserId: string | null
  createdAt: string
}

// Formes des messages envoyés par le Hub SignalR `EventHub` (`ItemRealtimeDto`/`ItemDeletedRealtimeDto`
// côté backend) — `itemId` plutôt que `id`, d'où le mapping vers `EventItem` dans `useItemRealtime`.
export interface ItemRealtimePayload {
  itemId: string
  eventId: string
  title: string
  quantity: string | null
  kind: EventItemKind
  assignedToUserId: string | null
  createdAt: string
}

export interface ItemDeletedRealtimePayload {
  eventId: string
  itemId: string
}
