import { useMutation, useQueryClient } from '@tanstack/react-query'
import { deleteItem } from '../api'
import type { EventItem } from '../types'

// Optimiste comme useUnassignItem, même raison (réactivité mobile), useItemRealtime confirme ensuite via le Hub.
export function useDeleteItem(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'items']

  return useMutation({
    mutationFn: (itemId: string) => deleteItem(eventId, itemId),
    onMutate: async (itemId: string) => {
      const previousItems = queryClient.getQueryData<EventItem[]>(queryKey)
      queryClient.setQueryData<EventItem[]>(queryKey, (items) => items?.filter((item) => item.id !== itemId))
      return { previousItems }
    },
    onError: (_error, _itemId, context) => {
      if (context?.previousItems) {
        queryClient.setQueryData(queryKey, context.previousItems)
      }
    },
  })
}
