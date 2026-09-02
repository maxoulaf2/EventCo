import { type SubmitEvent, useState } from 'react'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { useRequestMagicLink } from '../hooks/useRequestMagicLink'

export function RequestMagicLinkForm() {
  const [email, setEmail] = useState('')
  const { toCheckEmail } = useAppNavigate()
  const { mutate, isPending, error } = useRequestMagicLink()

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    mutate(email, {
      onSuccess: () => toCheckEmail({ email }),
    })
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full max-w-sm flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="email" className="text-sm font-medium text-gray-700">
          Adresse email
        </label>
        <input
          id="email"
          name="email"
          type="email"
          required
          autoComplete="email"
          inputMode="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="vous@exemple.com"
          className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
        />
      </div>

      {error && <p className="text-sm text-red-600">{error.message}</p>}

      <button
        type="submit"
        disabled={isPending}
        className="rounded-lg bg-gray-900 px-4 py-3 text-base font-medium text-white transition-colors disabled:opacity-50"
      >
        {isPending ? 'Envoi en cours…' : 'Recevoir un lien de connexion'}
      </button>
    </form>
  )
}
