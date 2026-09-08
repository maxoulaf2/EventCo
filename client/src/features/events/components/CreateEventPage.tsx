import { type SubmitEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { routes } from '../../../shared/lib/routes'
import { useCreateEvent } from '../hooks/useCreateEvent'

export function CreateEventPage() {
  const [title, setTitle] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [location, setLocation] = useState('')
  const [description, setDescription] = useState('')
  const { toEvents } = useAppNavigate()
  const { mutate, isPending, error } = useCreateEvent()

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    mutate(
      {
        title,
        eventDate,
        location: location || undefined,
        description: description || undefined,
      },
      { onSuccess: () => toEvents() },
    )
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-lg flex-col gap-6 p-4">
      <h1 className="text-xl font-semibold md:text-2xl">Nouvel événement</h1>

      <form onSubmit={handleSubmit} className="flex w-full flex-col gap-4">
        <div className="flex flex-col gap-1.5">
          <label htmlFor="title" className="text-sm font-medium text-gray-700">
            Titre
          </label>
          <input
            id="title"
            name="title"
            type="text"
            required
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            placeholder="Repas de Noël"
            className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="eventDate" className="text-sm font-medium text-gray-700">
            Date
          </label>
          <input
            id="eventDate"
            name="eventDate"
            type="date"
            required
            value={eventDate}
            onChange={(event) => setEventDate(event.target.value)}
            className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="location" className="text-sm font-medium text-gray-700">
            Lieu
          </label>
          <input
            id="location"
            name="location"
            type="text"
            value={location}
            onChange={(event) => setLocation(event.target.value)}
            placeholder="Chez Alice"
            className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="description" className="text-sm font-medium text-gray-700">
            Description
          </label>
          <textarea
            id="description"
            name="description"
            rows={3}
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        {error && <p className="text-sm text-red-600">{error.message}</p>}

        <div className="flex gap-3">
          <button
            type="submit"
            disabled={isPending}
            className="flex-1 rounded-lg bg-gray-900 px-4 py-3 text-base font-medium text-white transition-colors disabled:opacity-50"
          >
            {isPending ? 'Création en cours…' : 'Créer l\'événement'}
          </button>
          <Link
            to={routes.events}
            className="flex items-center justify-center rounded-lg border border-gray-300 px-4 py-3 text-base font-medium text-gray-700"
          >
            Annuler
          </Link>
        </div>
      </form>
    </main>
  )
}
