import { useState } from 'react'
import type { EventParticipant } from '../../events/types'
import { Avatar } from '../../../shared/components/Avatar'
import { AddTaskForm } from './AddTaskForm'
import { useAssignTask } from '../hooks/useAssignTask'
import { useEventTasks } from '../hooks/useEventTasks'
import { useTaskRealtime } from '../hooks/useTaskRealtime'
import { useToggleTaskDone } from '../hooks/useToggleTaskDone'
import type { EventTask, TaskCategory } from '../types'

const CATEGORIES: TaskCategory[] = ['Courses', 'Logistique', 'Autre']

type TaskTab = 'todo' | 'assigned' | 'done'

const TABS: { key: TaskTab; label: string }[] = [
  { key: 'todo', label: 'À prendre' },
  { key: 'assigned', label: 'Assignées' },
  { key: 'done', label: 'Faites' },
]

function tabOf(task: EventTask): TaskTab {
  if (task.isDone) return 'done'
  return task.assignedToUserId ? 'assigned' : 'todo'
}

interface TaskListProps {
  eventId: string
  currentUserId: string | undefined
  canManageAllTasks: boolean
  canSelfAssign: boolean
  canAddTask: boolean
  participants: EventParticipant[]
}

export function TaskList({
  eventId,
  currentUserId,
  canManageAllTasks,
  canSelfAssign,
  canAddTask,
  participants,
}: TaskListProps) {
  const { data: tasks, isPending, isError } = useEventTasks(eventId)
  useTaskRealtime(eventId)
  const toggleTaskDone = useToggleTaskDone(eventId)
  const assignTask = useAssignTask(eventId)
  const [tab, setTab] = useState<TaskTab>('todo')

  const counts: Record<TaskTab, number> = { todo: 0, assigned: 0, done: 0 }
  tasks?.forEach((task) => counts[tabOf(task)]++)

  const tabTasks = tasks?.filter((task) => tabOf(task) === tab) ?? []
  const groups = CATEGORIES.map((category) => ({
    category,
    items: tabTasks.filter((task) => task.category === category),
  })).filter((group) => group.items.length > 0)

  function canToggle(task: EventTask) {
    return canManageAllTasks || task.assignedToUserId === currentUserId
  }

  function participantName(userId: string | null) {
    return participants.find((p) => p.userId === userId)?.displayName ?? '?'
  }

  return (
    <div data-testid="task-list" id="tasks" className="flex flex-1 flex-col overflow-hidden">
      <div className="mb-4 flex items-baseline gap-2.5">
        <h2 data-testid="task-list-heading" className="m-0 text-xl sm:text-[26px]">
          Qui apporte quoi
        </h2>
        <span className="ml-auto text-[13px] text-accent-700 sm:text-sm">
          {tasks?.filter((t) => t.isDone).length ?? 0} / {tasks?.length ?? 0}
        </span>
      </div>

      {isPending && (
        <p data-testid="task-list-loading" className="text-sm text-ink/60">
          Chargement des tâches…
        </p>
      )}
      {isError && (
        <p data-testid="task-list-error" className="text-sm text-accent-800">
          Impossible de charger les tâches pour le moment.
        </p>
      )}

      {tasks && tasks.length === 0 && (
        <p data-testid="task-list-empty-message" className="text-sm text-ink/60">
          Aucune tâche pour le moment.
        </p>
      )}

      {tasks && tasks.length > 0 && (
        <>
          <div className="mb-4.5 flex flex-none gap-0 rounded-full bg-surface p-1.5" role="tablist">
            {TABS.map(({ key, label }) => (
              <button
                key={key}
                type="button"
                role="tab"
                aria-selected={tab === key}
                onClick={() => setTab(key)}
                data-testid={`task-list-tab-${key}`}
                className={`min-h-11 flex-1 rounded-full text-[13.5px] transition-colors ${
                  tab === key ? 'bg-accent-500 font-heading text-accent-900' : 'text-ink/65 hover:text-ink'
                }`}
              >
                {label} · {counts[key]}
              </button>
            ))}
          </div>

          <div className="flex-1 overflow-auto">
            {tabTasks.length === 0 ? (
              <p data-testid="task-list-empty-tab-message" className="text-sm text-ink/60">
                Aucune tâche dans cet onglet.
              </p>
            ) : (
              groups.map((group) => (
                <div key={group.category} className="mb-6.5">
                  <p className="mb-2.5 ml-4 text-[10px] tracking-[0.11em] text-accent-700 uppercase">{group.category}</p>
                  <ul className="flex flex-col gap-2.5">
                    {group.items.map((task) => {
                      const showAssign = canSelfAssign && !task.assignedToUserId && !task.isDone
                      return (
                        <li
                          key={task.id}
                          data-testid={`task-item-${task.id}`}
                          className={`flex min-h-14 items-center gap-3 rounded-full bg-surface px-3.5 ${
                            !task.isDone && !task.assignedToUserId && tab === 'todo'
                              ? 'shadow-[inset_0_0_0_1.5px_var(--color-accent-600)]'
                              : ''
                          }`}
                        >
                          <span data-testid={`task-item-category-badge-${task.id}`} className="sr-only">
                            {task.category}
                          </span>
                          <label className="grid h-11 w-11 shrink-0 cursor-pointer place-items-center">
                            <input
                              type="checkbox"
                              checked={task.isDone}
                              disabled={!canToggle(task) || toggleTaskDone.isPending}
                              onChange={() => toggleTaskDone.mutate(task)}
                              aria-label={`Marquer "${task.title}" comme ${task.isDone ? 'non faite' : 'faite'}`}
                              data-testid={`task-item-checkbox-${task.id}`}
                              className="sr-only"
                            />
                            <span
                              aria-hidden="true"
                              className={`grid h-[30px] w-[30px] place-items-center rounded-full border-2 ${
                                task.isDone ? 'border-sage-600 bg-sage-600 text-bg' : 'border-ink/22'
                              } ${!canToggle(task) ? 'opacity-40' : ''}`}
                            >
                              {task.isDone && (
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
                                  <path d="M20 6 9 17l-5-5" />
                                </svg>
                              )}
                            </span>
                          </label>
                          <span
                            data-testid={`task-item-title-${task.id}`}
                            className={`flex-1 text-[15px] ${task.isDone ? 'text-ink/55 line-through' : 'text-ink'}`}
                          >
                            {task.title}
                            {task.quantity && <span className="text-ink/50"> · {task.quantity}</span>}
                          </span>
                          {showAssign && (
                            <button
                              type="button"
                              onClick={() => currentUserId && assignTask.mutate({ taskId: task.id, userId: currentUserId })}
                              disabled={assignTask.isPending}
                              data-testid={`task-item-assign-button-${task.id}`}
                              className="inline-flex min-h-9.5 shrink-0 items-center rounded-full bg-accent-500 px-3.5 text-[13px] font-heading text-accent-900 transition-colors hover:bg-accent-400 disabled:cursor-not-allowed disabled:opacity-45"
                            >
                              Je prends
                            </button>
                          )}
                          {!showAssign && task.assignedToUserId && (
                            <Avatar name={participantName(task.assignedToUserId)} size="sm" />
                          )}
                        </li>
                      )
                    })}
                  </ul>
                </div>
              ))
            )}
          </div>
        </>
      )}

      {canAddTask && <AddTaskForm eventId={eventId} />}
    </div>
  )
}
