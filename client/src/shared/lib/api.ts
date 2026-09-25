import { APP_VERSION_HEADER, checkServerVersion } from './appVersion'

// Vide par défaut = appels relatifs à l'origine courante, proxifiés vers l'API par Vite en dev
// (cf. vite.config.ts) pour rester same-origin et éviter que le cookie de session soit traité comme
// cookie tiers (bloqué par défaut en navigation privée). VITE_API_URL reste utile en prod si le build
// statique est servi séparément de l'API.
export const API_BASE_URL = import.meta.env.VITE_API_URL ?? ''

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

async function extractErrorMessage(response: Response): Promise<string> {
  const problem = (await response.json().catch(() => null)) as ProblemDetails | null

  return (
    problem?.detail ??
    Object.values(problem?.errors ?? {})[0]?.[0] ??
    problem?.title ??
    'Une erreur est survenue.'
  )
}

export async function apiFetch<TResponse = undefined>(
  path: string,
  options: RequestInit = {},
): Promise<TResponse> {
  // Pour un FormData (upload de fichier), le navigateur doit poser lui-même le Content-Type
  // multipart/form-data, qui porte le délimiteur (boundary) des parties.
  const isFormData = options.body instanceof FormData

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...options.headers,
    },
  })

  checkServerVersion(response.headers.get(APP_VERSION_HEADER))

  if (!response.ok) {
    throw new ApiError(await extractErrorMessage(response), response.status)
  }

  if (response.status === 202 || response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}
