# language: fr
Fonctionnalité: Marquage d'une tâche comme faite
  En tant que participant d'un événement je veux marquer une tâche comme faite
  afin de suivre l'avancement des préparatifs

  Scénario: Un participant marque sa propre tâche assignée comme faite
    Etant donné un événement "Repas de Noël" avec une tâche assignée "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je deviens le participant assigné à cette tâche
    Quand je marque cette tâche comme faite
    Alors le marquage réussit
    Et la tâche est marquée comme faite
    Et une notification temps réel de tâche marquée comme faite est diffusée

  Scénario: Le créateur marque une tâche non assignée comme faite
    Etant donné un événement "Repas de Noël" avec une tâche non assignée "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je marque cette tâche comme faite
    Alors le marquage réussit
    Et la tâche est marquée comme faite
    Et une notification temps réel de tâche marquée comme faite est diffusée

  Scénario: Un participant simple marque la tâche assignée à un autre participant comme faite
    Etant donné un événement "Repas de Noël" avec une tâche assignée "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et un autre participant a rejoint cet événement et devient l'utilisateur courant
    Quand je marque cette tâche comme faite
    Alors le marquage échoue avec une erreur de statut réservé à l'assigné

  Scénario: Marquage par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec une tâche non assignée "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je marque cette tâche comme faite
    Alors le marquage échoue avec une erreur d'autorisation

  Scénario: Marquage sur un événement inexistant
    Quand je marque une tâche comme faite sur un événement inexistant
    Alors le marquage échoue avec une erreur d'événement introuvable
