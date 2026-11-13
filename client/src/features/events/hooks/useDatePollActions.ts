import { useMutation, useQueryClient } from '@tanstack/react-query'
import { addDateOption, chooseEventDate, removeDateOption, voteForDateOption, withdrawDateVote } from '../api'
import type { DateOptionInput } from '../types'

// Les actions du sondage de date d'un événement. Chacune recharge les événements (détail et tableau de
// bord) plutôt que de modifier le cache : à la clôture, la date, les options et les présences changent ensemble.
export function useDatePollActions(eventId: string) {
  const queryClient = useQueryClient()
  const onSuccess = () => queryClient.invalidateQueries({ queryKey: ['events'] })

  return {
    vote: useMutation({ mutationFn: (dateOptionId: string) => voteForDateOption(eventId, dateOptionId), onSuccess }),
    withdrawVote: useMutation({ mutationFn: (dateOptionId: string) => withdrawDateVote(eventId, dateOptionId), onSuccess }),
    addOption: useMutation({ mutationFn: (option: DateOptionInput) => addDateOption(eventId, option), onSuccess }),
    removeOption: useMutation({ mutationFn: (dateOptionId: string) => removeDateOption(eventId, dateOptionId), onSuccess }),
    chooseDate: useMutation({ mutationFn: (dateOptionId: string) => chooseEventDate(eventId, dateOptionId), onSuccess }),
  }
}
