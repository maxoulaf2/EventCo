import { useMutation, useQueryClient } from '@tanstack/react-query'
import { removeAvatar } from '../api'

export function useRemoveAvatar() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: removeAvatar,
    onSuccess: (user) => {
      queryClient.setQueryData(['auth', 'me'], user)
      void queryClient.invalidateQueries({ queryKey: ['events'] })
    },
  })
}
