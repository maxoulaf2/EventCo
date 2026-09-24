import { useMutation, useQueryClient } from '@tanstack/react-query'
import { prepareAvatarImage } from '../../../shared/lib/image'
import { updateAvatar } from '../api'

export function useUpdateAvatar() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (file: File) => updateAvatar(await prepareAvatarImage(file)),
    onSuccess: (user) => {
      queryClient.setQueryData(['auth', 'me'], user)
      // La photo apparaît aussi dans les participants des événements déjà chargés.
      void queryClient.invalidateQueries({ queryKey: ['events'] })
    },
  })
}
