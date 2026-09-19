import { useMutation } from '@tanstack/react-query'
import { joinEventViaInviteLink } from '../api'

export function useJoinEventViaInviteLink(token: string) {
  return useMutation({
    mutationFn: () => joinEventViaInviteLink(token),
  })
}
