export interface VerifyMagicLinkResult {
  userId: string
  email: string
  displayName: string
  eventId: string | null
}

export interface CurrentUser {
  userId: string
  email: string
  displayName: string
}
