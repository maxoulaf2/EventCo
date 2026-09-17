import { type SubmitEvent, useState } from 'react'
import { useInviteParticipant } from '../hooks/useInviteParticipant'

interface InviteParticipantFormProps {
  eventId: string
}

export function InviteParticipantForm({ eventId }: InviteParticipantFormProps) {
  const [email, setEmail] = useState('')
  const { mutate, isPending, error, isSuccess } = useInviteParticipant(eventId)

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    mutate(email, { onSuccess: () => setEmail('') })
  }

  return (
    <div className="flex flex-col gap-1.5">
      <form
        onSubmit={handleSubmit}
        data-testid="invite-participant-form"
        className="flex min-h-14 items-center gap-2 rounded-full border border-border bg-sand-100 py-1.5 pr-1.5 pl-4.5"
      >
        <label htmlFor="inviteEmail" className="sr-only">
          Inviter un participant
        </label>
        <input
          id="inviteEmail"
          name="inviteEmail"
          type="email"
          required
          autoComplete="email"
          inputMode="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="email@exemple.com"
          data-testid="invite-participant-email-input"
          className="flex-1 bg-transparent text-[15px] text-ink placeholder:text-ink/45 focus-visible:outline-none"
        />
        <button
          type="submit"
          disabled={isPending}
          data-testid="invite-participant-submit-button"
          className="inline-flex min-h-11 shrink-0 items-center rounded-full bg-accent-500 px-4.5 text-[13.5px] font-heading text-accent-900 transition-colors hover:bg-accent-400 disabled:cursor-not-allowed disabled:opacity-45"
        >
          {isPending ? 'Envoi…' : 'Inviter'}
        </button>
      </form>
      <p className="ml-4.5 text-[12.5px] text-ink/55">Ils recevront un lien — pas de compte à créer.</p>
      {error && (
        <p data-testid="invite-participant-error" className="text-sm text-accent-800">
          {error.message}
        </p>
      )}
      {isSuccess && (
        <p data-testid="invite-participant-success" className="text-sm text-sage-700">
          Invitation envoyée.
        </p>
      )}
    </div>
  )
}
