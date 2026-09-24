import { Link, Navigate } from 'react-router-dom'
import { useLocationState } from '../../../shared/hooks/useLocationState'
import { btnGhost, btnSecondary } from '../../../shared/lib/ui'
import { routes, type CheckEmailNavigationState } from '../../../shared/lib/routes'
import { useRequestMagicLink } from '../hooks/useRequestMagicLink'
import { VerifyLoginCodeForm } from './VerifyLoginCodeForm'

export function CheckEmailPage() {
  const state = useLocationState<CheckEmailNavigationState>()
  const { mutate, isPending, isSuccess } = useRequestMagicLink()

  if (!state) {
    return <Navigate to={routes.login} replace />
  }

  const email = state.email
  const eventInviteLinkToken = state.eventInviteLinkToken

  return (
    <main data-testid="check-email-page" className="relative flex min-h-screen flex-col bg-bg">
      <div className="flex min-h-14 items-center gap-2 p-3.5">
        <Link
          to={routes.login}
          aria-label="Retour"
          data-testid="check-email-page-back-icon-link"
          className="inline-flex h-11 w-11 items-center justify-center rounded-full text-ink/70 transition-colors hover:bg-ink/5"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
            <path d="m12 19-7-7 7-7" />
            <path d="M19 12H5" />
          </svg>
        </Link>
      </div>

      <div className="mx-auto my-auto flex w-full max-w-sm flex-col items-center px-6 pb-16 text-center">
        <div className="mb-7.5 grid h-[100px] w-[100px] place-items-center rounded-full bg-accent-200 text-accent-800">
          <svg width="42" height="42" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
            <rect x="2" y="4" width="20" height="16" rx="2" />
            <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
          </svg>
        </div>
        <h1 data-testid="check-email-page-title" className="mb-3.5 text-[30px] sm:text-[34px]">
          C&rsquo;est parti
        </h1>
        <p className="max-w-[300px] text-base text-ink/70">
          Un code de connexion vient de partir vers
          <br />
          <span data-testid="check-email-page-email" className="font-semibold text-accent-700">
            {email}
          </span>
        </p>
        <span className="mt-1.5 inline-flex rounded-full bg-sand-100 px-3.5 py-1.5 text-xs font-medium text-sand-800">
          Valable 15 minutes
        </span>

        <div className="mt-9 w-full">
          <VerifyLoginCodeForm email={email} />
        </div>

        <div className="mt-6 flex w-full flex-col gap-1.5">
          <button
            type="button"
            onClick={() => mutate({ email, eventInviteLinkToken })}
            disabled={isPending}
            data-testid="check-email-page-resend-button"
            className={`${btnSecondary} w-full`}
          >
            {isPending ? 'Envoi…' : isSuccess ? 'Code renvoyé' : 'Renvoyer le code'}
          </button>
          <Link to={routes.login} data-testid="check-email-page-back-link" className={`${btnGhost} w-full`}>
            Utiliser une autre adresse
          </Link>
        </div>
      </div>
    </main>
  )
}
