import { useMutation, useQueryClient } from '@tanstack/react-query'
import { correctItemCategory } from '../api'
import type { EventItem } from '../types'

// Optimiste comme useUnassignItem, même raison (réactivité mobile), useItemRealtime confirme ensuite via le Hub.
export function useCorrectItemCategory(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'items']

  return useMutation({
    mutationFn: ({ itemId, category }: { itemId: string; category: string }) =>
      correctItemCategory(eventId, itemId, category),
    onMutate: async ({ itemId, category }) => {
      const previousItems = queryClient.getQueryData<EventItem[]>(queryKey)
      queryClient.setQueryData<EventItem[]>(queryKey, (items) =>
        items?.map((item) => (item.id === itemId ? { ...item, category } : item)),
      )
      return { previousItems }
    },
    onError: (_error, _variables, context) => {
      if (context?.previousItems) {
        queryClient.setQueryData(queryKey, context.previousItems)
      }
    },
  })
}
