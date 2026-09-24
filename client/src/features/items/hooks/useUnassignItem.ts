import { useMutation, useQueryClient } from '@tanstack/react-query'
import { unassignItem } from '../api'
import type { EventItem } from '../types'

// Optimiste comme useAssignItem, même raison (réactivité mobile), useItemRealtime confirme ensuite via le Hub.
export function useUnassignItem(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'items']

  return useMutation({
    mutationFn: (itemId: string) => unassignItem(eventId, itemId),
    onMutate: async (itemId: string) => {
      const previousItems = queryClient.getQueryData<EventItem[]>(queryKey)
      queryClient.setQueryData<EventItem[]>(queryKey, (items) =>
        items?.map((item) => (item.id === itemId ? { ...item, assignedToUserId: null } : item)),
      )
      return { previousItems }
    },
    onError: (_error, _itemId, context) => {
      if (context?.previousItems) {
        queryClient.setQueryData(queryKey, context.previousItems)
      }
    },
  })
}
