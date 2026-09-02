import { useMutation } from '@tanstack/react-query'
import { requestMagicLink } from '../api'

export function useRequestMagicLink() {
  return useMutation({
    mutationFn: requestMagicLink,
  })
}
