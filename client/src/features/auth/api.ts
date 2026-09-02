import { apiFetch } from '../../shared/lib/api'
import type { VerifyMagicLinkResult } from './types'

export function requestMagicLink(email: string): Promise<void> {
  return apiFetch('/api/auth/request-link', {
    method: 'POST',
    body: JSON.stringify({ email }),
  })
}

export function verifyMagicLink(token: string): Promise<VerifyMagicLinkResult> {
  return apiFetch<VerifyMagicLinkResult>('/api/auth/verify', {
    method: 'POST',
    body: JSON.stringify({ token }),
  })
}
