import { Link, Navigate } from 'react-router-dom'
import { useCurrentUser } from '../../auth/hooks/useCurrentUser'
import { ApiError } from '../../../shared/lib/api'
import { btnPrimary } from '../../../shared/lib/ui'
import { routes } from '../../../shared/lib/routes'
import { useMyEvents } from '../hooks/useMyEvents'
import { Tag } from '../../../shared/components/Tag'
import type { MyEvent } from '../types'

function dateBadge(iso: string): { day: string; month: string } {
  const date = new Date(iso)
  return {
    day: date.toLocaleDateString('fr-FR', { day: 'numeric' }),
    month: date.toLocaleDateString('fr-FR', { month: 'short' }).replace('.', ''),
  }
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR', { day: 'numeric', month: 'long', year: 'numeric' })
}

export function EventsDashboardPage() {
  const { data: events, isPending, isError, error } = useMyEvents()
  const { data: currentUser } = useCurrentUser()

  if (isError && error instanceof ApiError && error.status === 401) {
    return <Navigate to={routes.login} replace />
  }

  const now = Date.now()
  const upcoming = events?.filter((event) => new Date(event.eventDate).getTime() >= now) ?? []
  const past = events?.filter((event) => new Date(event.eventDate).getTime() < now) ?? []

  return (
    <main data-testid="events-dashboard-page" className="mx-auto flex min-h-screen w-full max-w-lg flex-col bg-bg">
      <div className="flex items-end gap-3 px-5.5 pt-5.5">
        <div className="mr-auto">
          {currentUser && <p className="m-0 text-[12.5px] text-ink/60">Bonjour {currentUser.displayName}</p>}
          <h1 data-testid="events-dashboard-title" className="m-0 text-[26px] sm:text-[30px]">
            Mes événements
          </h1>
        </div>
        {currentUser && <span className="inline-grid h-11 w-11 shrink-0 place-items-center rounded-full bg-sage-600 text-[15px] font-semibold text-bg">{currentUser.displayName.charAt(0).toUpperCase()}</span>}
      </div>

      <div className="flex flex-1 flex-col gap-3.5 px-5.5 pt-6">
        {isPending && (
          <p data-testid="events-dashboard-loading" className="text-sm text-ink/60">
            Chargement…
          </p>
        )}

        {isError && !(error instanceof ApiError && error.status === 401) && (
          <p data-testid="events-dashboard-error" className="text-sm text-accent-800">
            Impossible de charger vos événements pour le moment.
          </p>
        )}

        {events && events.length === 0 && <EmptyState />}

        {events && events.length > 0 && (
          <ul data-testid="events-dashboard-list" className="flex flex-col gap-3.5">
            {upcoming.map((event) => (
              <EventCard key={event.id} event={event} />
            ))}
          </ul>
        )}

        {past.length > 0 && (
          <>
            <p className="mt-2 px-1 text-[12.5px] text-ink/50">Événements passés · {past.length}</p>
            <ul className="flex flex-col gap-3.5 opacity-80">
              {past.map((event) => (
                <EventCard key={event.id} event={event} />
              ))}
            </ul>
          </>
        )}
      </div>

      {events && events.length > 0 && (
        <div className="p-5.5">
          <Link to={routes.createEvent} data-testid="events-dashboard-new-event-link" className={`${btnPrimary} min-h-14 w-full text-base shadow-elev-md`}>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round">
              <path d="M5 12h14" />
              <path d="M12 5v14" />
            </svg>
            Nouvel événement
          </Link>
        </div>
      )}
    </main>
  )
}

function EventCard({ event }: { event: MyEvent }) {
  const { day, month } = dateBadge(event.eventDate)

  return (
    <li>
      <Link
        to={routes.eventDetail(event.id)}
        data-testid={`event-list-item-${event.id}`}
        className="flex flex-col gap-3.5 rounded-[calc(var(--radius-2xl)*1.15)] bg-surface p-4.5 shadow-elev-sm"
      >
        <div className="flex items-start gap-3.5">
          <div className="grid h-[62px] w-[62px] shrink-0 place-content-center rounded-full bg-accent-500 text-center leading-tight text-accent-900">
            <span className="font-heading text-[22px]">{day}</span>
            <span className="text-[10px] tracking-wide uppercase">{month}</span>
          </div>
          <div className="flex-1">
            <div className="mb-1 font-heading text-[19px] sm:text-[21px]">{event.title}</div>
            <div className="flex items-center gap-1.5 text-[13px] text-ink/62">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
                <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0" />
                <circle cx="12" cy="10" r="3" />
              </svg>
              {event.location ?? formatDate(event.eventDate)}
            </div>
          </div>
        </div>

        {(!event.hasJoined || event.role === 'Organizer') && (
          <div className="flex items-center gap-2.5">
            {!event.hasJoined && (
              <Tag variant="accent" className="ml-auto" >
                <span data-testid={`event-list-item-pending-badge-${event.id}`}>Invitation en attente</span>
              </Tag>
            )}
            {event.hasJoined && event.role === 'Organizer' && (
              <Tag variant="sage" className="ml-auto">
                Organisateur·ice
              </Tag>
            )}
          </div>
        )}
      </Link>
    </li>
  )
}

function EmptyState() {
  return (
    <div data-testid="events-dashboard-empty-message" className="mx-auto my-auto flex max-w-[280px] flex-col items-center pt-10 pb-20 text-center">
      <div className="relative mb-8.5 h-[160px] w-[160px]">
        <div className="absolute inset-0 rounded-full bg-sage-200" />
        <div className="absolute top-7 left-5 h-12 w-12 rounded-full bg-accent-300" />
        <div className="absolute right-4 bottom-5 h-[74px] w-[74px] rounded-full bg-accent-500" />
      </div>
      <h2 className="mb-3 text-[24px]">
        Rien de prévu…
        <br />
        pour l&rsquo;instant
      </h2>
      <p className="mb-3 text-[15px] text-ink/68">
        Vous ne participez encore à aucun événement. Créez un repas, un anniversaire, un week-end — puis invitez qui
        vous voulez.
      </p>
      <Link to={routes.createEvent} data-testid="events-dashboard-empty-cta" className={`${btnPrimary} mt-1.5 px-7.5 text-[16px]`}>
        Créer mon premier événement
      </Link>
    </div>
  )
}
