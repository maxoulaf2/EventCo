# Description de ce document

Ce document est une liste de notes que le développeur se fait à lui même pour plus tard, Claude Code n'a pas besoin de prendre cela en compte dans ses reflexions.

# Notes

- Mettre un rate limit sur l'envoi de mail pour éviter le spam
- Plutôt que d'avoir un EventContext dans les tests d'API, utiliser des services de résolution d'ID basé sur les informations de la step (le titre de l'event) pour éviter au maximum d'avoir un state courant du contexte
- Scoper le dbcontext à une requete et faire le SaveChangesAsync à la fin de la requête plutôt que dans les repository
- Dans les tests back : virer le "Via l'API" dans les steps de test