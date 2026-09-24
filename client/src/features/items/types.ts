export interface CreateItemInput {
  title: string
  quantity?: string
}

export interface EventItem {
  id: string
  eventId: string
  title: string
  quantity: string | null
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
  assignedToUserId: string | null
  createdAt: string
}

export interface ItemDeletedRealtimePayload {
  eventId: string
  itemId: string
}
