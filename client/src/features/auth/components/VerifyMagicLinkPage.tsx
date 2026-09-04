import { useEffect, useRef, type ReactNode } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { routes } from '../../../shared/lib/routes'
import { useVerifyMagicLink } from '../hooks/useVerifyMagicLink'

export function VerifyMagicLinkPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const { mutate, isPending, isSuccess, isError, data } = useVerifyMagicLink()
  const { toEvents } = useAppNavigate()
  const hasRequested = useRef(false)

  useEffect(() => {
    if (!token || hasRequested.current) return

    hasRequested.current = true
    mutate(token)
  }, [token, mutate])

  useEffect(() => {
    if (isSuccess) {
      toEvents()
    }
  }, [isSuccess, toEvents])

  if (!token) {
    return (
      <VerifyMessage
        title="Lien invalide"
        message="Ce lien de connexion est incomplet. Redemandez-en un nouveau."
      />
    )
  }

  if (isError) {
    return (
      <VerifyMessage
        title="Ce lien n'est plus valide"
        message="Il a peut-être expiré ou déjà été utilisé. Redemandez un nouveau lien."
      />
    )
  }

  if (isSuccess && data) {
    return (
      <VerifyMessage title={`Bienvenue, ${data.displayName}`} message="Vous êtes connecté." />
    )
  }

  return (
    <VerifyMessage
      title="Connexion en cours…"
      message="Un instant, on vérifie votre lien."
      isPending={isPending}
    />
  )
}

function VerifyMessage({
  title,
  message,
  isPending = false,
}: {
  title: string
  message: ReactNode
  isPending?: boolean
}) {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 p-4 text-center">
      <h1 className="text-xl font-semibold md:text-2xl">{title}</h1>
      <p className="max-w-sm text-sm text-gray-600 md:text-base">{message}</p>
      {isPending ? (
        <div
          aria-hidden
          className="h-6 w-6 animate-spin rounded-full border-2 border-gray-300 border-t-gray-900"
        />
      ) : (
        <Link to={routes.login} className="text-sm font-medium text-gray-900 underline underline-offset-2">
          Retour à l'accueil
        </Link>
      )}
    </main>
  )
}
