import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { createEventHubConnection } from '../../../shared/lib/signalr'
import type { EventItem, ItemDeletedRealtimePayload, ItemRealtimePayload } from '../types'

function toEventItem(payload: ItemRealtimePayload): EventItem {
  return {
    id: payload.itemId,
    eventId: payload.eventId,
    title: payload.title,
    quantity: payload.quantity,
    kind: payload.kind,
    assignedToUserId: payload.assignedToUserId,
    createdAt: payload.createdAt,
  }
}

// Connecte l'événement courant au Hub SignalR `EventHub` (`/hubs/events`) et tient à jour le cache
// React Query des articles (clé partagée avec `useEventItems`) au fil des messages reçus, pour que
// `ItemList` reflète en direct les changements faits par les autres participants.
export function useItemRealtime(eventId: string) {
  const queryClient = useQueryClient()

  useEffect(() => {
    const queryKey = ['events', eventId, 'items']
    const connection = createEventHubConnection()

    function upsertItem(payload: ItemRealtimePayload) {
      const item = toEventItem(payload)
      queryClient.setQueryData<EventItem[]>(queryKey, (items) => {
        if (!items) {
          return items
        }
        const index = items.findIndex((existing) => existing.id === item.id)
        if (index === -1) {
          return [...items, item]
        }
        return items.map((existing, i) => (i === index ? item : existing))
      })
    }

    function removeItem({ itemId }: ItemDeletedRealtimePayload) {
      queryClient.setQueryData<EventItem[]>(queryKey, (items) => items?.filter((item) => item.id !== itemId))
    }

    connection.on('ItemCreated', upsertItem)
    connection.on('ItemAssigned', upsertItem)
    connection.on('ItemUnassigned', upsertItem)
    connection.on('ItemDeleted', removeItem)

    connection
      .start()
      .then(() => connection.invoke('JoinEvent', eventId))
      .catch((error: unknown) => console.error('Connexion au hub temps réel impossible', error))

    return () => {
      connection.stop().catch(() => undefined)
    }
  }, [eventId, queryClient])
}
