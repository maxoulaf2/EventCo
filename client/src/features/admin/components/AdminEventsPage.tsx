import { Link, Navigate } from 'react-router-dom'
import { ApiError } from '../../../shared/lib/api'
import { routes } from '../../../shared/lib/routes'
import { useAllEvents } from '../hooks/useAllEvents'
import type { AdminEventSummary } from '../types'

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR', { day: 'numeric', month: 'long', year: 'numeric' })
}

export function AdminEventsPage() {
  const { data: events, isPending, isError, error } = useAllEvents()

  if (isError && error instanceof ApiError && error.status === 401) {
    return <Navigate to={routes.login} replace />
  }

  const isForbidden = isError && error instanceof ApiError && error.status === 403

  return (
    <main data-testid="admin-events-page" className="mx-auto flex min-h-screen w-full max-w-lg flex-col bg-bg">
      <div className="flex items-center gap-3 px-5.5 pt-5.5">
        <Link
          to={routes.events}
          aria-label="Retour"
          data-testid="admin-events-back-link"
          className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-bg text-ink shadow-elev-sm transition-colors hover:bg-sand-100"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
            <path d="m12 19-7-7 7-7" />
            <path d="M19 12H5" />
          </svg>
        </Link>
        <div>
          <p className="m-0 text-[12.5px] text-ink/60">Administration</p>
          <h1 data-testid="admin-events-title" className="m-0 text-[26px] sm:text-[30px]">
            Tous les événements
          </h1>
        </div>
      </div>

      <div className="flex flex-1 flex-col gap-3.5 px-5.5 pt-6 pb-5.5">
        {isPending && (
          <p data-testid="admin-events-loading" className="text-sm text-ink/60">
            Chargement…
          </p>
        )}

        {isForbidden && (
          <p data-testid="admin-events-forbidden" className="text-sm text-accent-800">
            Cette page est réservée aux administrateurs.
          </p>
        )}

        {isError && !isForbidden && !(error instanceof ApiError && error.status === 401) && (
          <p data-testid="admin-events-error" className="text-sm text-accent-800">
            Impossible de charger les événements pour le moment.
          </p>
        )}

        {events && events.length === 0 && (
          <p data-testid="admin-events-empty-message" className="text-sm text-ink/60">
            Aucun événement pour le moment.
          </p>
        )}

        {events && events.length > 0 && (
          <ul data-testid="admin-events-list" className="flex flex-col gap-3.5">
            {events.map((event) => (
              <AdminEventCard key={event.id} event={event} />
            ))}
          </ul>
        )}
      </div>
    </main>
  )
}

function AdminEventCard({ event }: { event: AdminEventSummary }) {
  return (
    <li>
      <Link
        to={routes.eventDetail(event.id)}
        data-testid={`admin-event-list-item-${event.id}`}
        className="flex flex-col gap-1 rounded-[calc(var(--radius-2xl)*1.15)] bg-surface p-4.5 shadow-elev-sm"
      >
        <div className="font-heading text-[19px] sm:text-[21px]">{event.title}</div>
        <div className="text-[13px] text-ink/62">
          {formatDate(event.eventDate)}
          {event.location && ` · ${event.location}`}
        </div>
        <div data-testid={`admin-event-list-item-participant-count-${event.id}`} className="text-[12.5px] text-ink/55">
          {event.participantCount} participant{event.participantCount > 1 ? 's' : ''}
        </div>
      </Link>
    </li>
  )
}
