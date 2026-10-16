import { screen, waitFor, within } from '@testing-library/react'
import { expect } from 'vitest'

// Un article à prendre est mis en valeur dans la section « À prendre », en tête de liste ; un article pris
// rejoint sa catégorie et affiche la personne qui le prend.
export async function expectItemToTake(itemId: string) {
  await waitFor(() =>
    expect(within(screen.getByTestId('item-list-to-take')).getByTestId(`item-row-${itemId}`)).toBeInTheDocument(),
  )
}

export async function expectItemAssigned(itemId: string) {
  await waitFor(() => {
    expect(screen.getByTestId(`item-row-assignee-${itemId}`)).toBeInTheDocument()
    expect(screen.queryByTestId('item-list-to-take')?.querySelector(`[data-testid="item-row-${itemId}"]`)).toBeFalsy()
  })
}
