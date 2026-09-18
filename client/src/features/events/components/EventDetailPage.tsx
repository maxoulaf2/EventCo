import { useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { useCurrentUser } from '../../auth/hooks/useCurrentUser'
import { TaskList } from '../../tasks/components/TaskList'
import { Avatar } from '../../../shared/components/Avatar'
import { Modal } from '../../../shared/components/Modal'
import { Tag } from '../../../shared/components/Tag'
import { ApiError } from '../../../shared/lib/api'
import { routes } from '../../../shared/lib/routes'
import { useDemoteParticipant } from '../hooks/useDemoteParticipant'
import { useEventDetail } from '../hooks/useEventDetail'
import { usePromoteParticipant } from '../hooks/usePromoteParticipant'
import { InviteParticipantForm } from './InviteParticipantForm'
import eventPhoto from '../../../assets/event-photo.jpg'
import type { EventParticipant } from '../types'

function formatDate(iso: string): string {
  const date = new Date(iso)
  const day = date.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
  const time = date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  return `${day.charAt(0).toUpperCase()}${day.slice(1)} · ${time}`
}

export function EventDetailPage() {
  const { eventId } = useParams<{ eventId: string }>()
  const { data: event, isPending, isError, error } = useEventDetail(eventId!)
  const { data: currentUser } = useCurrentUser()
  const promoteParticipant = usePromoteParticipant(eventId!)
  const demoteParticipant = useDemoteParticipant(eventId!)
  const [isParticipantsModalOpen, setIsParticipantsModalOpen] = useState(false)

  if (isError && error instanceof ApiError && error.status === 401) {
    return <Navigate to={routes.login} replace />
  }

  const isCreator = event !== undefined && currentUser !== undefined && event.createdByUserId === currentUser.userId
  const currentParticipant = event?.participants.find((p) => p.userId === currentUser?.userId)
  const isCreatorOrOrganizer = isCreator || currentParticipant?.role === 'Organizer'
  const canInvite = isCreatorOrOrganizer
  const isParticipant = isCreator || currentParticipant !== undefined

  const organizers = event?.participants.filter((p) => p.role === 'Organizer') ?? []
  const attendees = event?.participants.filter((p) => p.role === 'Participant') ?? []
  const confirmedCount = event?.participants.filter((p) => p.hasJoined).length ?? 0

  function renderParticipantAction(participant: EventParticipant) {
    if (!isCreator || participant.userId === event!.createdByUserId) {
      return null
    }

    if (participant.role === 'Participant') {
      return (
        <button
          type="button"
          onClick={() => promoteParticipant.mutate(participant.userId)}
          disabled={promoteParticipant.isPending}
          data-testid={`participant-promote-button-${participant.userId}`}
          className="inline-flex min-h-9.5 shrink-0 items-center rounded-full border border-border px-3.5 text-xs font-medium text-ink/75 transition-colors hover:bg-ink/5 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Promouvoir
        </button>
      )
    }

    return (
      <button
        type="button"
        onClick={() => demoteParticipant.mutate(participant.userId)}
        disabled={demoteParticipant.isPending}
        data-testid={`participant-demote-button-${participant.userId}`}
        className="inline-flex min-h-9.5 shrink-0 items-center rounded-full border border-border px-3.5 text-xs font-medium text-ink/75 transition-colors hover:bg-ink/5 disabled:cursor-not-allowed disabled:opacity-50"
      >
        Rétrograder
      </button>
    )
  }

  function ParticipantRow({ participant }: { participant: EventParticipant }) {
    return (
      <li
        data-testid={`participant-row-${participant.userId}`}
        className="flex min-h-14 items-center gap-3 rounded-full bg-surface px-3.5 py-2"
      >
        <Avatar name={participant.displayName} />
        <div className="flex-1">
          <div className="text-[15px] font-semibold text-ink">{participant.displayName}</div>
          <div className="text-[12.5px] text-ink/55">
            {participant.userId === event?.createdByUserId
              ? 'Créateur·ice'
              : participant.role === 'Organizer'
                ? 'Co-organisateur·ice'
                : participant.hasJoined
                  ? 'A rejoint'
                  : 'Invité·e'}
          </div>
        </div>
        <span data-testid={`participant-role-badge-${participant.userId}`} className="sr-only">
          {participant.role === 'Organizer' ? 'Co-organisateur' : 'Participant'}
        </span>
        {!participant.hasJoined && (
          <Tag variant="accent">
            <span data-testid={`participant-pending-badge-${participant.userId}`}>En attente</span>
          </Tag>
        )}
        {renderParticipantAction(participant)}
      </li>
    )
  }

  return (
    <main data-testid="event-detail-page" className="relative mx-auto flex min-h-screen w-full max-w-lg flex-col bg-bg lg:max-w-5xl">
      <Link
        to={routes.events}
        aria-label="Retour"
        data-testid="event-detail-back-link"
        className="absolute top-3.5 left-3.5 z-10 inline-flex h-11 w-11 items-center justify-center rounded-full bg-bg text-ink shadow-elev-sm transition-colors hover:bg-sand-100"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
          <path d="m12 19-7-7 7-7" />
          <path d="M19 12H5" />
        </svg>
      </Link>

      {isPending && (
        <p data-testid="event-detail-loading" className="p-6 text-sm text-ink/60">
          Chargement…
        </p>
      )}

      {isError && !(error instanceof ApiError && error.status === 401) && (
        <p data-testid="event-detail-error" className="p-6 text-sm text-accent-800">
          Impossible de charger cet événement pour le moment.
        </p>
      )}

      {event && (
        <>
          <div className="flex flex-col gap-6 px-5.5 pt-5.5 lg:grid lg:grid-cols-2 lg:gap-10 lg:px-8.5 lg:pb-9">
            <div className="flex flex-col overflow-hidden">
              <div className="h-52 flex-none overflow-hidden rounded-3xl lg:h-64">
                <img src={eventPhoto} alt="" className="h-full w-full object-cover opacity-95 saturate-[0.6] contrast-[0.85] brightness-110" />
              </div>

              <div className="pt-5.5">
                {(isCreator || currentParticipant?.role === 'Organizer') && (
                  <Tag variant="sage" className="mb-2.5">
                    {isCreator ? 'Vous organisez' : 'Co-organisateur·ice'}
                  </Tag>
                )}
                <h1 data-testid="event-detail-title" className="mb-4 text-[28px] lg:text-[40px]">
                  {event.title}
                </h1>

                <div data-testid="event-detail-date-location" className="flex flex-col gap-1.5">
                  <div className="flex items-center gap-2.5 text-[15px]">
                    <span className="grid h-8.5 w-8.5 shrink-0 place-items-center rounded-full bg-surface text-accent-700">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="3" y="4" width="18" height="18" rx="2" />
                        <path d="M16 2v4" />
                        <path d="M8 2v4" />
                        <path d="M3 10h18" />
                      </svg>
                    </span>
                    {formatDate(event.eventDate)}
                  </div>
                  {event.location && (
                    <div className="flex items-center gap-2.5 text-[15px]">
                      <span className="grid h-8.5 w-8.5 shrink-0 place-items-center rounded-full bg-surface text-accent-700">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
                          <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0" />
                          <circle cx="12" cy="10" r="3" />
                        </svg>
                      </span>
                      {event.location}
                    </div>
                  )}
                </div>

                {event.description && (
                  <p data-testid="event-detail-description" className="mt-4 max-w-md text-[15px] text-ink/75">
                    {event.description}
                  </p>
                )}

                <button
                  type="button"
                  onClick={() => setIsParticipantsModalOpen(true)}
                  data-testid="event-detail-participants-button"
                  className="mt-5 flex items-center gap-3.5 rounded-full bg-surface px-4 py-3"
                >
                  <span className="flex">
                    {event.participants.slice(0, 4).map((p) => (
                      <span key={p.userId} className="-ml-2 first:ml-0">
                        <Avatar name={p.displayName} ringed />
                      </span>
                    ))}
                    {event.participants.length > 4 && (
                      <span className="-ml-2 grid h-[38px] w-[38px] place-items-center rounded-full bg-sand-300 text-[11px] font-semibold text-sand-800 shadow-[0_0_0_2.5px_var(--color-surface)]">
                        +{event.participants.length - 4}
                      </span>
                    )}
                  </span>
                  <span className="text-[13.5px] text-ink/65">
                    {confirmedCount} sur {event.participants.length} ont confirmé
                  </span>
                  <span className="ml-auto text-ink/45">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
                      <path d="m9 18 6-6-6-6" />
                    </svg>
                  </span>
                </button>
              </div>
            </div>

            <div className="flex flex-col lg:overflow-hidden">
              <TaskList
                eventId={eventId!}
                currentUserId={currentUser?.userId}
                canManageAllTasks={isCreatorOrOrganizer}
                canSelfAssign={isParticipant}
                canAddTask={isParticipant}
                participants={event.participants}
              />
            </div>
          </div>

          <Modal
            open={isParticipantsModalOpen}
            onClose={() => setIsParticipantsModalOpen(false)}
            title="Participants"
            testId="participants-modal"
          >
            <div className="flex flex-col gap-6">
              {organizers.length > 0 && (
                <div className="flex flex-col gap-2.5">
                  <p data-testid="event-detail-participants-heading" className="ml-4 text-[10px] tracking-[0.11em] text-accent-700 uppercase">
                    Organisation
                  </p>
                  <ul data-testid="event-detail-participants-list" className="flex flex-col gap-2.5">
                    {organizers.map((participant) => (
                      <ParticipantRow key={participant.userId} participant={participant} />
                    ))}
                  </ul>
                </div>
              )}

              {attendees.length > 0 && (
                <div className="flex flex-col gap-2.5">
                  <p className="ml-4 text-[10px] tracking-[0.11em] text-accent-700 uppercase">Participants</p>
                  <ul className="flex flex-col gap-2.5">
                    {attendees.map((participant) => (
                      <ParticipantRow key={participant.userId} participant={participant} />
                    ))}
                  </ul>
                </div>
              )}

              {canInvite && <InviteParticipantForm eventId={eventId!} />}
            </div>
          </Modal>
        </>
      )}
    </main>
  )
}
