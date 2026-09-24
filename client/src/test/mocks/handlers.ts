import { HttpResponse, http } from 'msw'

/**
 * Comportement par défaut du back mocké : chemin nominal pour chaque endpoint.
 * Les scénarios d'erreur surchargent un handler ponctuellement via `server.use(...)`.
 */
export const handlers = [
  http.post('*/api/auth/request-code', () => new HttpResponse(null, { status: 202 })),

  http.post('*/api/auth/verify', () =>
    HttpResponse.json({
      userId: 'user-1',
      email: 'test@example.com',
      displayName: 'Test',
      eventId: null,
    }),
  ),

  http.get('*/api/auth/me', () =>
    HttpResponse.json({
      userId: 'user-1',
      email: 'test@example.com',
      displayName: 'Test',
      isAdmin: false,
    }),
  ),

  http.put('*/api/auth/me', async ({ request }) => {
    const body = (await request.json()) as { displayName: string }
    return HttpResponse.json({
      userId: 'user-1',
      email: 'test@example.com',
      displayName: body.displayName,
      isAdmin: false,
    })
  }),

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
      },
      {
        id: 'event-2',
        title: 'Weekend au ski',
        eventDate: '2027-01-10T00:00:00Z',
        location: null,
        createdByUserId: 'user-2',
        status: 'Planned',
        role: 'Participant',
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
          participationStatus: 'Attending',
        },
        {
          userId: 'user-2',
          email: 'ami@example.com',
          displayName: 'Ami',
          role: 'Participant',
          invitedAt: '2026-09-02T00:00:00Z',
          participationStatus: 'Unknown',
        },
      ],
      inviteLinkToken: 'invite-token-1',
    }),
  ),

  http.get('*/api/events/:id/items', () => HttpResponse.json([])),

  http.post('*/api/events/:id/invite-link/regenerate', ({ params }) =>
    HttpResponse.json({ eventId: params.id, inviteLinkToken: 'invite-token-2' }),
  ),

  http.get('*/api/events/invite-links/:token/preview', () =>
    HttpResponse.json({
      eventId: 'event-1',
      title: 'Repas de Noël',
      eventDate: '2026-12-24T00:00:00Z',
      location: 'Chez Alice',
      createdByDisplayName: 'Test',
    }),
  ),

  http.post('*/api/events/invite-links/:token/join', () => HttpResponse.json({ eventId: 'event-1' })),
]
