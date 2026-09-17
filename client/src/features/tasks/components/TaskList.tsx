import { useState } from 'react'
import { useEventTasks } from '../hooks/useEventTasks'
import { useTaskRealtime } from '../hooks/useTaskRealtime'
import { useToggleTaskDone } from '../hooks/useToggleTaskDone'
import type { EventTask, TaskCategory } from '../types'

const CATEGORIES: TaskCategory[] = ['Courses', 'Logistique', 'Autre']

interface TaskListProps {
  eventId: string
  currentUserId: string | undefined
  canManageAllTasks: boolean
}

export function TaskList({ eventId, currentUserId, canManageAllTasks }: TaskListProps) {
  const { data: tasks, isPending, isError } = useEventTasks(eventId)
  useTaskRealtime(eventId)
  const toggleTaskDone = useToggleTaskDone(eventId)
  const [categoryFilter, setCategoryFilter] = useState<TaskCategory | 'Toutes'>('Toutes')

  const filteredTasks = tasks?.filter((task) => categoryFilter === 'Toutes' || task.category === categoryFilter) ?? []

  // Reflète `Event.EnsureActingUserCanToggleTaskDone` (Domain) : le créateur/co-organisateur peut cocher
  // n'importe quelle tâche, un simple participant seulement celles qui lui sont assignées.
  function canToggle(task: EventTask) {
    return canManageAllTasks || task.assignedToUserId === currentUserId
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center justify-between gap-2">
        <h2 className="text-sm font-semibold text-gray-900">Tâches ({tasks?.length ?? 0})</h2>
        <label className="flex items-center gap-2 text-xs text-gray-600">
          Filtrer par catégorie
          <select
            value={categoryFilter}
            onChange={(event) => setCategoryFilter(event.target.value as TaskCategory | 'Toutes')}
            className="rounded-lg border border-gray-300 px-2 py-1 text-xs text-gray-900"
          >
            <option value="Toutes">Toutes</option>
            {CATEGORIES.map((category) => (
              <option key={category} value={category}>
                {category}
              </option>
            ))}
          </select>
        </label>
      </div>

      {isPending && <p className="text-sm text-gray-600">Chargement des tâches…</p>}
      {isError && <p className="text-sm text-red-600">Impossible de charger les tâches pour le moment.</p>}

      {tasks && tasks.length === 0 && <p className="text-sm text-gray-600">Aucune tâche pour le moment.</p>}

      {tasks && tasks.length > 0 && filteredTasks.length === 0 && (
        <p className="text-sm text-gray-600">Aucune tâche dans cette catégorie.</p>
      )}

      {filteredTasks.length > 0 && (
        <ul className="flex flex-col gap-3">
          {filteredTasks.map((task) => (
            <li key={task.id} className="flex items-center gap-3 rounded-lg border border-gray-200 p-3">
              <label className="flex h-11 w-11 shrink-0 cursor-pointer items-center justify-center">
                <input
                  type="checkbox"
                  checked={task.isDone}
                  disabled={!canToggle(task) || toggleTaskDone.isPending}
                  onChange={() => toggleTaskDone.mutate(task)}
                  aria-label={`Marquer "${task.title}" comme ${task.isDone ? 'non faite' : 'faite'}`}
                  className="h-6 w-6 rounded border-gray-300 text-green-600 focus:ring-green-600 disabled:cursor-not-allowed disabled:opacity-40"
                />
              </label>
              <div className="flex flex-1 flex-col gap-0.5">
                <span
                  className={task.isDone ? 'font-medium text-gray-400 line-through' : 'font-medium text-gray-900'}
                >
                  {task.title}
                </span>
                {task.quantity && <span className="text-xs text-gray-600">Quantité : {task.quantity}</span>}
                <div className="mt-1 flex gap-1.5">
                  <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-700">
                    {task.category}
                  </span>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
