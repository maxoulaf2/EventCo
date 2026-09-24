import { type ChangeEvent, useRef } from 'react'
import { Avatar } from '../../../shared/components/Avatar'
import { btnGhost, btnSecondary } from '../../../shared/lib/ui'
import { useRemoveAvatar } from '../hooks/useRemoveAvatar'
import { useUpdateAvatar } from '../hooks/useUpdateAvatar'
import type { CurrentUser } from '../types'

interface UpdateAvatarFormProps {
  currentUser: CurrentUser
}

export function UpdateAvatarForm({ currentUser }: UpdateAvatarFormProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const updateAvatar = useUpdateAvatar()
  const removeAvatar = useRemoveAvatar()
  const isPending = updateAvatar.isPending || removeAvatar.isPending
  const error = updateAvatar.error ?? removeAvatar.error

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    // Réinitialisé pour qu'un nouveau choix du même fichier (ex: après une erreur) redéclenche onChange.
    event.target.value = ''
    if (!file) return

    removeAvatar.reset()
    updateAvatar.mutate(file)
  }

  return (
    <div data-testid="account-modal-avatar-form" className="flex items-center gap-4">
      <Avatar
        name={currentUser.displayName}
        imageUrl={currentUser.avatarUrl}
        size="xl"
        testId="account-modal-avatar-preview"
      />
      <div className="flex min-w-0 flex-1 flex-col items-start gap-1">
        <input
          ref={fileInputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          onChange={handleFileChange}
          data-testid="account-modal-avatar-input"
          className="sr-only"
          tabIndex={-1}
          aria-hidden="true"
        />
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={isPending}
          data-testid="account-modal-avatar-choose-button"
          className={`${btnSecondary} min-h-10 px-4 text-[14px]`}
        >
          {updateAvatar.isPending ? 'Envoi…' : currentUser.avatarUrl ? 'Changer la photo' : 'Ajouter une photo'}
        </button>
        {currentUser.avatarUrl && (
          <button
            type="button"
            onClick={() => {
              updateAvatar.reset()
              removeAvatar.mutate()
            }}
            disabled={isPending}
            data-testid="account-modal-avatar-remove-button"
            className={`${btnGhost} min-h-9 px-2 text-[13px]`}
          >
            Retirer la photo
          </button>
        )}
        {error && (
          <p data-testid="account-modal-avatar-error" className="text-sm text-accent-800">
            {error.message}
          </p>
        )}
      </div>
    </div>
  )
}
