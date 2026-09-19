import { useEffect, useRef, type ReactNode } from 'react'
import { useParams } from 'react-router-dom'
import { useCurrentUser } from '../../auth/hooks/useCurrentUser'
import { RequestMagicLinkForm } from '../../auth/components/RequestMagicLinkForm'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { useEventInvitePreview } from '../hooks/useEventInvitePreview'
import { useJoinEventViaInviteLink } from '../hooks/useJoinEventViaInviteLink'

function formatDate(iso: string): string {
  const date = new Date(iso)
  const day = date.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
  const time = date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  return `${day.charAt(0).toUpperCase()}${day.slice(1)} · ${time}`
}

export function InviteLinkPage() {
  const { token } = useParams<{ token: string }>()
  const { data: currentUser, isPending: isCurrentUserPending } = useCurrentUser()

  if (isCurrentUserPending) {
    return (
      <Layout>
        <Spinner testId="invite-link-loading-spinner" />
      </Layout>
    )
  }

  if (currentUser) {
    return <JoinAsCurrentUser token={token!} />
  }

  return <PreviewAndRequestLogin token={token!} />
}

function JoinAsCurrentUser({ token }: { token: string }) {
  const { mutate, isPending, isError } = useJoinEventViaInviteLink(token)
  const { toEventDetail } = useAppNavigate()
  const hasRequested = useRef(false)

  useEffect(() => {
    if (hasRequested.current) return

    hasRequested.current = true
    mutate(undefined, {
      onSuccess: (result) => toEventDetail(result.eventId),
    })
  }, [mutate, toEventDetail])

  if (isError) {
    return (
      <Layout>
        <h1 data-testid="invite-link-error" className="mb-3.5 text-2xl">
          Ce lien n&rsquo;est plus valide
        </h1>
        <p className="max-w-sm text-center text-base text-ink/70">
          Il a peut-être été régénéré par l&rsquo;organisateur·ice. Demandez un nouveau lien.
        </p>
      </Layout>
    )
  }

  return (
    <Layout>
      <div data-testid="invite-link-join-pending" className="flex flex-col items-center gap-4">
        <Spinner testId="invite-link-loading-spinner" />
        <p className="text-base text-ink/70">{isPending ? 'On vous ajoute à l’événement…' : 'Un instant…'}</p>
      </div>
    </Layout>
  )
}

function PreviewAndRequestLogin({ token }: { token: string }) {
  const { data: preview, isPending, isError } = useEventInvitePreview(token)

  if (isPending) {
    return (
      <Layout>
        <Spinner testId="invite-link-loading-spinner" />
      </Layout>
    )
  }

  if (isError || !preview) {
    return (
      <Layout>
        <h1 data-testid="invite-link-invalid" className="mb-3.5 text-2xl">
          Ce lien n&rsquo;est plus valide
        </h1>
        <p className="max-w-sm text-center text-base text-ink/70">
          Il a peut-être été régénéré par l&rsquo;organisateur·ice. Demandez-en un nouveau à la personne qui vous
          l&rsquo;a partagé.
        </p>
      </Layout>
    )
  }

  return (
    <Layout>
      <p className="mb-1.5 text-[13px] text-ink/55">Vous êtes invité·e à</p>
      <h1 data-testid="invite-link-preview-title" className="mb-2 text-[30px] sm:text-[34px]">
        {preview.title}
      </h1>
      <p data-testid="invite-link-preview-date" className="mb-8 text-base text-ink/70">
        {formatDate(preview.eventDate)}
      </p>
      <RequestMagicLinkForm eventInviteLinkToken={token} />
      <p className="mt-4.5 text-center text-[12.5px] text-ink/55">Première connexion = inscription.</p>
    </Layout>
  )
}

function Spinner({ testId }: { testId: string }) {
  return (
    <div
      aria-hidden="true"
      data-testid={testId}
      className="h-6 w-6 animate-spin rounded-full border-2 border-accent-300 border-t-accent-700"
    />
  )
}

function Layout({ children }: { children: ReactNode }) {
  return (
    <main
      data-testid="invite-link-page"
      className="flex min-h-screen flex-col items-center justify-center bg-bg px-6 py-14 text-center"
    >
      <div className="flex w-full max-w-sm flex-col items-center">{children}</div>
    </main>
  )
}
