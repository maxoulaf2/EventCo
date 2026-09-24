# language: fr
Fonctionnalité: Photo de profil
  En tant qu'utilisateur connecté je veux choisir ou retirer ma photo de profil depuis la modale
  de mon compte afin que les autres participants me reconnaissent plus facilement

  Scénario: Ajout d'une photo de profil
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Alors l'aperçu de ma photo de profil affiche mon initiale
    Quand je choisis une image comme photo de profil
    Alors l'aperçu de ma photo de profil affiche la photo envoyée
    Et le bouton de mon compte affiche la photo envoyée
    Et je peux retirer ma photo de profil

  Scénario: Retrait de la photo de profil
    Etant donné que j'ai déjà une photo de profil
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Et je retire ma photo de profil
    Alors l'aperçu de ma photo de profil affiche mon initiale
    Et je ne peux plus retirer ma photo de profil

  Scénario: Le serveur refuse l'image
    Etant donné que le serveur refuse l'image envoyée
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Et je choisis une image comme photo de profil
    Alors je vois un message d'erreur sur la photo de profil
