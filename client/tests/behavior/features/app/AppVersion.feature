# language: fr
Fonctionnalité: Rechargement automatique après un déploiement
  En tant qu'utilisateur ayant laissé l'application ouverte (onglet ou PWA installée)
  je veux qu'elle se recharge d'elle-même quand une nouvelle version a été déployée
  afin de ne jamais utiliser un frontend périmé face à une API plus récente

  Scénario: Rechargement quand une réponse de l'API vient d'une autre version
    Etant donné que l'application est en version "build-a"
    Et que l'API a été déployée en version "build-b"
    Quand j'arrive sur le tableau de bord
    Alors l'application est rechargée

  Scénario: Rechargement au retour sur l'onglet quand l'API a été redéployée
    Etant donné que l'application est en version "build-a"
    Et que l'API a été déployée en version "build-b"
    Quand je reviens sur l'onglet de l'application
    Alors l'application est rechargée

  Scénario: Pas de rechargement quand l'API est dans la même version
    Etant donné que l'application est en version "build-a"
    Et que l'API a été déployée en version "build-a"
    Quand je reviens sur l'onglet de l'application
    Alors l'application n'est pas rechargée

  Scénario: Pas de rechargement hors build déployé (développement local)
    Etant donné que l'application est en version "dev"
    Et que l'API a été déployée en version "build-b"
    Quand je reviens sur l'onglet de l'application
    Alors l'application n'est pas rechargée

  Scénario: Un seul rechargement par version, même si l'application reste périmée
    Etant donné que l'application est en version "build-a"
    Et que l'API a été déployée en version "build-b"
    Quand je reviens sur l'onglet de l'application
    Et je reviens à nouveau sur l'onglet de l'application
    Alors l'application est rechargée une seule fois
