import { useState } from 'react'
import { copyTextToClipboard } from '../../../shared/lib/clipboard'
import { routes } from '../../../shared/lib/routes'
import { useRegenerateInviteLink } from '../hooks/useRegenerateInviteLink'

interface InviteLinkShareBoxProps {
  eventId: string
  token: string
}

export function InviteLinkShareBox({ eventId, token }: InviteLinkShareBoxProps) {
  const [isCopied, setIsCopied] = useState(false)
  const [copyFailed, setCopyFailed] = useState(false)
  const [isConfirmingRegenerate, setIsConfirmingRegenerate] = useState(false)
  const { mutate, isPending } = useRegenerateInviteLink(eventId)

  const inviteUrl = `${window.location.origin}${routes.inviteLink(token)}`

  async function handleCopy() {
    const succeeded = await copyTextToClipboard(inviteUrl)
    setCopyFailed(!succeeded)

    if (succeeded) {
      setIsCopied(true)
      setTimeout(() => setIsCopied(false), 2000)
    }
  }

  function handleRegenerate() {
    mutate(undefined, { onSuccess: () => setIsConfirmingRegenerate(false) })
  }

  return (
    <div data-testid="invite-link-share-box" className="flex flex-col gap-1.5">
      <div className="flex min-h-14 items-center gap-2 rounded-full border border-border bg-sand-100 py-1.5 pr-1.5 pl-4.5">
        <span data-testid="invite-link-share-url" className="flex-1 truncate text-[13.5px] text-ink/70">
          {inviteUrl}
        </span>
        <button
          type="button"
          onClick={handleCopy}
          data-testid="invite-link-copy-button"
          className="inline-flex min-h-11 shrink-0 items-center rounded-full bg-accent-500 px-4.5 text-[13.5px] font-heading text-accent-900 transition-colors hover:bg-accent-400"
        >
          {isCopied ? 'Copié !' : 'Copier'}
        </button>
      </div>

      {copyFailed && (
        <p data-testid="invite-link-copy-error" className="ml-4.5 text-[12.5px] text-accent-800">
          Copie impossible sur cet appareil — sélectionnez le lien manuellement.
        </p>
      )}

      {isConfirmingRegenerate ? (
        <p className="ml-4.5 flex items-center gap-2 text-[12.5px] text-ink/55">
          L&rsquo;ancien lien cessera de fonctionner.
          <button
            type="button"
            onClick={handleRegenerate}
            disabled={isPending}
            data-testid="invite-link-regenerate-confirm-button"
            className="font-semibold text-accent-800 underline underline-offset-2 disabled:cursor-not-allowed disabled:opacity-45"
          >
            {isPending ? 'Régénération…' : 'Confirmer'}
          </button>
          <button type="button" onClick={() => setIsConfirmingRegenerate(false)} className="underline underline-offset-2">
            Annuler
          </button>
        </p>
      ) : (
        <button
          type="button"
          onClick={() => setIsConfirmingRegenerate(true)}
          data-testid="invite-link-regenerate-button"
          className="ml-4.5 self-start text-[12.5px] text-ink/55 underline underline-offset-2"
        >
          Régénérer le lien
        </button>
      )}
    </div>
  )
}
