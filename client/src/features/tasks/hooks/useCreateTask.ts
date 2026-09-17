import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createTask } from '../api'
import type { CreateTaskInput, EventTask } from '../types'

export function useCreateTask(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateTaskInput) => createTask(eventId, input),
    onSuccess: (task) => {
      const queryKey = ['events', eventId, 'tasks']
      queryClient.setQueryData<EventTask[]>(queryKey, (tasks) => {
        if (!tasks) {
          return tasks
        }
        return tasks.some((existing) => existing.id === task.id) ? tasks : [...tasks, task]
      })
    },
  })
}
