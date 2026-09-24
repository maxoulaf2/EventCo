# language: fr
Fonctionnalité: Image de présentation d'un événement
  En tant que créateur ou co-organisateur d'un événement je veux envoyer une image de présentation
  afin que les invités reconnaissent l'événement d'un coup d'œil

  Scénario: Envoi d'une image PNG par le créateur
    Etant donné un événement sans image "Repas de Noël"
    Quand j'envoie une image "PNG" comme image de présentation de cet événement
    Alors l'envoi de l'image de présentation réussit
    Et l'image de présentation est accessible à une URL publique du bucket des images d'événement
    Et le bucket des images d'événement contient 1 fichier de type "image/png"
    Et l'événement référence l'image envoyée

  Scénario: Remplacement d'une image existante
    Etant donné un événement sans image "Repas de Noël"
    Et j'ai déjà envoyé une image "PNG" comme image de présentation de cet événement
    Quand j'envoie une image "JPEG" comme image de présentation de cet événement
    Alors l'envoi de l'image de présentation réussit
    Et l'image de présentation a changé d'URL
    Et le bucket des images d'événement contient 1 fichier de type "image/jpeg"

  Scénario: Envoi par un utilisateur qui n'est ni créateur ni co-organisateur
    Etant donné un événement sans image "Repas de Noël"
    Et je change d'utilisateur courant
    Quand j'envoie une image "PNG" comme image de présentation de cet événement
    Alors l'envoi de l'image de présentation échoue avec une erreur d'autorisation
    Et le bucket des images d'événement ne contient aucun fichier

  Scénario: Envoi pour un événement inexistant
    Quand j'envoie une image "PNG" comme image de présentation d'un événement inexistant
    Alors l'envoi de l'image de présentation échoue avec une erreur d'événement introuvable

  Scénario: Envoi d'un fichier qui n'est pas une image
    Etant donné un événement sans image "Repas de Noël"
    Quand j'envoie une image "texte" comme image de présentation de cet événement
    Alors l'envoi de l'image de présentation échoue avec une erreur de validation
    Et le bucket des images d'événement ne contient aucun fichier

  Scénario: Envoi d'une image trop lourde
    Etant donné un événement sans image "Repas de Noël"
    Quand j'envoie une image PNG de 6 Mo comme image de présentation de cet événement
    Alors l'envoi de l'image de présentation échoue avec une erreur de validation
    Et le bucket des images d'événement ne contient aucun fichier
