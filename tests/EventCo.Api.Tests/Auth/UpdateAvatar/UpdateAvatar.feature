# language: fr
Fonctionnalité: Modification de la photo de profil via l'API
  En tant qu'utilisateur connecté je veux envoyer une photo de profil via l'API
  afin que les autres participants me reconnaissent plus facilement

  Scénario: Envoi d'une image PNG avec une session valide
    Etant donné une session ouverte via l'API pour "update-avatar-api-test@example.com"
    Quand j'envoie une image "PNG" comme photo de profil via l'API
    Alors la réponse d'envoi de photo de profil a le statut 200
    Et la photo de profil retournée est téléchargeable avec le type "image/png"
    Et l'utilisateur courant expose la même photo de profil

  Scénario: Envoi d'un fichier qui n'est pas une image
    Etant donné une session ouverte via l'API pour "update-avatar-invalid-api-test@example.com"
    Quand j'envoie une image "texte" comme photo de profil via l'API
    Alors la réponse d'envoi de photo de profil a le statut 400

  Scénario: Envoi sans cookie de session
    Quand j'envoie une image "PNG" comme photo de profil via l'API sans cookie de session
    Alors la réponse d'envoi de photo de profil a le statut 401
