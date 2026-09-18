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

  http.get('*/api/auth/me', () =>
    HttpResponse.json({
      userId: 'user-1',
      email: 'test@example.com',
      displayName: 'Test',
    }),
  ),

  http.post('*/api/auth/logout', () => new HttpResponse(null, { status: 204 })),

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

  http.post('*/api/events', () =>
    HttpResponse.json(
      {
        id: 'event-new',
        title: 'Nouvel événement',
        description: null,
        eventDate: '2026-12-24T00:00:00Z',
        location: null,
        createdByUserId: 'user-1',
        status: 'Draft',
        createdAt: '2026-09-08T00:00:00Z',
      },
      { status: 201 },
    ),
  ),

  http.get('*/api/events/:id', ({ params }) =>
    HttpResponse.json({
      id: params.id,
      title: 'Repas de Noël',
      description: 'Un bon repas de fêtes entre amis.',
      eventDate: '2026-12-24T00:00:00Z',
      location: 'Chez Alice',
      createdByUserId: 'user-1',
      status: 'Planned',
      createdAt: '2026-09-01T00:00:00Z',
      participants: [
        {
          userId: 'user-1',
          email: 'test@example.com',
          displayName: 'Test',
          role: 'Organizer',
          invitedAt: '2026-09-01T00:00:00Z',
          hasJoined: true,
        },
        {
          userId: 'user-2',
          email: 'ami@example.com',
          displayName: 'Ami',
          role: 'Participant',
          invitedAt: '2026-09-02T00:00:00Z',
          hasJoined: false,
        },
      ],
    }),
  ),

  http.get('*/api/events/:id/tasks', () => HttpResponse.json([])),
]
