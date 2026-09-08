# language: fr
Fonctionnalité: Marquage d'une tâche comme non faite
  En tant que participant d'un événement je veux rouvrir une tâche déjà marquée comme faite
  afin de corriger une erreur ou de refaire ce qui reste à faire

  Scénario: Un participant rouvre sa propre tâche assignée
    Etant donné un événement "Repas de Noël" avec une tâche assignée déjà marquée comme faite "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je suis le participant auquel cette tâche est assignée
    Quand je marque cette tâche comme non faite
    Alors la réouverture réussit
    Et la tâche est marquée comme non faite

  Scénario: Le créateur rouvre une tâche non assignée
    Etant donné un événement "Repas de Noël" avec une tâche non assignée déjà marquée comme faite "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je marque cette tâche comme non faite
    Alors la réouverture réussit
    Et la tâche est marquée comme non faite

  Scénario: Un participant simple rouvre la tâche assignée à un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche assignée déjà marquée comme faite "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et un second participant a également rejoint cet événement et en devient l'utilisateur courant
    Quand je marque cette tâche comme non faite
    Alors la réouverture échoue avec une erreur de statut réservé à l'assigné

  Scénario: Réouverture par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec une tâche non assignée déjà marquée comme faite "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je marque cette tâche comme non faite
    Alors la réouverture échoue avec une erreur d'autorisation

  Scénario: Réouverture sur un événement inexistant
    Quand je marque une tâche comme non faite sur un événement inexistant
    Alors la réouverture échoue avec une erreur d'événement introuvable
