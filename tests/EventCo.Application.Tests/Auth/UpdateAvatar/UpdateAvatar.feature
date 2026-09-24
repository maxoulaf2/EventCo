# language: fr
Fonctionnalité: Modification de la photo de profil
  En tant qu'utilisateur connecté je veux choisir une photo de profil
  afin que les autres participants me reconnaissent plus facilement

  Scénario: Envoi d'une image PNG
    Etant donné un compte sans photo de profil pour "alice@example.com"
    Quand j'envoie une image "PNG" comme photo de profil
    Alors l'envoi de la photo de profil réussit
    Et ma photo de profil est accessible à une URL publique
    Et le stockage contient 1 fichier de type "image/png"

  Scénario: Envoi d'une image WebP
    Etant donné un compte sans photo de profil pour "alice@example.com"
    Quand j'envoie une image "WebP" comme photo de profil
    Alors l'envoi de la photo de profil réussit
    Et le stockage contient 1 fichier de type "image/webp"

  Scénario: Remplacement d'une photo existante
    Etant donné un compte sans photo de profil pour "alice@example.com"
    Et j'ai déjà envoyé une image "PNG" comme photo de profil
    Quand j'envoie une image "JPEG" comme photo de profil
    Alors l'envoi de la photo de profil réussit
    Et ma photo de profil a changé d'URL
    Et le stockage contient 1 fichier de type "image/jpeg"

  Scénario: Envoi d'un fichier qui n'est pas une image
    Etant donné un compte sans photo de profil pour "alice@example.com"
    Quand j'envoie une image "texte" comme photo de profil
    Alors l'envoi de la photo de profil échoue avec une erreur de validation
    Et le stockage ne contient aucun fichier

  Scénario: Envoi d'un fichier vide
    Etant donné un compte sans photo de profil pour "alice@example.com"
    Quand j'envoie une image "vide" comme photo de profil
    Alors l'envoi de la photo de profil échoue avec une erreur de validation

  Scénario: Envoi d'une image trop lourde
    Etant donné un compte sans photo de profil pour "alice@example.com"
    Quand j'envoie une image PNG de 3 Mo comme photo de profil
    Alors l'envoi de la photo de profil échoue avec une erreur de validation
    Et le stockage ne contient aucun fichier
