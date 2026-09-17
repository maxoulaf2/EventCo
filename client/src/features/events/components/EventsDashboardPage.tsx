import { Link, Navigate } from 'react-router-dom'
import { ApiError } from '../../../shared/lib/api'
import { routes } from '../../../shared/lib/routes'
import { useMyEvents } from '../hooks/useMyEvents'

export function EventsDashboardPage() {
  const { data: events, isPending, isError, error } = useMyEvents()

  if (isError && error instanceof ApiError && error.status === 401) {
    return <Navigate to={routes.login} replace />
  }

  return (
    <main data-testid="events-dashboard-page" className="mx-auto flex min-h-screen max-w-lg flex-col gap-6 p-4">
      <div className="flex items-center justify-between gap-2">
        <h1 data-testid="events-dashboard-title" className="text-xl font-semibold md:text-2xl">Mes événements</h1>
        <Link
          to={routes.createEvent}
          data-testid="events-dashboard-new-event-link"
          className="shrink-0 rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white"
        >
          Nouvel événement
        </Link>
      </div>

      {isPending && (
        <p data-testid="events-dashboard-loading" className="text-sm text-gray-600">
          Chargement…
        </p>
      )}

      {isError && !(error instanceof ApiError && error.status === 401) && (
        <p data-testid="events-dashboard-error" className="text-sm text-red-600">
          Impossible de charger vos événements pour le moment.
        </p>
      )}

      {events && events.length === 0 && (
        <p data-testid="events-dashboard-empty-message" className="text-sm text-gray-600">
          Vous ne participez encore à aucun événement.
        </p>
      )}

      {events && events.length > 0 && (
        <ul data-testid="events-dashboard-list" className="flex flex-col gap-3">
          {events.map((event) => (
            <li key={event.id}>
              <Link
                to={routes.eventDetail(event.id)}
                data-testid={`event-list-item-${event.id}`}
                className="flex flex-col gap-1 rounded-lg border border-gray-200 p-4"
              >
                <div className="flex items-start justify-between gap-2">
                  <span className="font-medium text-gray-900">{event.title}</span>
                  {!event.hasJoined && (
                    <span
                      data-testid={`event-list-item-pending-badge-${event.id}`}
                      className="shrink-0 rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-800"
                    >
                      Invitation en attente
                    </span>
                  )}
                </div>
                <span className="text-sm text-gray-600">
                  {new Date(event.eventDate).toLocaleDateString('fr-FR', {
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                  })}
                  {event.location ? ` · ${event.location}` : ''}
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}
