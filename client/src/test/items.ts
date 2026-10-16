import { screen, waitFor } from '@testing-library/react'
import { expect } from 'vitest'

// Depuis le regroupement par catégorie, le statut d'un article se lit sur sa ligne :
// un article assigné affiche la personne qui le prend, un article à prendre non.
export async function expectItemToTake(itemId: string) {
  await waitFor(() => {
    expect(screen.getByTestId(`item-row-${itemId}`)).toBeInTheDocument()
    expect(screen.queryByTestId(`item-row-assignee-${itemId}`)).not.toBeInTheDocument()
  })
}

export async function expectItemAssigned(itemId: string) {
  await waitFor(() => expect(screen.getByTestId(`item-row-assignee-${itemId}`)).toBeInTheDocument())
}
