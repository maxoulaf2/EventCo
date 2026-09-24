import { type SubmitEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { btnGhost, btnPrimary, fieldLabel, input, textarea } from '../../../shared/lib/ui'
import { routes } from '../../../shared/lib/routes'
import { useCreateEvent } from '../hooks/useCreateEvent'
import { useUpdateEventImage } from '../hooks/useUpdateEventImage'
import { EventImageField, type SelectedEventImage } from './EventImageField'

export function CreateEventPage() {
  const [title, setTitle] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [eventTime, setEventTime] = useState('')
  const [location, setLocation] = useState('')
  const [description, setDescription] = useState('')
  const [image, setImage] = useState<SelectedEventImage | null>(null)
  const [isPreparingImage, setIsPreparingImage] = useState(false)
  // Renseigné si l'événement a été créé mais que l'envoi de son image a échoué : une nouvelle
  // soumission ne relance alors que l'envoi de l'image, sans recréer l'événement.
  const [createdEventId, setCreatedEventId] = useState<string | null>(null)
  const { toEvents } = useAppNavigate()
  const createEvent = useCreateEvent()
  const updateEventImage = useUpdateEventImage()
  const isPending = createEvent.isPending || updateEventImage.isPending

  function uploadImageThenLeave(eventId: string, selectedImage: SelectedEventImage | null) {
    if (!selectedImage) {
      toEvents()
      return
    }

    updateEventImage.mutate({ eventId, image: selectedImage.blob }, { onSuccess: () => toEvents() })
  }

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()

    if (createdEventId) {
      uploadImageThenLeave(createdEventId, image)
      return
    }

    createEvent.mutate(
      {
        title,
        eventDate,
        eventTime: eventTime || undefined,
        location: location || undefined,
        description: description || undefined,
      },
      {
        onSuccess: ({ id }) => {
          setCreatedEventId(id)
          uploadImageThenLeave(id, image)
        },
      },
    )
  }

  function submitLabel() {
    if (createEvent.isPending) return 'Création en cours…'
    if (updateEventImage.isPending) return "Envoi de l'image…"
    if (createdEventId) return "Réessayer l'envoi de l'image"
    return "Créer l'événement"
  }

  return (
    <main data-testid="create-event-page" className="relative mx-auto flex min-h-screen w-full max-w-lg flex-col bg-bg">
      <div className="flex min-h-14 items-center p-3.5">
        <Link
          to={routes.events}
          aria-label="Fermer"
          data-testid="create-event-cancel-link"
          className="inline-flex h-11 w-11 items-center justify-center rounded-full text-ink/70 transition-colors hover:bg-ink/5"
        >
          <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round">
            <path d="M18 6 6 18" />
            <path d="m6 6 12 12" />
          </svg>
        </Link>
      </div>

      <div className="flex-1 px-5.5 pt-3">
        <h1 data-testid="create-event-page-title" className="mb-1.5 text-[28px] sm:text-[30px]">
          Nouvel événement
        </h1>
        <p className="mb-5 text-sm text-ink/62">Deux champs suffisent pour commencer.</p>

        <form onSubmit={handleSubmit} data-testid="create-event-form" className="flex w-full flex-col gap-4.5">
          {/* Figé une fois l'événement créé : seule l'image peut encore être renvoyée (cf. createdEventId). */}
          <fieldset disabled={createdEventId !== null} className="contents">
            <div className="flex flex-col gap-1.5">
              <label htmlFor="title" className={fieldLabel}>
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
                data-testid="create-event-title-input"
                className={input}
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="eventDate" className={fieldLabel}>
                Date
              </label>
              <input
                id="eventDate"
                name="eventDate"
                type="date"
                required
                value={eventDate}
                onChange={(event) => setEventDate(event.target.value)}
                data-testid="create-event-date-input"
                className={input}
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="eventTime" className={fieldLabel}>
                Heure <span className="text-ink/50">— optionnel</span>
              </label>
              <input
                id="eventTime"
                name="eventTime"
                type="time"
                value={eventTime}
                onChange={(event) => setEventTime(event.target.value)}
                data-testid="create-event-time-input"
                className={input}
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="location" className={fieldLabel}>
                Lieu <span className="text-ink/50">— optionnel</span>
              </label>
              <input
                id="location"
                name="location"
                type="text"
                value={location}
                onChange={(event) => setLocation(event.target.value)}
                placeholder="Chez Alice, 12 rue des Lilas"
                data-testid="create-event-location-input"
                className={input}
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="description" className={fieldLabel}>
                Un mot pour les invités <span className="text-ink/50">— optionnel</span>
              </label>
              <textarea
                id="description"
                name="description"
                rows={3}
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                placeholder="Dîner sans chichi, on répartit les plats ensemble."
                data-testid="create-event-description-input"
                className={textarea}
              />
            </div>
          </fieldset>

          <EventImageField
            image={image}
            onChange={(selectedImage) => {
              updateEventImage.reset()
              setImage(selectedImage)
            }}
            isPreparing={isPreparingImage}
            onPreparingChange={setIsPreparingImage}
            disabled={isPending}
          />

          {createEvent.error && (
            <p data-testid="create-event-error" className="text-sm text-accent-800">
              {createEvent.error.message}
            </p>
          )}

          {updateEventImage.error && (
            <div className="flex flex-col items-start gap-1">
              <p data-testid="create-event-image-error" className="text-sm text-accent-800">
                L'événement a été créé, mais son image n'a pas pu être envoyée : {updateEventImage.error.message}
              </p>
              <Link to={routes.events} data-testid="create-event-skip-image-link" className={`${btnGhost} min-h-9 px-2 text-[13px]`}>
                Continuer sans image
              </Link>
            </div>
          )}

          <button
            type="submit"
            disabled={isPending || isPreparingImage}
            data-testid="create-event-submit-button"
            className={`${btnPrimary} mt-2 min-h-14 w-full text-base`}
          >
            {submitLabel()}
          </button>
        </form>
      </div>
    </main>
  )
}
