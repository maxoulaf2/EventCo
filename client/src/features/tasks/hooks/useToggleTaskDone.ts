import { useMutation, useQueryClient } from '@tanstack/react-query'
import { completeTask, reopenTask } from '../api'
import type { EventTask } from '../types'

// Bascule optimiste (le badge change avant la réponse de l'API) pour rester réactif sur mobile ;
// `useTaskRealtime` confirme ensuite l'état via le Hub, `onError` restaure l'état précédent en cas d'échec.
export function useToggleTaskDone(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'tasks']

  return useMutation({
    mutationFn: (task: EventTask) => (task.isDone ? reopenTask(eventId, task.id) : completeTask(eventId, task.id)),
    onMutate: async (task: EventTask) => {
      const previousTasks = queryClient.getQueryData<EventTask[]>(queryKey)
      queryClient.setQueryData<EventTask[]>(queryKey, (tasks) =>
        tasks?.map((existing) => (existing.id === task.id ? { ...existing, isDone: !existing.isDone } : existing)),
      )
      return { previousTasks }
    },
    onError: (_error, _task, context) => {
      if (context?.previousTasks) {
        queryClient.setQueryData(queryKey, context.previousTasks)
      }
    },
  })
}
