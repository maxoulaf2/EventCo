import { apiFetch } from '../../shared/lib/api'
import type { MyEvent } from './types'

export function getMyEvents(): Promise<MyEvent[]> {
  return apiFetch<MyEvent[]>('/api/events')
}
