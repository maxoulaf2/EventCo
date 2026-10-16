# language: fr
Fonctionnalité: Consultation des catégories d'articles via l'API
  En tant qu'utilisateur connecté je veux connaître les catégories d'articles, leur libellé et leur ordre
  afin que le frontend n'en garde aucune copie

  Scénario: Consultation des catégories avec une session valide
    Etant donné une session ouverte via l'API pour "get-item-categories-api-test@example.com"
    Quand je consulte les catégories d'articles via l'API
    Alors la réponse de consultation des catégories a le statut 200
    Et les catégories retournées sont dans l'ordre "Apéro", "Entrées et plats", "Fromage et pain", "Desserts", "Boissons", "Matériel et déco", "Autre"
    Et la catégorie retournée "Boissons" a la clé "Drinks"

  Scénario: Consultation des catégories sans cookie de session
    Quand je consulte les catégories d'articles via l'API sans cookie de session
    Alors la réponse de consultation des catégories a le statut 401
