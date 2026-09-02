import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { CheckEmailPage } from './features/auth/components/CheckEmailPage'
import { LoginPage } from './features/auth/components/LoginPage'
import { VerifyMagicLinkPage } from './features/auth/components/VerifyMagicLinkPage'
import { routes } from './shared/lib/routes'

const queryClient = new QueryClient()

/** Arbre de routes seul, sans provider ni router — réutilisé tel quel par les tests. */
export function AppRoutes() {
  return (
    <Routes>
      <Route path={routes.login} element={<LoginPage />} />
      <Route path={routes.checkEmail} element={<CheckEmailPage />} />
      <Route path={routes.verifyMagicLink} element={<VerifyMagicLinkPage />} />
    </Routes>
  )
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </QueryClientProvider>
  )
}

export default App
