import { useMutation, useQueryClient } from '@tanstack/react-query'
import { unassignTask } from '../api'
import type { EventTask } from '../types'

// Optimiste comme useAssignTask, même raison (réactivité mobile), useTaskRealtime confirme ensuite via le Hub.
export function useUnassignTask(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'tasks']

  return useMutation({
    mutationFn: (taskId: string) => unassignTask(eventId, taskId),
    onMutate: async (taskId: string) => {
      const previousTasks = queryClient.getQueryData<EventTask[]>(queryKey)
      queryClient.setQueryData<EventTask[]>(queryKey, (tasks) =>
        tasks?.map((task) => (task.id === taskId ? { ...task, assignedToUserId: null } : task)),
      )
      return { previousTasks }
    },
    onError: (_error, _taskId, context) => {
      if (context?.previousTasks) {
        queryClient.setQueryData(queryKey, context.previousTasks)
      }
    },
  })
}
