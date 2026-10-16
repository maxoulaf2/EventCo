import type { EventParticipant } from '../../events/types'
import { Avatar } from '../../../shared/components/Avatar'
import { AddItemForm } from './AddItemForm'
import { useAssignItem } from '../hooks/useAssignItem'
import { useCorrectItemCategory } from '../hooks/useCorrectItemCategory'
import { useDeleteItem } from '../hooks/useDeleteItem'
import { useEventItems } from '../hooks/useEventItems'
import { useItemCategories } from '../hooks/useItemCategories'
import { useItemRealtime } from '../hooks/useItemRealtime'
import { useUnassignItem } from '../hooks/useUnassignItem'
import type { EventItem } from '../types'

// Les articles arrivent déjà triés par le backend (catégorie, à prendre d'abord, ordre alphabétique) :
// on les regroupe par catégorie sans les retrier, les groupes suivant l'ordre de leur premier article.
function groupByCategory(items: EventItem[]) {
  const groups = new Map<string, EventItem[]>()
  for (const item of items) {
    groups.set(item.category, [...(groups.get(item.category) ?? []), item])
  }
  return [...groups]
}

interface ItemListProps {
  eventId: string
  currentUserId: string | undefined
  canManageAllItems: boolean
  canSelfAssign: boolean
  canAddItem: boolean
  canAddItemToBring: boolean
  participants: EventParticipant[]
}

export function ItemList({
  eventId,
  currentUserId,
  canManageAllItems,
  canSelfAssign,
  canAddItem,
  canAddItemToBring,
  participants,
}: ItemListProps) {
  const { data: items, isPending, isError } = useEventItems(eventId)
  useItemRealtime(eventId)
  const assignItem = useAssignItem(eventId)
  const unassignItem = useUnassignItem(eventId)
  const deleteItem = useDeleteItem(eventId)
  const correctItemCategory = useCorrectItemCategory(eventId)
  const { data: categories } = useItemCategories()

  // Un article apporté reste attribué à la personne qui l'apporte : elle peut seulement l'annuler.
  function canUnassign(item: EventItem) {
    return item.kind === 'ToBring' && (canManageAllItems || item.assignedToUserId === currentUserId)
  }

  function canCancel(item: EventItem) {
    return item.kind === 'Contribution' && item.assignedToUserId === currentUserId
  }

  // Le créateur de l'article ou un (co-)organisateur. Un article apporté reste attribué à son créateur pour
  // toujours, et seul un (co-)organisateur crée un article à prendre : pas besoin d'exposer le créateur.
  function canCorrectCategory(item: EventItem) {
    return canManageAllItems || (item.kind === 'Contribution' && item.assignedToUserId === currentUserId)
  }

  function participantOf(userId: string | null) {
    return participants.find((p) => p.userId === userId)
  }

  const toTakeCount = items?.filter((item) => !item.assignedToUserId).length ?? 0

  return (
    <div data-testid="item-list" id="items" className="flex flex-1 flex-col overflow-hidden">
      <div className="mb-4 flex items-baseline gap-2.5">
        <h2 data-testid="item-list-heading" className="m-0 text-xl sm:text-[26px]">
          Qui apporte quoi
        </h2>
        {items && items.length > 0 && (
          <span data-testid="item-list-to-take-count" className="font-heading text-[13.5px] text-ink/65">
            {toTakeCount} à prendre
          </span>
        )}
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
          {groupByCategory(items).map(([key, categoryItems]) => {
            const label = categories?.find((category) => category.key === key)?.label ?? key
            return (
              <section key={key} data-testid={`item-list-category-${key}`}>
                <h3
                  data-testid={`item-list-category-heading-${key}`}
                  className="m-0 mb-2.5 pl-3.5 font-heading text-[13.5px] text-ink/65"
                >
                  {label} · {categoryItems.length}
                </h3>
                <ul className="flex flex-col gap-2.5">
                  {categoryItems.map((item) => {
                    const showAssign = canSelfAssign && !item.assignedToUserId
                    return (
                      <li
                        key={item.id}
                        data-testid={`item-row-${item.id}`}
                        className={`flex min-h-14 items-center gap-3 rounded-[1.75rem] bg-surface px-3.5 ${
                          !item.assignedToUserId ? 'shadow-[inset_0_0_0_1.5px_var(--color-accent-600)]' : ''
                        }`}
                      >
                        <span data-testid={`item-row-title-${item.id}`} className="flex-1 pl-3.5 text-[15px] text-ink">
                          {item.title}
                          {item.quantity && <span className="text-ink/50"> · {item.quantity}</span>}
                        </span>
                        {categories && canCorrectCategory(item) && (
                          <label className="relative inline-flex size-9.5 shrink-0 items-center justify-center rounded-full text-ink/45 transition-colors hover:text-ink">
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
                              <path d="M12.586 2.586A2 2 0 0 0 11.172 2H4a2 2 0 0 0-2 2v7.172a2 2 0 0 0 .586 1.414l8.704 8.704a2.426 2.426 0 0 0 3.42 0l6.58-6.58a2.426 2.426 0 0 0 0-3.42z" />
                              <circle cx="7.5" cy="7.5" r=".5" fill="currentColor" />
                            </svg>
                            {/* Sélecteur natif invisible par-dessus l'icône : sur mobile, le navigateur ouvre son propre sélecteur. */}
                            <select
                              value={item.category}
                              onChange={(event) =>
                                correctItemCategory.mutate({ itemId: item.id, category: event.target.value })
                              }
                              aria-label={`Changer la catégorie de "${item.title}"`}
                              data-testid={`item-row-category-select-${item.id}`}
                              className="absolute inset-0 cursor-pointer opacity-0"
                            >
                              {categories.map((category) => (
                                <option key={category.key} value={category.key}>
                                  {category.label}
                                </option>
                              ))}
                            </select>
                          </label>
                        )}
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
                          <div data-testid={`item-row-assignee-${item.id}`} className="flex shrink-0 items-center gap-2">
                            <Avatar
                              name={participantOf(item.assignedToUserId)?.displayName ?? '?'}
                              imageUrl={participantOf(item.assignedToUserId)?.avatarUrl}
                              size="sm"
                            />
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
                            {canCancel(item) && (
                              <button
                                type="button"
                                onClick={() => deleteItem.mutate(item.id)}
                                disabled={deleteItem.isPending}
                                aria-label={`Annuler "${item.title}"`}
                                data-testid={`item-row-cancel-button-${item.id}`}
                                className="inline-flex min-h-9.5 shrink-0 items-center rounded-full border border-ink/15 bg-surface px-3.5 text-[13px] font-heading text-ink/70 transition-colors hover:text-ink disabled:cursor-not-allowed disabled:opacity-45"
                              >
                                Annuler
                              </button>
                            )}
                          </div>
                        )}
                      </li>
                    )
                  })}
                </ul>
              </section>
            )
          })}
        </div>
      )}

      {canAddItem && <AddItemForm eventId={eventId} canAddItemToBring={canAddItemToBring} />}
    </div>
  )
}
