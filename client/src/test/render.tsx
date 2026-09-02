import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { AppRoutes } from '../App'

/**
 * Rendu de l'application pour les tests de comportement : mêmes routes que `App`,
 * mais avec un `MemoryRouter` (contrôle de l'URL de départ) et un `QueryClient`
 * dédié sans retry (échecs immédiats et déterministes face au back mocké).
 */
export function renderApp(initialPath = '/') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <AppRoutes />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}
