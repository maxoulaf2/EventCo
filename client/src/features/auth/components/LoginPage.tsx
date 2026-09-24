import { Navigate } from 'react-router-dom'
import { routes } from '../../../shared/lib/routes'
import { useCurrentUser } from '../hooks/useCurrentUser'
import { RequestMagicLinkForm } from './RequestMagicLinkForm'

export function LoginPage() {
  const { data: currentUser, isPending } = useCurrentUser()

  if (isPending) {
    return (
      <main data-testid="login-page-loading" className="flex min-h-screen items-center justify-center bg-bg">
        <div
          aria-hidden="true"
          className="h-6 w-6 animate-spin rounded-full border-2 border-accent-300 border-t-accent-700"
        />
      </main>
    )
  }

  if (currentUser) {
    return <Navigate to={routes.events} replace />
  }

  return (
    <main
      data-testid="login-page"
      className="relative flex min-h-screen flex-col items-center justify-center overflow-hidden bg-bg px-6 py-14"
    >
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -top-24 -right-20 h-72 w-72 rounded-full bg-accent-200"
      />
      <div
        aria-hidden="true"
        className="pointer-events-none absolute top-[38%] -left-16 h-32 w-32 rounded-full bg-sage-200"
      />

      <div className="relative flex w-full max-w-sm flex-col">
        <h1 data-testid="login-page-title" className="mb-3.5 text-[44px] leading-none sm:text-[52px]">
          EventCo
        </h1>
        <p className="mb-10 max-w-[290px] text-base text-ink/70">
          On organise à plusieurs. Pas de mot de passe : un code arrive dans votre boîte mail.
        </p>
        <RequestMagicLinkForm />
        <p className="mt-4.5 text-center text-[12.5px] text-ink/55">Première connexion = inscription.</p>
      </div>
    </main>
  )
}
