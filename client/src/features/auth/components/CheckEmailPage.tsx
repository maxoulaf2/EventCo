import { Link, Navigate } from 'react-router-dom'
import { useLocationState } from '../../../shared/hooks/useLocationState'
import { routes, type CheckEmailNavigationState } from '../../../shared/lib/routes'

export function CheckEmailPage() {
  const state = useLocationState<CheckEmailNavigationState>()

  if (!state) {
    return <Navigate to={routes.login} replace />
  }

  const email = state.email

  return (
    <main data-testid="check-email-page" className="flex min-h-screen flex-col items-center justify-center gap-4 p-4 text-center">
      <h1 data-testid="check-email-page-title" className="text-xl font-semibold md:text-2xl">Vérifiez votre boîte mail</h1>
      <p className="max-w-sm text-sm text-gray-600 md:text-base">
        {
          <>
            Un lien de connexion a été envoyé à{' '}
            <span data-testid="check-email-page-email" className="font-medium text-gray-900">{email}</span>.
          </>
        }{' '}
        Il expire dans 15 minutes.
      </p>
      <Link
        to={routes.login}
        data-testid="check-email-page-back-link"
        className="text-sm font-medium text-gray-900 underline underline-offset-2"
      >
        Utiliser une autre adresse
      </Link>
    </main>
  )
}
