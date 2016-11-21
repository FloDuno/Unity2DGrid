**_Comment l’utiliser :_**

Ne pas changer le nom du dossier GridConfig, ni de l’asset Prefabs

Pour activer la grille, cliquer sur « **Activate Grid** » en bas de la scène

Il est absolument nécessaire d’avoir l’**option 2D activée** dans la scène pour que ça marche	

X, Y et Both permettent de choisir l’axe à changer

Sur la gauche apparait la liste des prefabs que l’on peut instancier		

Suffit de cliquer sur un pour le sélectionner

Clic gauche dans la scène : instancie le prefab sélectionné

Clic droit dans la scène : supprime le prefab sous la souris

Pour changer les prefabs que l’on peut instancier : **Assets/Grid/Prefabs**

**Attention : Tous les prefabs doivent être taggés « Environment »**

**_Comment ça marche :_**
Le script est un bordel sans nom pour l’instant. Je nettoierai si j’ai le temps. On commence par dessiner la barre en bas grace à la fonction BeginGUI. On vérifie si la grille est activée, auquel cas on dessine aussi les options de taille. Pour ça on utilise des GuiLayoutToolbar qui permettent de faire ces barres de sélection.

Si la grille n’est pas activée, on arrête le script ici. Sinon on commence par repeindre la scène pour enlever l’affichage des outils de base d’Unity. On récupère ensuite la position de la souris et on détermine la position du cube en arrondissant à l’unité cette position. Ensuite on dessine le cube de prévisualisation selon les paramètres de taille. Puis on affiche le menu sur la gauche qui va récupérer le ScriptableObject PrefabsGrid afin de lire la liste de Prefabs que l’on veut pouvoir instancier. Pour chaque élément de la liste on crée un carré avec une texture pour prévisualiser l’objet ainsi que son nom. C’est ici aussi qu’on récupère quel prefab est sélectionné. Enfin on vérifie s’il y a un clic. S’il y a un clic gauche alors on instancie le prefab à la position du cube puis on ajuste sa taille.  S’il y a eu un clic droit, on fait un Raycast à la position de la souris pour voir s’il y un block, auquel cas on le supprime.


*Source et aide :* https://www.youtube.com/watch?v=9bHzTDIJX_Q

