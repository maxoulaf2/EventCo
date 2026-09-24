# language: fr
Fonctionnalité: Validation du code de connexion
  En tant qu'utilisateur je veux saisir le code de connexion reçu par email
  afin d'ouvrir une session sur EventCo

  Scénario: Première connexion — le compte est créé automatiquement
    Quand un code de connexion est demandé pour "nouvel-utilisateur@example.com"
    Et je saisis le code de connexion reçu
    Alors la validation réussit
    Et un compte est créé pour "nouvel-utilisateur@example.com"
    Et une session est ouverte pour "nouvel-utilisateur@example.com"
    Et le code de connexion pour "nouvel-utilisateur@example.com" est marqué comme utilisé

  Scénario: Connexion suivante — le compte existant est réutilisé
    Quand un code de connexion est demandé pour "utilisateur-existant@example.com"
    Et je saisis le code de connexion reçu
    Et un nouveau code de connexion est demandé pour "utilisateur-existant@example.com"
    Et je saisis le code de connexion reçu
    Alors la validation réussit
    Et un seul compte existe pour "utilisateur-existant@example.com"

  Scénario: Code erroné
    Quand un code de connexion est demandé pour "code-errone@example.com"
    Et je saisis un code erroné
    Alors la validation échoue avec un code invalide
    Et 1 essai erroné est comptabilisé sur le code de connexion pour "code-errone@example.com"

  Scénario: Aucun code demandé pour cet email
    Quand je saisis le code "123456" pour "jamais-demande@example.com"
    Alors la validation échoue avec un code invalide

  Scénario: Code reçu par un autre email
    Quand un code de connexion est demandé pour "destinataire@example.com"
    Et je saisis le code de connexion reçu pour l'email "autre@example.com"
    Alors la validation échoue avec un code invalide

  Scénario: Code déjà utilisé
    Quand un code de connexion est demandé pour "deja-utilise@example.com"
    Et je saisis le code de connexion reçu
    Et je saisis à nouveau le même code de connexion
    Alors la validation échoue avec un code invalide

  Scénario: Code expiré
    Quand un code de connexion est demandé pour "expire@example.com"
    Et le temps avance de 20 minutes
    Et je saisis le code de connexion reçu
    Alors la validation échoue avec un code invalide

  Scénario: Code bloqué après trop d'essais erronés
    Quand un code de connexion est demandé pour "force-brute@example.com"
    Et je saisis 5 fois un code erroné
    Et je saisis le code de connexion reçu
    Alors la validation échoue avec un code invalide

  Scénario: Un code redemandé n'invalide pas le précédent
    Quand un code de connexion est demandé pour "deux-codes@example.com"
    Et un nouveau code de connexion est demandé pour "deux-codes@example.com"
    Et je saisis le premier code de connexion reçu
    Alors la validation réussit

  Scénario: Connexion via un code associé à une intention de rejoindre un événement
    Etant donné un événement "Réveillon" avec un lien d'invitation actif
    Quand un code de connexion avec intention de rejoindre cet événement est demandé pour "invite-code@example.com"
    Et je saisis le code de connexion reçu
    Alors la validation réussit
    Et je rejoins l'événement "Réveillon"

  Scénario: Connexion via un code dont l'événement associé a été régénéré entre-temps
    Etant donné un événement "Réveillon" avec un lien d'invitation actif
    Et un code de connexion avec intention de rejoindre cet événement est demandé pour "invite-perime@example.com"
    Et le lien d'invitation de cet événement est régénéré
    Quand je saisis le code de connexion reçu
    Alors la validation réussit
    Et je ne rejoins aucun événement
