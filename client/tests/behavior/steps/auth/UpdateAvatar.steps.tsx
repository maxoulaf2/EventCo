import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../../../src/test/mocks/server'
import { renderApp } from '../../../../src/test/render'

const feature = await loadFeature('tests/behavior/features/auth/UpdateAvatar.feature', { language: 'fr' })

const uploadedAvatarUrl = '/uploads/avatars/user-1/avatar-1.jpg'

async function openAccountModal() {
  const user = userEvent.setup()
  await user.click(await screen.findByTestId('events-dashboard-account-button'))
}

async function chooseAvatarImage() {
  const user = userEvent.setup()
  const image = new File([new Uint8Array([0x89, 0x50, 0x4e, 0x47])], 'photo.png', { type: 'image/png' })
  await user.upload(screen.getByTestId('account-modal-avatar-input'), image)
}

function expectPreviewShowsInitial() {
  const preview = screen.getByTestId('account-modal-avatar-preview')
  expect(preview.tagName).toBe('SPAN')
  expect(preview).toHaveTextContent('T')
}

describeFeature(feature, ({ AfterEachScenario, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
    cleanup()
  })

  Scenario('Ajout d\'une photo de profil', ({ When, And, Then }) => {
    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', openAccountModal)

    Then('l\'aperçu de ma photo de profil affiche mon initiale', expectPreviewShowsInitial)

    When('je choisis une image comme photo de profil', chooseAvatarImage)

    Then('l\'aperçu de ma photo de profil affiche la photo envoyée', async () => {
      await waitFor(() =>
        expect(screen.getByTestId('account-modal-avatar-preview')).toHaveAttribute('src', uploadedAvatarUrl),
      )
    })

    And('le bouton de mon compte affiche la photo envoyée', () => {
      expect(screen.getByTestId('events-dashboard-account-avatar-image')).toHaveAttribute('src', uploadedAvatarUrl)
    })

    And('je peux retirer ma photo de profil', () => {
      expect(screen.getByTestId('account-modal-avatar-remove-button')).toBeInTheDocument()
    })
  })

  Scenario('Retrait de la photo de profil', ({ Given, When, And, Then }) => {
    Given('j\'ai déjà une photo de profil', () => {
      server.use(
        http.get('*/api/auth/me', () =>
          HttpResponse.json({
            userId: 'user-1',
            email: 'test@example.com',
            displayName: 'Test',
            isAdmin: false,
            avatarUrl: uploadedAvatarUrl,
          }),
        ),
      )
    })

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', openAccountModal)

    And('je retire ma photo de profil', async () => {
      const user = userEvent.setup()
      await user.click(screen.getByTestId('account-modal-avatar-remove-button'))
    })

    Then('l\'aperçu de ma photo de profil affiche mon initiale', async () => {
      await waitFor(expectPreviewShowsInitial)
    })

    And('je ne peux plus retirer ma photo de profil', () => {
      expect(screen.queryByTestId('account-modal-avatar-remove-button')).not.toBeInTheDocument()
    })
  })

  Scenario('Le serveur refuse l\'image', ({ Given, When, And, Then }) => {
    Given('le serveur refuse l\'image envoyée', () => {
      server.use(
        http.put('*/api/auth/me/avatar', () =>
          HttpResponse.json(
            { errors: { Content: ['Format d\'image non supporté (JPEG, PNG ou WebP attendu).'] } },
            { status: 400 },
          ),
        ),
      )
    })

    When('j\'arrive sur le tableau de bord', () => {
      renderApp('/events')
    })

    And('j\'ouvre la modale de mon compte', openAccountModal)

    And('je choisis une image comme photo de profil', chooseAvatarImage)

    Then('je vois un message d\'erreur sur la photo de profil', async () => {
      const errorMessage = await screen.findByTestId('account-modal-avatar-error')
      expect(errorMessage).toHaveTextContent('Format d\'image non supporté (JPEG, PNG ou WebP attendu).')
    })
  })
})
