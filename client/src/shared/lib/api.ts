const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7166'

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
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    throw new ApiError(await extractErrorMessage(response), response.status)
  }

  if (response.status === 202 || response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}
