export type TaskCategory = 'Courses' | 'Logistique' | 'Autre'

export interface EventTask {
  id: string
  eventId: string
  title: string
  category: TaskCategory
  quantity: string | null
  assignedToUserId: string | null
  isDone: boolean
  createdAt: string
}

// Formes des messages envoyés par le Hub SignalR `EventHub` (`TaskRealtimeDto`/`TaskDeletedRealtimeDto`
// côté backend) — `taskId` plutôt que `id`, d'où le mapping vers `EventTask` dans `useTaskRealtime`.
export interface TaskRealtimePayload {
  taskId: string
  eventId: string
  title: string
  category: TaskCategory
  quantity: string | null
  assignedToUserId: string | null
  isDone: boolean
  createdAt: string
}

export interface TaskDeletedRealtimePayload {
  eventId: string
  taskId: string
}
