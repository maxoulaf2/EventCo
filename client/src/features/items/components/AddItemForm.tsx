import { type SubmitEvent, useState } from 'react'
import { useCreateItem } from '../hooks/useCreateItem'

interface AddItemFormProps {
  eventId: string
}

// Ajout rapide, une seule zone de saisie (cf. maquette 1f) : l'article part sans quantité
// — modifiable ensuite depuis le détail de l'article.
export function AddItemForm({ eventId }: AddItemFormProps) {
  const [title, setTitle] = useState('')
  const { mutate, isPending, error } = useCreateItem(eventId)

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!title.trim()) return
    mutate({ title }, { onSuccess: () => setTitle('') })
  }

  return (
    <div className="mt-3 flex-none bg-bg pt-2">
      {error && (
        <p data-testid="add-item-error" className="mb-2 text-sm text-accent-800">
          {error.message}
        </p>
      )}
      <form
        onSubmit={handleSubmit}
        data-testid="add-item-form"
        className="flex min-h-14 items-center gap-2 rounded-full border border-dashed border-ink/25 bg-sand-100 py-1.5 pr-1.5 pl-4.5"
      >
        <label htmlFor="itemTitle" className="sr-only">
          Ajouter un article
        </label>
        <input
          id="itemTitle"
          name="itemTitle"
          type="text"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="Ajouter un article…"
          data-testid="add-item-title-input"
          className="flex-1 bg-transparent text-[15px] text-ink placeholder:text-ink/45 focus-visible:outline-none"
        />
        <button
          type="submit"
          disabled={isPending || !title.trim()}
          aria-label="Ajouter"
          data-testid="add-item-submit-button"
          className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-accent-500 text-accent-900 transition-colors hover:bg-accent-400 active:bg-accent-600 disabled:cursor-not-allowed disabled:opacity-45"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round">
            <path d="M5 12h14" />
            <path d="M12 5v14" />
          </svg>
        </button>
      </form>
    </div>
  )
}
