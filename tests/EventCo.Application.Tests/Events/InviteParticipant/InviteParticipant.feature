# language: fr
Fonctionnalité: Invitation d'un participant par email
  En tant qu'utilisateur connecté je veux inviter quelqu'un par email à un événement
  afin qu'il puisse y participer

  Scénario: Invitation d'une personne qui n'a pas encore de compte
    Etant donné un événement ouvert aux invitations "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "amie@example.com" à cet événement
    Alors l'invitation réussit
    Et la personne invitée a le rôle "Participant"
    Et la personne invitée n'a pas encore indiqué de statut de participation
    Et un compte est créé pour la personne invitée "amie@example.com"
    Et un email d'invitation est envoyé à "amie@example.com"

  Scénario: Invitation d'une personne ayant déjà un compte
    Etant donné un événement ouvert aux invitations "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et un compte existe déjà pour "ami@example.com"
    Quand j'invite "ami@example.com" à cet événement
    Alors l'invitation réussit
    Et un seul compte existe pour la personne invitée "ami@example.com"

  Scénario: Invitation d'une personne déjà invitée
    Etant donné un événement ouvert aux invitations "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" est déjà invité à cet événement
    Quand j'invite "ami@example.com" à cet événement
    Alors l'invitation échoue avec une erreur de participant déjà invité

  Scénario: Invitation avec un email invalide
    Etant donné un événement ouvert aux invitations "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "pas-un-email" à cet événement
    Alors l'invitation échoue avec une erreur de validation

  Scénario: Invitation sur un événement inexistant
    Quand j'invite "ami@example.com" à un événement inexistant
    Alors l'invitation échoue avec une erreur d'événement introuvable

  Scénario: Invitation par un utilisateur qui n'est ni créateur ni co-organisateur
    Etant donné un événement ouvert aux invitations "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand j'invite "amie@example.com" à cet événement
    Alors l'invitation échoue avec une erreur d'autorisation

  Scénario: Trop d'invitations envoyées à la même personne, tous événements confondus
    Etant donné un événement ouvert aux invitations "Repas 1" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "spam@example.com" à cet événement
    Etant donné un événement ouvert aux invitations "Repas 2" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "spam@example.com" à cet événement
    Etant donné un événement ouvert aux invitations "Repas 3" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "spam@example.com" à cet événement
    Etant donné un événement ouvert aux invitations "Repas 4" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "spam@example.com" à cet événement
    Etant donné un événement ouvert aux invitations "Repas 5" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "spam@example.com" à cet événement
    Etant donné un événement ouvert aux invitations "Repas 6" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'invite "spam@example.com" à cet événement
    Alors l'invitation échoue avec une erreur de trop de requêtes
    Et exactement 5 emails d'invitation ont été envoyés à "spam@example.com"
