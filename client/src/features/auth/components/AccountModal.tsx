import { Modal } from '../../../shared/components/Modal'
import { btnSecondary } from '../../../shared/lib/ui'
import { useLogout } from '../hooks/useLogout'

interface AccountModalProps {
  open: boolean
  onClose: () => void
}

export function AccountModal({ open, onClose }: AccountModalProps) {
  const logout = useLogout()

  return (
    <Modal open={open} onClose={onClose} title="Mon compte" testId="account-modal">
      <button
        type="button"
        onClick={() => logout.mutate()}
        disabled={logout.isPending}
        data-testid="account-modal-logout-button"
        className={`${btnSecondary} w-full`}
      >
        Se déconnecter
      </button>
    </Modal>
  )
}
