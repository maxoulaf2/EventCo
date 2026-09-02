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
]
