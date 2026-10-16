import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { createEventHubConnection } from '../../../shared/lib/signalr'

// Connecte l'événement courant au Hub SignalR `EventHub` (`/hubs/events`) et recharge la liste des articles
// (clé partagée avec `useEventItems`) à chaque message reçu, pour que `ItemList` reflète en direct les
// changements faits par les autres participants. Recharger plutôt qu'appliquer le message au cache : l'ordre
// de la liste est décidé par le backend, et un article assigné ou recatégorisé doit changer de place.
export function useItemRealtime(eventId: string) {
  const queryClient = useQueryClient()

  useEffect(() => {
    const connection = createEventHubConnection()

    function reloadItems() {
      void queryClient.invalidateQueries({ queryKey: ['events', eventId, 'items'] })
    }

    connection.on('ItemCreated', reloadItems)
    connection.on('ItemAssigned', reloadItems)
    connection.on('ItemUnassigned', reloadItems)
    connection.on('ItemCategoryCorrected', reloadItems)
    connection.on('ItemDeleted', reloadItems)

    connection
      .start()
      .then(() => connection.invoke('JoinEvent', eventId))
      .catch((error: unknown) => console.error('Connexion au hub temps réel impossible', error))

    return () => {
      connection.stop().catch(() => undefined)
    }
  }, [eventId, queryClient])
}
