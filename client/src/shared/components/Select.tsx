import { useEffect, useRef, useState } from 'react'

interface SelectOption {
  value: string
  label: string
}

interface SelectProps {
  id?: string
  value: string
  onChange: (value: string) => void
  options: SelectOption[]
  disabled?: boolean
  testId: string
}

// Remplace un <select> natif : la liste déroulante native ne peut pas être stylisée de façon fiable
// entre navigateurs (fond blanc brut, curseur au survol de la flèche selon l'OS/le thème) et son
// outline de focus par défaut, positionné hors de la boîte (`outline-offset`), se fait rogner par le
// premier ancêtre `overflow-hidden` — cf. mise en page de EventDetailPage. Reconstruit en bouton +
// liste stylisée (`focus-visible:ring-inset`, jamais hors de la boîte donc jamais rogné) plutôt que de
// tenter de contourner ces limitations du <select> natif.
export function Select({ id, value, onChange, options, disabled, testId }: SelectProps) {
  const [open, setOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return

    function handlePointerDown(event: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(event.target as Node)) {
        setOpen(false)
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') setOpen(false)
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [open])

  const selected = options.find((option) => option.value === value)

  return (
    <div ref={rootRef} className="relative">
      <button
        type="button"
        id={id}
        data-testid={testId}
        aria-haspopup="listbox"
        aria-expanded={open}
        disabled={disabled}
        onClick={() => setOpen((isOpen) => !isOpen)}
        className="flex w-full items-center justify-between gap-2 rounded-full bg-surface px-4 py-3 text-left text-[15px] text-ink outline-none transition-colors hover:bg-sand-100 focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-accent-600 disabled:cursor-not-allowed disabled:opacity-60"
      >
        <span>{selected?.label}</span>
        <svg
          aria-hidden="true"
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2.75"
          strokeLinecap="round"
          strokeLinejoin="round"
          className={`shrink-0 text-ink/50 transition-transform ${open ? 'rotate-180' : ''}`}
        >
          <path d="m6 9 6 6 6-6" />
        </svg>
      </button>

      {open && (
        <ul
          role="listbox"
          data-testid={`${testId}-list`}
          className="absolute z-10 mt-1.5 w-full overflow-hidden rounded-2xl bg-bg py-1.5 shadow-elev-md ring-1 ring-border"
        >
          {options.map((option) => (
            <li key={option.value} role="option" aria-selected={option.value === value}>
              <button
                type="button"
                data-testid={`${testId}-option-${option.value}`}
                onClick={() => {
                  onChange(option.value)
                  setOpen(false)
                }}
                className={`flex min-h-11 w-full items-center px-4 text-left text-[15px] outline-none transition-colors hover:bg-accent-100 focus-visible:bg-accent-100 ${
                  option.value === value ? 'font-heading text-accent-900' : 'text-ink'
                }`}
              >
                {option.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
