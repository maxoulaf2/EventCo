import { useEffect } from 'react'
import type { ReactNode } from 'react'

interface ModalProps {
  open: boolean
  onClose: () => void
  title: string
  testId: string
  children: ReactNode
}

export function Modal({ open, onClose, title, testId, children }: ModalProps) {
  useEffect(() => {
    if (!open) return

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') onClose()
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [open, onClose])

  if (!open) return null

  return (
    <div
      data-testid={`${testId}-overlay`}
      onClick={onClose}
      className="fixed inset-0 z-50 flex items-end justify-center bg-ink/45 sm:items-center sm:p-6"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label={title}
        data-testid={testId}
        onClick={(event) => event.stopPropagation()}
        className="flex max-h-[85vh] w-full flex-col overflow-hidden rounded-t-3xl bg-bg sm:max-w-md sm:rounded-3xl"
      >
        <div className="flex items-center gap-3 border-b border-border px-5.5 py-4.5">
          <h2 className="flex-1 text-lg font-heading text-ink">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="Fermer"
            data-testid={`${testId}-close-button`}
            className="grid h-9.5 w-9.5 shrink-0 place-items-center rounded-full text-ink/60 transition-colors hover:bg-sand-100 hover:text-ink"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round" strokeLinejoin="round">
              <path d="M18 6 6 18" />
              <path d="m6 6 12 12" />
            </svg>
          </button>
        </div>
        <div className="flex-1 overflow-auto px-5.5 py-5">{children}</div>
      </div>
    </div>
  )
}
