# Description de ce document

Ce document est une liste de notes que le développeur se fait à lui même pour plus tard, Claude Code n'a pas besoin de prendre cela en compte dans ses reflexions.

# Notes

- Pouvoir supprimer un event
- Trier par date les events (et filtrer les passés)
- Changer les queries pour simplifier au max et faire directement une requête en base specifique
- Plutôt que d'avoir un EventContext dans les tests d'API, utiliser des services de résolution d'ID basé sur les informations de la step (le titre de l'event) pour éviter au maximum d'avoir un state courant du contexte
- Ajouter un moyen de reporter des bugs facilement
- afficher des erreurs clair à l'utilisateur
- Dans les tests front, faire en sorte qu'un appel à l'api non mocké throw une erreur pour éviter les oublis
