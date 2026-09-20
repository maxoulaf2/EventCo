import { useMutation, useQueryClient } from '@tanstack/react-query'
import { assignTask } from '../api'
import type { EventTask } from '../types'

interface AssignTaskInput {
  taskId: string
  userId: string
}

// Auto-assignation ("Je prends") : optimiste pour la réactivité mobile,
// useTaskRealtime confirme ensuite via le Hub.
export function useAssignTask(eventId: string) {
  const queryClient = useQueryClient()
  const queryKey = ['events', eventId, 'tasks']

  return useMutation({
    mutationFn: ({ taskId, userId }: AssignTaskInput) => assignTask(eventId, taskId, userId),
    onMutate: async ({ taskId, userId }: AssignTaskInput) => {
      const previousTasks = queryClient.getQueryData<EventTask[]>(queryKey)
      queryClient.setQueryData<EventTask[]>(queryKey, (tasks) =>
        tasks?.map((task) => (task.id === taskId ? { ...task, assignedToUserId: userId } : task)),
      )
      return { previousTasks }
    },
    onError: (_error, _vars, context) => {
      if (context?.previousTasks) {
        queryClient.setQueryData(queryKey, context.previousTasks)
      }
    },
  })
}
