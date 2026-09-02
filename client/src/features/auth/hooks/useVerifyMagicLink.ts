import { useMutation } from '@tanstack/react-query'
import { verifyMagicLink } from '../api'

export function useVerifyMagicLink() {
  return useMutation({
    mutationFn: verifyMagicLink,
  })
}
