import { useQuery } from '@tanstack/react-query'
import { getItemCategories } from '../api'

// Liste fixe côté backend : chargée une fois par session.
export function useItemCategories() {
  return useQuery({
    queryKey: ['item-categories'],
    queryFn: getItemCategories,
    staleTime: Infinity,
  })
}
