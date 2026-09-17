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
    <form onSubmit={handleSubmit} data-testid="invite-participant-form" className="flex flex-col gap-2">
      <label htmlFor="inviteEmail" className="text-sm font-medium text-gray-700">
        Inviter un participant
      </label>
      <div className="flex gap-2">
        <input
          id="inviteEmail"
          name="inviteEmail"
          type="email"
          required
          autoComplete="email"
          inputMode="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="ami@exemple.com"
          data-testid="invite-participant-email-input"
          className="flex-1 rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
        />
        <button
          type="submit"
          disabled={isPending}
          data-testid="invite-participant-submit-button"
          className="shrink-0 rounded-lg bg-gray-900 px-4 py-3 text-base font-medium text-white transition-colors disabled:opacity-50"
        >
          {isPending ? 'Envoi…' : 'Inviter'}
        </button>
      </div>
      {error && (
        <p data-testid="invite-participant-error" className="text-sm text-red-600">
          {error.message}
        </p>
      )}
      {isSuccess && (
        <p data-testid="invite-participant-success" className="text-sm text-green-700">
          Invitation envoyée.
        </p>
      )}
    </form>
  )
}
