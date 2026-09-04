import { HttpResponse, http } from 'msw'

/**
 * Comportement par défaut du back mocké : chemin nominal pour chaque endpoint.
 * Les scénarios d'erreur surchargent un handler ponctuellement via `server.use(...)`.
 */
export const handlers = [
  http.post('*/api/auth/request-link', () => new HttpResponse(null, { status: 202 })),

  http.post('*/api/auth/verify', () =>
    HttpResponse.json({
      userId: 'user-1',
      email: 'test@example.com',
      displayName: 'Test',
    }),
  ),

  http.get('*/api/events', () =>
    HttpResponse.json([
      {
        id: 'event-1',
        title: 'Repas de Noël',
        eventDate: '2026-12-24T00:00:00Z',
        location: 'Chez Alice',
        createdByUserId: 'user-1',
        status: 'Planned',
        role: 'Organizer',
        hasJoined: true,
      },
      {
        id: 'event-2',
        title: 'Weekend au ski',
        eventDate: '2027-01-10T00:00:00Z',
        location: null,
        createdByUserId: 'user-2',
        status: 'Planned',
        role: 'Participant',
        hasJoined: false,
      },
    ]),
  ),
]
