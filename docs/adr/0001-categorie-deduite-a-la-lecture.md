# La catégorie d'un article est déduite à la lecture, seule la correction est stockée

Un article n'a pas de colonne « catégorie ». On stocke seulement une catégorie corrigée (`CorrectedCategory`, facultative, `null` dans la plupart des cas). La catégorie effective vaut cette correction si elle existe, sinon la catégorie déduite de l'intitulé par un dictionnaire de mots-clés écrit dans le Domain, recalculée à chaque lecture. Ce choix a deux effets voulus : chaque amélioration du dictionnaire reclasse aussitôt tous les articles non corrigés, sans migration ni rattrapage, et une correction faite à la main reste figée. C'est possible parce que l'intitulé d'un article ne change jamais après sa création.

## Option écartée

Stocker la catégorie calculée à la création. Une correction aurait écrasé cette valeur. Il aurait fallu rattraper les articles existants, et un dictionnaire amélioré ne se serait appliqué qu'aux nouveaux articles.

## Conséquences

- Ne pas « corriger » la colonne presque toujours vide en y écrivant la catégorie déduite : on figerait le classement actuel et on perdrait la reclassification automatique.
- Si le dictionnaire est un jour remplacé par un classement coûteux ou non déterministe (un LLM, par exemple), ce choix est à revoir : il faudra alors stocker le résultat.
