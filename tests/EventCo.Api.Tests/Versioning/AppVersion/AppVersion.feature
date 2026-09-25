# language: fr
Fonctionnalité: Version de l'API
  En tant que frontend EventCo déjà ouvert dans un navigateur
  je veux connaître la version de l'API
  afin de me recharger si un déploiement a eu lieu depuis mon chargement

  Scénario: Consultation de la version de l'API
    Quand je consulte la version de l'API
    Alors la réponse de version a le statut 200
    Et la version retournée est celle du build déployé
    Et la réponse de version n'est pas mise en cache

  Scénario: Version portée par toute réponse de l'API, y compris en erreur
    Quand je consulte l'utilisateur courant sans cookie de session
    Alors la réponse de version a le statut 401
    Et la réponse porte l'en-tête de version du build déployé
