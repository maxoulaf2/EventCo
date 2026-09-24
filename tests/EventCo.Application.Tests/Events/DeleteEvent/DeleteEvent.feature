# language: fr
Fonctionnalité: Suppression d'un événement
  En tant que créateur d'un événement je veux pouvoir le supprimer
  afin de retirer un événement qui n'a plus lieu d'être

  Scénario: Suppression par le créateur
    Etant donné un événement à supprimer "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je supprime cet événement
    Alors la suppression réussit
    Et l'événement supprimé n'est plus consultable

  Scénario: Suppression d'un événement avec une image de présentation
    Etant donné un événement à supprimer "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et cet événement à supprimer a une image de présentation
    Quand je supprime cet événement
    Alors la suppression réussit
    Et l'image de présentation de l'événement supprimé a été retirée du stockage

  Scénario: Suppression par un utilisateur qui n'est pas le créateur
    Etant donné un événement à supprimer "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je supprime cet événement
    Alors la suppression échoue avec une erreur d'autorisation

  Scénario: Suppression d'un événement inexistant
    Quand je supprime un événement inexistant
    Alors la suppression échoue avec une erreur d'événement introuvable
