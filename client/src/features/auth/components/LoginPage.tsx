import { RequestMagicLinkForm } from './RequestMagicLinkForm'

export function LoginPage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-8 p-4">
      <div className="flex flex-col items-center gap-2 text-center">
        <h1 className="text-2xl font-semibold md:text-3xl">EventCo</h1>
        <p className="text-sm text-gray-600 md:text-base">
          Connectez-vous sans mot de passe : on vous envoie un lien par email.
        </p>
      </div>
      <RequestMagicLinkForm />
    </main>
  )
}
