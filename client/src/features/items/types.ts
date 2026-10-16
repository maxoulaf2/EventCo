// « À prendre » : demande d'un (co-)organisateur, que n'importe quel participant peut prendre.
// « Contribution » : ce qu'un participant déclare apporter, attribué à lui définitivement (annulable seulement).
export type EventItemKind = 'ToBring' | 'Contribution'

export interface CreateItemInput {
  title: string
  quantity?: string
  kind: EventItemKind
}

export interface EventItem {
  id: string
  eventId: string
  title: string
  quantity: string | null
  kind: EventItemKind
  // Clé d'une `ItemCategory` : déduite de l'intitulé côté backend, sauf si elle a été corrigée (cf. CONTEXT.md).
  category: string
  assignedToUserId: string | null
  createdAt: string
}

// Liste, libellés et ordre des catégories fournis par l'API (`GET /api/item-categories`) : le frontend n'en
// garde aucune copie.
export interface ItemCategory {
  key: string
  label: string
}
