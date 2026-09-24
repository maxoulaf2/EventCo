export interface VerifyLoginCodeResult {
  userId: string
  email: string
  displayName: string
  eventId: string | null
}

export interface CurrentUser {
  userId: string
  email: string
  displayName: string
  isAdmin: boolean
}
