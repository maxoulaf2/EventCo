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
