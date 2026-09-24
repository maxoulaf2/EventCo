import { type SubmitEvent, useState } from 'react'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { btnPrimary, fieldLabel, input } from '../../../shared/lib/ui'
import { useVerifyLoginCode } from '../hooks/useVerifyLoginCode'

const CODE_LENGTH = 6

interface VerifyLoginCodeFormProps {
  email: string
}

export function VerifyLoginCodeForm({ email }: VerifyLoginCodeFormProps) {
  const [code, setCode] = useState('')
  const { toEvents, toEventDetail } = useAppNavigate()
  const { mutate, isPending, error } = useVerifyLoginCode()

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    mutate(
      { email, code },
      {
        onSuccess: (result) => (result.eventId ? toEventDetail(result.eventId) : toEvents()),
        // Un code refusé ne se corrige pas chiffre par chiffre : on repart d'un champ vide.
        onError: () => setCode(''),
      },
    )
  }

  return (
    <form onSubmit={handleSubmit} data-testid="verify-login-code-form" className="flex w-full flex-col gap-3.5">
      <div className="flex flex-col gap-1.5 text-left">
        <label htmlFor="code" className={fieldLabel}>
          Code à {CODE_LENGTH} chiffres
        </label>
        <input
          id="code"
          name="code"
          type="text"
          required
          inputMode="numeric"
          autoComplete="one-time-code"
          pattern={`\\d{${CODE_LENGTH}}`}
          maxLength={CODE_LENGTH}
          value={code}
          // Ne garde que les chiffres : tolère un copier-coller avec espaces (« 123 456 »).
          onChange={(event) => setCode(event.target.value.replace(/\D/g, '').slice(0, CODE_LENGTH))}
          placeholder="000000"
          data-testid="verify-login-code-input"
          className={`${input} text-center font-heading tracking-[0.4em]`}
        />
      </div>

      {error && (
        <p data-testid="verify-login-code-error" className="text-sm text-accent-800">
          {error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={isPending || code.length !== CODE_LENGTH}
        data-testid="verify-login-code-submit-button"
        className={`${btnPrimary} w-full`}
      >
        {isPending ? 'Vérification…' : 'Me connecter'}
      </button>
    </form>
  )
}
