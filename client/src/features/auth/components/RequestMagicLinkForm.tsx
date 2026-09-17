import { type SubmitEvent, useState } from 'react'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { fieldLabel, input } from '../../../shared/lib/ui'
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
    <form onSubmit={handleSubmit} data-testid="request-magic-link-form" className="flex w-full flex-col gap-3.5">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="email" className={fieldLabel}>
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
          data-testid="request-magic-link-email-input"
          className={input}
        />
      </div>

      {error && (
        <p data-testid="request-magic-link-error" className="text-sm text-accent-800">
          {error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={isPending}
        data-testid="request-magic-link-submit-button"
        className="mt-1 inline-flex min-h-14 w-full items-center justify-center rounded-full bg-accent-500 text-[17px] font-heading text-accent-900 transition-colors hover:bg-accent-400 active:bg-accent-600 disabled:cursor-not-allowed disabled:opacity-45"
      >
        {isPending ? 'Envoi en cours…' : 'Recevoir mon lien'}
      </button>
    </form>
  )
}
