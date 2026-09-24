import { useMutation } from '@tanstack/react-query'
import { requestLoginCode } from '../api'

export function useRequestLoginCode() {
  return useMutation({
    mutationFn: ({ email, eventInviteLinkToken }: { email: string; eventInviteLinkToken?: string }) =>
      requestLoginCode(email, eventInviteLinkToken),
  })
}
