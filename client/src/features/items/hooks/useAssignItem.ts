import { useMutation, useQueryClient } from '@tanstack/react-query'
import { assignItem } from '../api'
import type { EventItem } from '../types'

interface AssignItemInput {
  itemId: string
  userId: string
}

// Auto-assignation ("Je prends") : optimiste pour la réactivité mobile,
// useItemRealtime confirme ensuite via le Hub.
export function useAssignItem(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'items']

  return useMutation({
    mutationFn: ({ itemId, userId }: AssignItemInput) => assignItem(eventId, itemId, userId),
    onMutate: async ({ itemId, userId }: AssignItemInput) => {
      const previousItems = queryClient.getQueryData<EventItem[]>(queryKey)
      queryClient.setQueryData<EventItem[]>(queryKey, (items) =>
        items?.map((item) => (item.id === itemId ? { ...item, assignedToUserId: userId } : item)),
      )
      return { previousItems }
    },
    onError: (_error, _vars, context) => {
      if (context?.previousItems) {
        queryClient.setQueryData(queryKey, context.previousItems)
      }
    },
  })
}
