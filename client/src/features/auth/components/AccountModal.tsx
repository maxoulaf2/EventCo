import { Modal } from '../../../shared/components/Modal'
import { btnSecondary } from '../../../shared/lib/ui'
import { useCurrentUser } from '../hooks/useCurrentUser'
import { useLogout } from '../hooks/useLogout'
import { UpdateDisplayNameForm } from './UpdateDisplayNameForm'

interface AccountModalProps {
  open: boolean
  onClose: () => void
}

export function AccountModal({ open, onClose }: AccountModalProps) {
  const { data: currentUser } = useCurrentUser()
  const logout = useLogout()

  return (
    <Modal open={open} onClose={onClose} title="Mon compte" testId="account-modal">
      <div className="flex flex-col gap-5">
        {currentUser && <UpdateDisplayNameForm currentDisplayName={currentUser.displayName} />}
        <button
          type="button"
          onClick={() => logout.mutate()}
          disabled={logout.isPending}
          data-testid="account-modal-logout-button"
          className={`${btnSecondary} w-full`}
        >
          Se déconnecter
        </button>
      </div>
    </Modal>
  )
}
