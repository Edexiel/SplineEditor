# Spline Editor

## Fonctionnalités 

Courbes :
- Bezier
- BSpline
- Hermite
- Catmull-rom

Les courbes sont affichées et éditable dans la scene d'unity et sont configurable dans l'inspecteur sur le script "spline"

Les courbes sont rendues en temps réel grace au line renderer d'unity

Les points de controle sont affichés et modifiables en temps reel grace aux handles d'unity

Les courbes sont automatiquement sauvegardées et restaurées grace au systeme de serialisation d'unity

Nous pouvons changer en temps réel le type de courbe

Quand nous avons "smooth spline" activé dans l'éditeur, les noeuds de controle vont se réfléchir avec le noeud de fin de la section de courbe en son centre

On peut ajouter une nouvelle courbe grace a ubouton "add curve" et la retirer avec "delete last curve"

On peut choisir la resolution de rendu de la courbe dans une gamme [1-1000]

## Problemes connus

Il faut passer la souris sur la fenetre de scene d'unity pour rafraichir une courbe apres un changement

## Amélioration possible

Pouvoir supprimer des courbes dans la spline, pas juste a la fin, mais il faudrait dessiner dans l'inspecteur un éditeur de courbe pour pouvoir selectionner individuellement les courbes.



