# language: fr
Fonctionnalité: Validation du lien de connexion (magic link)
  En tant qu'utilisateur je veux valider mon lien de connexion
  afin d'ouvrir une session sur EventCo

  Scénario: Première connexion — le compte est créé automatiquement
    Quand un lien de connexion est demandé pour "nouvel-utilisateur@example.com"
    Et je valide le lien de connexion reçu
    Alors la validation réussit
    Et un compte est créé pour "nouvel-utilisateur@example.com"
    Et une session est ouverte pour "nouvel-utilisateur@example.com"
    Et le lien de connexion pour "nouvel-utilisateur@example.com" est marqué comme utilisé

  Scénario: Connexion suivante — le compte existant est réutilisé
    Quand un lien de connexion est demandé pour "utilisateur-existant@example.com"
    Et je valide le lien de connexion reçu
    Et un nouveau lien de connexion est demandé pour "utilisateur-existant@example.com"
    Et je valide le lien de connexion reçu
    Alors la validation réussit
    Et un seul compte existe pour "utilisateur-existant@example.com"

  Scénario: Token inexistant
    Quand je valide le token "token-qui-n-existe-pas"
    Alors la validation échoue avec une erreur de token invalide

  Scénario: Token déjà utilisé
    Quand un lien de connexion est demandé pour "deja-utilise@example.com"
    Et je valide le lien de connexion reçu
    Et je valide à nouveau le même lien de connexion
    Alors la validation échoue avec une erreur de token déjà utilisé

  Scénario: Token expiré
    Quand un lien de connexion est demandé pour "expire@example.com"
    Et le temps avance de 20 minutes
    Et je valide le lien de connexion reçu
    Alors la validation échoue avec une erreur d'expiration
