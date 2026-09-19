import { apiFetch } from '../../shared/lib/api'
import type { CurrentUser, VerifyMagicLinkResult } from './types'

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

export function getCurrentUser(): Promise<CurrentUser> {
  return apiFetch<CurrentUser>('/api/auth/me')
}

export function logout(): Promise<void> {
  return apiFetch('/api/auth/logout', { method: 'POST' })
}

export function updateProfile(displayName: string): Promise<CurrentUser> {
  return apiFetch<CurrentUser>('/api/auth/me', {
    method: 'PUT',
    body: JSON.stringify({ displayName }),
  })
}
