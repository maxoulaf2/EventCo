import type { EventParticipant } from '../../events/types'
import { Avatar } from '../../../shared/components/Avatar'
import { AddItemForm } from './AddItemForm'
import { useAssignItem } from '../hooks/useAssignItem'
import { useEventItems } from '../hooks/useEventItems'
import { useItemRealtime } from '../hooks/useItemRealtime'
import { useUnassignItem } from '../hooks/useUnassignItem'
import type { EventItem } from '../types'

type ItemSection = 'todo' | 'assigned'

const SECTIONS: { key: ItemSection; label: string; emptyMessage: string }[] = [
  { key: 'todo', label: 'À prendre', emptyMessage: 'Aucun article à prendre.' },
  { key: 'assigned', label: 'Assignés', emptyMessage: 'Aucun article assigné.' },
]

function sectionOf(item: EventItem): ItemSection {
  return item.assignedToUserId ? 'assigned' : 'todo'
}

interface ItemListProps {
  eventId: string
  currentUserId: string | undefined
  canManageAllItems: boolean
  canSelfAssign: boolean
  canAddItem: boolean
  participants: EventParticipant[]
}

export function ItemList({
  eventId,
  currentUserId,
  canManageAllItems,
  canSelfAssign,
  canAddItem,
  participants,
}: ItemListProps) {
  const { data: items, isPending, isError } = useEventItems(eventId)
  useItemRealtime(eventId)
  const assignItem = useAssignItem(eventId)
  const unassignItem = useUnassignItem(eventId)

  function canUnassign(item: EventItem) {
    return canManageAllItems || item.assignedToUserId === currentUserId
  }

  function participantName(userId: string | null) {
    return participants.find((p) => p.userId === userId)?.displayName ?? '?'
  }

  return (
    <div data-testid="item-list" id="items" className="flex flex-1 flex-col overflow-hidden">
      <div className="mb-4 flex items-baseline gap-2.5">
        <h2 data-testid="item-list-heading" className="m-0 text-xl sm:text-[26px]">
          Qui apporte quoi
        </h2>
      </div>

      {isPending && (
        <p data-testid="item-list-loading" className="text-sm text-ink/60">
          Chargement des articles…
        </p>
      )}
      {isError && (
        <p data-testid="item-list-error" className="text-sm text-accent-800">
          Impossible de charger les articles pour le moment.
        </p>
      )}

      {items && items.length === 0 && (
        <p data-testid="item-list-empty-message" className="text-sm text-ink/60">
          Aucun article pour le moment.
        </p>
      )}

      {items && items.length > 0 && (
        <div className="flex flex-1 flex-col gap-6 overflow-auto">
          {SECTIONS.map(({ key, label, emptyMessage }) => {
            const sectionItems = items.filter((item) => sectionOf(item) === key)
            return (
              <section key={key} data-testid={`item-list-section-${key}`}>
                <h3
                  data-testid={`item-list-section-heading-${key}`}
                  className="m-0 mb-2.5 pl-3.5 font-heading text-[13.5px] text-ink/65"
                >
                  {label} · {sectionItems.length}
                </h3>
                {sectionItems.length === 0 ? (
                  <p data-testid={`item-list-section-empty-message-${key}`} className="pl-3.5 text-sm text-ink/60">
                    {emptyMessage}
                  </p>
                ) : (
                  <ul className="flex flex-col gap-2.5">
                    {sectionItems.map((item) => {
                      const showAssign = canSelfAssign && !item.assignedToUserId
                      return (
                        <li
                          key={item.id}
                          data-testid={`item-row-${item.id}`}
                          className={`flex min-h-14 items-center gap-3 rounded-[1.75rem] bg-surface px-3.5 ${
                            key === 'todo' ? 'shadow-[inset_0_0_0_1.5px_var(--color-accent-600)]' : ''
                          }`}
                        >
                          <span data-testid={`item-row-title-${item.id}`} className="flex-1 pl-3.5 text-[15px] text-ink">
                            {item.title}
                            {item.quantity && <span className="text-ink/50"> · {item.quantity}</span>}
                          </span>
                          {showAssign && (
                            <button
                              type="button"
                              onClick={() => currentUserId && assignItem.mutate({ itemId: item.id, userId: currentUserId })}
                              disabled={assignItem.isPending}
                              data-testid={`item-row-assign-button-${item.id}`}
                              className="inline-flex min-h-9.5 shrink-0 items-center rounded-full bg-accent-500 px-3.5 text-[13px] font-heading text-accent-900 transition-colors hover:bg-accent-400 disabled:cursor-not-allowed disabled:opacity-45"
                            >
                              Je prends
                            </button>
                          )}
                          {!showAssign && item.assignedToUserId && (
                            <div className="flex shrink-0 items-center gap-2">
                              <Avatar name={participantName(item.assignedToUserId)} size="sm" />
                              {canUnassign(item) && (
                                <button
                                  type="button"
                                  onClick={() => unassignItem.mutate(item.id)}
                                  disabled={unassignItem.isPending}
                                  aria-label={`Se désassigner de "${item.title}"`}
                                  data-testid={`item-row-unassign-button-${item.id}`}
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

      {canAddItem && <AddItemForm eventId={eventId} />}
    </div>
  )
}
