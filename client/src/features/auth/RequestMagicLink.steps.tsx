import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { expect } from 'vitest'
import { server } from '../../test/mocks/server'
import { renderApp } from '../../test/render'

const feature = await loadFeature('./RequestMagicLink.feature', { language: 'fr' })

describeFeature(feature, ({ AfterEachScenario, Background, Scenario }) => {
  AfterEachScenario(() => {
    server.resetHandlers()
  })

  Background(({ Given }) => {
    Given('je suis sur la page de connexion', () => {
      renderApp('/')
    })
  })

  Scenario('Email valide', ({ When, Then, And }) => {
    When('je saisis l\'email "test@example.com" et je valide le formulaire', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByLabelText('Adresse email'), 'test@example.com')
      await user.click(screen.getByRole('button', { name: /recevoir un lien/i }))
    })

    Then('je suis redirigé vers la page de confirmation', async () => {
      await screen.findByRole('heading', { name: /vérifiez votre boîte mail/i })
    })

    And('je vois l\'email "test@example.com" affiché', () => {
      expect(screen.getByText('test@example.com')).toBeInTheDocument()
    })
  })

  Scenario('Le serveur refuse la demande', ({ And, When, Then }) => {
    And('le serveur refusera la prochaine demande de lien', () => {
      server.use(
        http.post('*/api/auth/request-link', () =>
          HttpResponse.json({ detail: 'Adresse email invalide.' }, { status: 400 }),
        ),
      )
    })

    When('je saisis l\'email "test@example.com" et je valide le formulaire', async () => {
      const user = userEvent.setup()
      await user.type(screen.getByLabelText('Adresse email'), 'test@example.com')
      await user.click(screen.getByRole('button', { name: /recevoir un lien/i }))
    })

    Then('je vois un message d\'erreur sur le formulaire', async () => {
      await screen.findByText('Adresse email invalide.')
    })

    And('je reste sur la page de connexion', () => {
      expect(screen.getByRole('button', { name: /recevoir un lien/i })).toBeInTheDocument()
    })
  })
})
