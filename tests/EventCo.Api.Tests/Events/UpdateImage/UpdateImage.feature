# language: fr
Fonctionnalité: Image de présentation d'un événement via l'API
  En tant que créateur d'un événement je veux envoyer une image de présentation via l'API
  afin que les invités reconnaissent l'événement d'un coup d'œil

  Scénario: Envoi d'une image PNG par le créateur
    Etant donné une session ouverte via l'API pour "update-event-image-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'envoie une image "PNG" comme image de présentation de cet événement via l'API
    Alors la réponse d'envoi d'image de présentation a le statut 200
    Et l'image de présentation retournée est téléchargeable avec le type "image/png"
    Et le détail de l'événement expose la même image de présentation

  Scénario: Envoi par un utilisateur qui n'est ni créateur ni co-organisateur
    Etant donné une session ouverte via l'API pour "update-event-image-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "update-event-image-other-user-api-test@example.com"
    Quand j'envoie une image "PNG" comme image de présentation de cet événement via l'API
    Alors la réponse d'envoi d'image de présentation a le statut 403

  Scénario: Envoi d'un fichier qui n'est pas une image
    Etant donné une session ouverte via l'API pour "update-event-image-invalid-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'envoie une image "texte" comme image de présentation de cet événement via l'API
    Alors la réponse d'envoi d'image de présentation a le statut 400

  Scénario: Envoi sans cookie de session
    Quand j'envoie une image "PNG" comme image de présentation d'un événement inexistant via l'API sans cookie de session
    Alors la réponse d'envoi d'image de présentation a le statut 401
