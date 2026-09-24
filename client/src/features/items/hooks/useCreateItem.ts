import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createItem } from '../api'
import type { CreateItemInput, EventItem } from '../types'

export function useCreateItem(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateItemInput) => createItem(eventId, input),
    onSuccess: (item) => {
      const queryKey = ['events', eventId, 'items']
      queryClient.setQueryData<EventItem[]>(queryKey, (items) => {
        if (!items) {
          return items
        }
        return items.some((existing) => existing.id === item.id) ? items : [...items, item]
      })
    },
  })
}
