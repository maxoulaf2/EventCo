import { type SubmitEvent, useState } from 'react'
import { Avatar } from '../../../shared/components/Avatar'
import { Tag } from '../../../shared/components/Tag'
import { btnSecondary, input } from '../../../shared/lib/ui'
import { formatEventDate } from '../formatEventDate'
import { useDatePollActions } from '../hooks/useDatePollActions'
import type { EventDetail } from '../types'

// Mêmes bornes que l'agrégat Event côté API (MinDateOptions / MaxDateOptions).
const MIN_DATE_OPTIONS = 2
const MAX_DATE_OPTIONS = 10

const smallButton =
  'inline-flex min-h-9.5 shrink-0 items-center rounded-full border border-border px-3.5 text-xs font-medium text-ink/75 transition-colors hover:bg-ink/5 disabled:cursor-not-allowed disabled:opacity-50'

interface DatePollProps {
  event: EventDetail
  currentUserId: string | undefined
  canVote: boolean
  canManage: boolean
}

/** Sondage de date d'un événement sans date : chaque participant coche les options qui lui conviennent,
 * un organisateur retient une option pour clore le sondage. */
export function DatePoll({ event, currentUserId, canVote, canManage }: DatePollProps) {
  const { vote, withdrawVote, addOption, removeOption, chooseDate } = useDatePollActions(event.id)
  const [newDate, setNewDate] = useState('')
  const [newTime, setNewTime] = useState('')

  const options = event.dateOptions
  const bestScore = Math.max(0, ...options.map((option) => option.voterUserIds.length))
  const isVoting = vote.isPending || withdrawVote.isPending
  const isManaging = addOption.isPending || removeOption.isPending || chooseDate.isPending
  const error = vote.error ?? withdrawVote.error ?? addOption.error ?? removeOption.error ?? chooseDate.error

  function handleAddOption(submitEvent: SubmitEvent<HTMLFormElement>) {
    submitEvent.preventDefault()
    addOption.mutate(
      { date: newDate, time: newTime },
      {
        onSuccess: () => {
          setNewDate('')
          setNewTime('')
        },
      },
    )
  }

  function handleChoose(dateOptionId: string, label: string) {
    // Clôture définitive : pas de réouverture du sondage ensuite. Une option passée est refusée par l'API,
    // dont le message s'affiche sous le sondage.
    if (window.confirm(`Retenir « ${label} » ? Le sondage sera clos définitivement.`)) {
      chooseDate.mutate(dateOptionId)
    }
  }

  return (
    <section data-testid="date-poll" className="mt-4 flex flex-col gap-2.5">
      <p className="ml-4 text-[10px] tracking-[0.11em] text-accent-700 uppercase">
        {canVote ? 'Cochez les dates qui vous conviennent' : 'Dates proposées'}
      </p>

      <ul className="flex flex-col gap-2.5">
        {options.map((option) => {
          const label = formatEventDate(option.date)
          const voteCount = option.voterUserIds.length
          const hasVoted = currentUserId !== undefined && option.voterUserIds.includes(currentUserId)
          const isLeading = voteCount > 0 && voteCount === bestScore
          const voters = event.participants.filter((participant) => option.voterUserIds.includes(participant.userId))

          return (
            <li
              key={option.id}
              data-testid={`date-poll-option-${option.id}`}
              className={`flex flex-col gap-2 rounded-[1.75rem] px-3.5 py-2.5 ${isLeading ? 'bg-sage-200' : 'bg-surface'}`}
            >
              <label className="flex min-h-11 items-center gap-3">
                <input
                  type="checkbox"
                  checked={hasVoted}
                  disabled={!canVote || isVoting}
                  onChange={() => (hasVoted ? withdrawVote : vote).mutate(option.id)}
                  data-testid={`date-poll-vote-checkbox-${option.id}`}
                  className="h-5 w-5 shrink-0 accent-sage-600"
                />
                <span className="min-w-0 grow text-[15px] font-semibold text-ink">{label}</span>
                {isLeading && (
                  <Tag variant="accent">
                    <span data-testid={`date-poll-leading-badge-${option.id}`}>En tête</span>
                  </Tag>
                )}
              </label>

              <div className="flex flex-wrap items-center gap-x-3 gap-y-2 pl-8">
                <span data-testid={`date-poll-vote-count-${option.id}`} className="text-[12.5px] text-ink/60">
                  {voteCount} {voteCount > 1 ? 'votes' : 'vote'}
                </span>
                <span className="flex">
                  {voters.map((voter) => (
                    <span key={voter.userId} className="-ml-1.5 first:ml-0">
                      <Avatar name={voter.displayName} imageUrl={voter.avatarUrl} size="sm" ringed />
                    </span>
                  ))}
                </span>

                {canManage && (
                  <span className="ml-auto flex flex-wrap gap-2">
                    {options.length > MIN_DATE_OPTIONS && (
                      <button
                        type="button"
                        onClick={() => removeOption.mutate(option.id)}
                        disabled={isManaging}
                        data-testid={`date-poll-remove-button-${option.id}`}
                        className={smallButton}
                      >
                        Supprimer
                      </button>
                    )}
                    <button
                      type="button"
                      onClick={() => handleChoose(option.id, label)}
                      disabled={isManaging}
                      data-testid={`date-poll-choose-button-${option.id}`}
                      className={smallButton}
                    >
                      Retenir cette date
                    </button>
                  </span>
                )}
              </div>
            </li>
          )
        })}
      </ul>

      {canManage && options.length < MAX_DATE_OPTIONS && (
        <form onSubmit={handleAddOption} data-testid="date-poll-add-form" className="flex flex-wrap items-center gap-2">
          <input
            type="date"
            required
            aria-label="Nouvelle date à proposer"
            value={newDate}
            onChange={(changeEvent) => setNewDate(changeEvent.target.value)}
            data-testid="date-poll-add-date-input"
            className={`${input} min-h-11 min-w-0 flex-1 basis-36`}
          />
          <input
            type="time"
            aria-label="Heure de la nouvelle date"
            value={newTime}
            onChange={(changeEvent) => setNewTime(changeEvent.target.value)}
            data-testid="date-poll-add-time-input"
            className={`${input} min-h-11 min-w-0 flex-1 basis-24`}
          />
          <button type="submit" disabled={isManaging} data-testid="date-poll-add-button" className={btnSecondary}>
            Ajouter
          </button>
        </form>
      )}

      {error && (
        <p data-testid="date-poll-error" className="ml-4 text-sm text-accent-800">
          {error.message}
        </p>
      )}
    </section>
  )
}
