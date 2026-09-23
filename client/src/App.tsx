import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AdminEventsPage } from './features/admin/components/AdminEventsPage'
import { CheckEmailPage } from './features/auth/components/CheckEmailPage'
import { LoginPage } from './features/auth/components/LoginPage'
import { VerifyMagicLinkPage } from './features/auth/components/VerifyMagicLinkPage'
import { CreateEventPage } from './features/events/components/CreateEventPage'
import { EventDetailPage } from './features/events/components/EventDetailPage'
import { EventsDashboardPage } from './features/events/components/EventsDashboardPage'
import { InviteLinkPage } from './features/events/components/InviteLinkPage'
import { routes } from './shared/lib/routes'

const queryClient = new QueryClient()

/** Arbre de routes seul, sans provider ni router — réutilisé tel quel par les tests. */
export function AppRoutes() {
  return (
    <Routes>
      <Route path={routes.login} element={<LoginPage />} />
      <Route path={routes.checkEmail} element={<CheckEmailPage />} />
      <Route path={routes.verifyMagicLink} element={<VerifyMagicLinkPage />} />
      <Route path={routes.events} element={<EventsDashboardPage />} />
      <Route path={routes.createEvent} element={<CreateEventPage />} />
      <Route path="/events/:eventId" element={<EventDetailPage />} />
      <Route path="/invite/:token" element={<InviteLinkPage />} />
      <Route path={routes.adminEvents} element={<AdminEventsPage />} />
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
