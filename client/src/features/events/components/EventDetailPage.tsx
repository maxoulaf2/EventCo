import { Link, Navigate, useParams } from 'react-router-dom'
import { useCurrentUser } from '../../auth/hooks/useCurrentUser'
import { ApiError } from '../../../shared/lib/api'
import { routes } from '../../../shared/lib/routes'
import { useDemoteParticipant } from '../hooks/useDemoteParticipant'
import { useEventDetail } from '../hooks/useEventDetail'
import { usePromoteParticipant } from '../hooks/usePromoteParticipant'
import { InviteParticipantForm } from './InviteParticipantForm'
import type { EventParticipant } from '../types'

export function EventDetailPage() {
  const { eventId } = useParams<{ eventId: string }>()
  const { data: event, isPending, isError, error } = useEventDetail(eventId!)
  const { data: currentUser } = useCurrentUser()
  const promoteParticipant = usePromoteParticipant(eventId!)
  const demoteParticipant = useDemoteParticipant(eventId!)

  if (isError && error instanceof ApiError && error.status === 401) {
    return <Navigate to={routes.login} replace />
  }

  const isCreator = event !== undefined && currentUser !== undefined && event.createdByUserId === currentUser.userId
  const currentParticipant = event?.participants.find((p) => p.userId === currentUser?.userId)
  const canInvite = isCreator || currentParticipant?.role === 'Organizer'

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
          className="shrink-0 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 disabled:opacity-50"
        >
          Promouvoir co-organisateur
        </button>
      )
    }

    return (
      <button
        type="button"
        onClick={() => demoteParticipant.mutate(participant.userId)}
        disabled={demoteParticipant.isPending}
        className="shrink-0 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 disabled:opacity-50"
      >
        Rétrograder participant
      </button>
    )
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-lg flex-col gap-6 p-4">
      <Link to={routes.events} className="text-sm text-gray-600">
        ← Mes événements
      </Link>

      {isPending && <p className="text-sm text-gray-600">Chargement…</p>}

      {isError && !(error instanceof ApiError && error.status === 401) && (
        <p className="text-sm text-red-600">Impossible de charger cet événement pour le moment.</p>
      )}

      {event && (
        <>
          <div className="flex flex-col gap-1">
            <h1 className="text-xl font-semibold md:text-2xl">{event.title}</h1>
            <span className="text-sm text-gray-600">
              {new Date(event.eventDate).toLocaleDateString('fr-FR', {
                day: 'numeric',
                month: 'long',
                year: 'numeric',
              })}
              {event.location ? ` · ${event.location}` : ''}
            </span>
            {event.description && <p className="mt-2 text-sm text-gray-700">{event.description}</p>}
          </div>

          <div className="flex flex-col gap-3">
            <h2 className="text-sm font-semibold text-gray-900">
              Participants ({event.participants.length})
            </h2>
            <ul className="flex flex-col gap-3">
              {event.participants.map((participant) => (
                <li
                  key={participant.userId}
                  className="flex items-center justify-between gap-2 rounded-lg border border-gray-200 p-3"
                >
                  <div className="flex flex-col gap-0.5">
                    <span className="font-medium text-gray-900">{participant.displayName}</span>
                    <span className="text-xs text-gray-600">{participant.email}</span>
                    <div className="mt-1 flex gap-1.5">
                      <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-700">
                        {participant.role === 'Organizer' ? 'Co-organisateur' : 'Participant'}
                      </span>
                      {!participant.hasJoined && (
                        <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-800">
                          Invitation en attente
                        </span>
                      )}
                    </div>
                  </div>
                  {renderParticipantAction(participant)}
                </li>
              ))}
            </ul>
          </div>

          {canInvite && <InviteParticipantForm eventId={eventId!} />}
        </>
      )}
    </main>
  )
}
