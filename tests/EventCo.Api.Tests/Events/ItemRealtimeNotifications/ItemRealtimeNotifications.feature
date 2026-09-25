# language: fr
Fonctionnalité: Diffusion temps réel des événements d'articles
  En tant que participant connecté au hub temps réel d'un événement je veux recevoir les mises à jour d'articles en direct
  afin de suivre les préparatifs sans recharger la page

  Scénario: La création d'un article est diffusée au groupe temps réel
    Etant donné une session ouverte via l'API pour "realtime-create-item-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les notifications temps réel d'articles
    Quand j'ajoute l'article à prendre "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors une notification temps réel de création d'article est reçue pour cet article

  Scénario: L'assignation d'un article est diffusée au groupe temps réel
    Etant donné une session ouverte via l'API pour "realtime-assign-item-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "realtime-assign-item-participant-api-test@example.com" invité à cet événement via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les notifications temps réel d'articles
    Quand j'assigne cet article à ce participant via l'API
    Alors une notification temps réel d'assignation d'article est reçue pour cet article

  Scénario: La suppression d'un article est diffusée au groupe temps réel
    Etant donné une session ouverte via l'API pour "realtime-delete-item-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les notifications temps réel d'articles
    Quand je supprime cet article via l'API
    Alors une notification temps réel de suppression d'article est reçue pour cet article
