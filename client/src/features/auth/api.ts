import { apiFetch } from '../../shared/lib/api'
import type { CurrentUser, VerifyLoginCodeResult } from './types'

export function requestLoginCode(email: string, eventInviteLinkToken?: string): Promise<void> {
  return apiFetch('/api/auth/request-code', {
    method: 'POST',
    body: JSON.stringify({ email, eventInviteLinkToken }),
  })
}

export function verifyLoginCode(email: string, code: string): Promise<VerifyLoginCodeResult> {
  return apiFetch<VerifyLoginCodeResult>('/api/auth/verify', {
    method: 'POST',
    body: JSON.stringify({ email, code }),
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
