import { type SubmitEvent, useState } from 'react'
import { btnPrimary, fieldLabel, input } from '../../../shared/lib/ui'
import { useUpdateProfile } from '../hooks/useUpdateProfile'

interface UpdateDisplayNameFormProps {
  currentDisplayName: string
}

export function UpdateDisplayNameForm({ currentDisplayName }: UpdateDisplayNameFormProps) {
  const [displayName, setDisplayName] = useState(currentDisplayName)
  const { mutate, isPending, error, isSuccess } = useUpdateProfile()

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    mutate(displayName)
  }

  return (
    <form onSubmit={handleSubmit} data-testid="account-modal-display-name-form" className="flex flex-col gap-1.5">
      <label htmlFor="displayName" className={fieldLabel}>
        Nom d'affichage
      </label>
      <input
        id="displayName"
        name="displayName"
        type="text"
        required
        value={displayName}
        onChange={(event) => setDisplayName(event.target.value)}
        data-testid="account-modal-display-name-input"
        className={input}
      />
      <button
        type="submit"
        disabled={isPending}
        data-testid="account-modal-display-name-submit-button"
        className={`${btnPrimary} mt-1`}
      >
        {isPending ? 'Enregistrement…' : 'Enregistrer'}
      </button>
      {error && (
        <p data-testid="account-modal-display-name-error" className="text-sm text-accent-800">
          {error.message}
        </p>
      )}
      {isSuccess && (
        <p data-testid="account-modal-display-name-success" className="text-sm text-sage-700">
          Nom d'affichage mis à jour.
        </p>
      )}
    </form>
  )
}
