import type { EventParticipant } from '../../events/types'
import { Avatar } from '../../../shared/components/Avatar'
import { AddTaskForm } from './AddTaskForm'
import { useAssignTask } from '../hooks/useAssignTask'
import { useEventTasks } from '../hooks/useEventTasks'
import { useTaskRealtime } from '../hooks/useTaskRealtime'
import { useUnassignTask } from '../hooks/useUnassignTask'
import type { EventTask } from '../types'

type TaskSection = 'todo' | 'assigned'

const SECTIONS: { key: TaskSection; label: string; emptyMessage: string }[] = [
  { key: 'todo', label: 'À prendre', emptyMessage: 'Aucune tâche à prendre.' },
  { key: 'assigned', label: 'Assignées', emptyMessage: 'Aucune tâche assignée.' },
]

function sectionOf(task: EventTask): TaskSection {
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
  const assignTask = useAssignTask(eventId)
  const unassignTask = useUnassignTask(eventId)

  function canUnassign(task: EventTask) {
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
        <div className="flex flex-1 flex-col gap-6 overflow-auto">
          {SECTIONS.map(({ key, label, emptyMessage }) => {
            const sectionTasks = tasks.filter((task) => sectionOf(task) === key)
            return (
              <section key={key} data-testid={`task-list-section-${key}`}>
                <h3
                  data-testid={`task-list-section-heading-${key}`}
                  className="m-0 mb-2.5 pl-3.5 font-heading text-[13.5px] text-ink/65"
                >
                  {label} · {sectionTasks.length}
                </h3>
                {sectionTasks.length === 0 ? (
                  <p data-testid={`task-list-section-empty-message-${key}`} className="pl-3.5 text-sm text-ink/60">
                    {emptyMessage}
                  </p>
                ) : (
                  <ul className="flex flex-col gap-2.5">
                    {sectionTasks.map((task) => {
                      const showAssign = canSelfAssign && !task.assignedToUserId
                      return (
                        <li
                          key={task.id}
                          data-testid={`task-item-${task.id}`}
                          className={`flex min-h-14 items-center gap-3 rounded-[1.75rem] bg-surface px-3.5 ${
                            key === 'todo' ? 'shadow-[inset_0_0_0_1.5px_var(--color-accent-600)]' : ''
                          }`}
                        >
                          <span data-testid={`task-item-title-${task.id}`} className="flex-1 pl-3.5 text-[15px] text-ink">
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
                            <div className="flex shrink-0 items-center gap-2">
                              <Avatar name={participantName(task.assignedToUserId)} size="sm" />
                              {canUnassign(task) && (
                                <button
                                  type="button"
                                  onClick={() => unassignTask.mutate(task.id)}
                                  disabled={unassignTask.isPending}
                                  aria-label={`Se désassigner de "${task.title}"`}
                                  data-testid={`task-item-unassign-button-${task.id}`}
                                  className="inline-flex min-h-9.5 shrink-0 items-center rounded-full border border-ink/15 bg-surface px-3.5 text-[13px] font-heading text-ink/70 transition-colors hover:text-ink disabled:cursor-not-allowed disabled:opacity-45"
                                >
                                  Laisser
                                </button>
                              )}
                            </div>
                          )}
                        </li>
                      )
                    })}
                  </ul>
                )}
              </section>
            )
          })}
        </div>
      )}

      {canAddTask && <AddTaskForm eventId={eventId} />}
    </div>
  )
}
