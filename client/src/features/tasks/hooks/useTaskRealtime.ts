import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { createEventHubConnection } from '../../../shared/lib/signalr'
import type { EventTask, TaskDeletedRealtimePayload, TaskRealtimePayload } from '../types'

function toEventTask(payload: TaskRealtimePayload): EventTask {
  return {
    id: payload.taskId,
    eventId: payload.eventId,
    title: payload.title,
    quantity: payload.quantity,
    assignedToUserId: payload.assignedToUserId,
    createdAt: payload.createdAt,
  }
}

// Connecte l'événement courant au Hub SignalR `EventHub` (`/hubs/events`) et tient à jour le cache
// React Query des tâches (clé partagée avec `useEventTasks`) au fil des messages reçus, pour que
// `TaskList` reflète en direct les changements faits par les autres participants.
export function useTaskRealtime(eventId: string) {
  const queryClient = useQueryClient()

  useEffect(() => {
    const queryKey = ['events', eventId, 'tasks']
    const connection = createEventHubConnection()

    function upsertTask(payload: TaskRealtimePayload) {
      const task = toEventTask(payload)
      queryClient.setQueryData<EventTask[]>(queryKey, (tasks) => {
        if (!tasks) {
          return tasks
        }
        const index = tasks.findIndex((existing) => existing.id === task.id)
        if (index === -1) {
          return [...tasks, task]
        }
        return tasks.map((existing, i) => (i === index ? task : existing))
      })
    }

    function removeTask({ taskId }: TaskDeletedRealtimePayload) {
      queryClient.setQueryData<EventTask[]>(queryKey, (tasks) => tasks?.filter((task) => task.id !== taskId))
    }

    connection.on('TaskCreated', upsertTask)
    connection.on('TaskAssigned', upsertTask)
    connection.on('TaskUnassigned', upsertTask)
    connection.on('TaskDeleted', removeTask)

    connection
      .start()
      .then(() => connection.invoke('JoinEvent', eventId))
      .catch((error: unknown) => console.error('Connexion au hub temps réel impossible', error))

    return () => {
      connection.stop().catch(() => undefined)
    }
  }, [eventId, queryClient])
}
