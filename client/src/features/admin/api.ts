import { apiFetch } from '../../shared/lib/api'
import type { AdminEventSummary } from './types'

export function getAllEvents(): Promise<AdminEventSummary[]> {
  return apiFetch<AdminEventSummary[]>('/api/admin/events')
}
