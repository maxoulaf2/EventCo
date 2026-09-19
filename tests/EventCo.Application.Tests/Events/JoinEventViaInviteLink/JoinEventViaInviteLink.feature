# language: fr
Fonctionnalité: Rejoindre un événement via un lien d'invitation
  En tant qu'utilisateur je veux rejoindre un événement en cliquant sur son lien d'invitation
  afin de devenir participant sans passer par une invitation par email

  Scénario: Un utilisateur rejoint un événement avec un lien valide
    Etant donné un événement à rejoindre "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je rejoins l'événement via son lien d'invitation
    Alors je rejoins l'événement avec succès
    Et j'ai le rôle "Participant" dans cet événement

  Scénario: Rejoindre avec un lien d'invitation inexistant
    Quand je rejoins un événement via un lien d'invitation inexistant
    Alors la tentative échoue avec une erreur de lien introuvable

  Scénario: Le créateur clique sur son propre lien
    Etant donné un événement à rejoindre "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je rejoins l'événement via son lien d'invitation
    Alors je rejoins l'événement avec succès
    Et j'ai le rôle "Organizer" dans cet événement

  Scénario: Un participant déjà invité clique à nouveau sur le lien
    Etant donné un événement à rejoindre "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Et je rejoins l'événement via son lien d'invitation
    Quand je rejoins l'événement via son lien d'invitation
    Alors je rejoins l'événement avec succès
    Et j'ai le rôle "Participant" dans cet événement
