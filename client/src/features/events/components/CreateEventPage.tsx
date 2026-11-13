import { type SubmitEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAppNavigate } from '../../../shared/hooks/useAppNavigate'
import { btnGhost, btnPrimary, fieldLabel, input, textarea } from '../../../shared/lib/ui'
import { routes } from '../../../shared/lib/routes'
import { useCreateEvent } from '../hooks/useCreateEvent'
import { useUpdateEventImage } from '../hooks/useUpdateEventImage'
import type { DateOptionInput } from '../types'
import { EventImageField, type SelectedEventImage } from './EventImageField'

// Mêmes bornes que l'agrégat Event côté API (MinDateOptions / MaxDateOptions).
const MIN_DATE_OPTIONS = 2
const MAX_DATE_OPTIONS = 10

const DATE_MODES = [
  { value: 'fixed', label: 'Date fixe' },
  { value: 'poll', label: 'Proposer plusieurs dates' },
] as const

export function CreateEventPage() {
  const [title, setTitle] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [eventTime, setEventTime] = useState('')
  const [dateMode, setDateMode] = useState<'fixed' | 'poll'>('fixed')
  const [dateOptions, setDateOptions] = useState<DateOptionInput[]>([
    { date: '', time: '' },
    { date: '', time: '' },
  ])
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

  function updateDateOption(index: number, change: Partial<DateOptionInput>) {
    setDateOptions(dateOptions.map((option, i) => (i === index ? { ...option, ...change } : option)))
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
        ...(dateMode === 'poll' ? { dateOptions } : { eventDate, eventTime: eventTime || undefined }),
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

            <div role="radiogroup" aria-label="Choix de la date" className="flex gap-2">
              {DATE_MODES.map((mode) => (
                <label
                  key={mode.value}
                  className={`inline-flex min-h-11 flex-1 cursor-pointer items-center justify-center rounded-full px-4 text-center text-[14px] transition-colors has-focus-visible:outline-2 has-focus-visible:outline-accent-600 ${
                    dateMode === mode.value ? 'bg-accent-500 text-accent-900' : 'border border-border text-ink/70'
                  }`}
                >
                  <input
                    type="radio"
                    name="dateMode"
                    checked={dateMode === mode.value}
                    onChange={() => setDateMode(mode.value)}
                    data-testid={`create-event-date-mode-${mode.value}`}
                    className="sr-only"
                  />
                  {mode.label}
                </label>
              ))}
            </div>

            {dateMode === 'fixed' && (
              <>
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
              </>
            )}

            {dateMode === 'poll' && (
              <div className="flex flex-col gap-2.5">
                <p className={fieldLabel}>Dates proposées au vote — l&rsquo;heure est optionnelle</p>
                {dateOptions.map((option, index) => (
                  <div key={index} className="flex items-center gap-2">
                    <input
                      type="date"
                      required
                      aria-label={`Date proposée ${index + 1}`}
                      value={option.date}
                      onChange={(event) => updateDateOption(index, { date: event.target.value })}
                      data-testid={`create-event-date-option-date-input-${index}`}
                      className={`${input} min-w-0 flex-[3]`}
                    />
                    <input
                      type="time"
                      aria-label={`Heure de la date proposée ${index + 1}`}
                      value={option.time}
                      onChange={(event) => updateDateOption(index, { time: event.target.value })}
                      data-testid={`create-event-date-option-time-input-${index}`}
                      className={`${input} min-w-0 flex-[2]`}
                    />
                    {dateOptions.length > MIN_DATE_OPTIONS && (
                      <button
                        type="button"
                        onClick={() => setDateOptions(dateOptions.filter((_, i) => i !== index))}
                        aria-label={`Retirer la date proposée ${index + 1}`}
                        data-testid={`create-event-date-option-remove-button-${index}`}
                        className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full text-ink/70 transition-colors hover:bg-ink/5"
                      >
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.75" strokeLinecap="round">
                          <path d="M18 6 6 18" />
                          <path d="m6 6 12 12" />
                        </svg>
                      </button>
                    )}
                  </div>
                ))}
                {dateOptions.length < MAX_DATE_OPTIONS && (
                  <button
                    type="button"
                    onClick={() => setDateOptions([...dateOptions, { date: '', time: '' }])}
                    data-testid="create-event-date-option-add-button"
                    className={`${btnGhost} self-start`}
                  >
                    Ajouter une date
                  </button>
                )}
              </div>
            )}

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
