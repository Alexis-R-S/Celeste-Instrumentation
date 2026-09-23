# Tutoriel de mise en place d'un agent sur Céleste

## Étape 1 : Prérequis

- Windows : le mod fonctionne sur la version FNA de Céleste.
- Céleste acheté sur Steam ou itch.io (Version FNA = version Windows). Il peut être installé depuis d'autres sources mais je ne peux pas garantir que tout fonctionne.
- Python 3.14
- dnSpy
- Celeste-Instrumentation
- Visual Studio

J'ai aussi installé ceci durant mes tests, je n'ai pas encore déterminé si ces logiciels sont nécessaires ou non :
- Microsoft XNA Framework Redistribuatable 4.0
- Microsoft XNA Framework Redistribuatable 4.0 Refresh
- Microsoft XNA Game Studio Platform Tools

### Installer Celeste FNA sur Steam

Sur Steam, aller dans Bibliothèque > Celeste > La roue dentée > propriétés > Versions et betas du jeu puis choisir la version opengl du jeu

### Installer dnSpy

dnSpy est un logiciel permettant de débugger et d'éditer du code .NET sans avoir le code source.  
Le dépôt est disponible ici : https://github.com/dnspy/dnspy

Vous trouverez le code source en arrivant sur la page. Une version compilée de dnSpy est disponible en cliquant sur ```tags``` puis sur le numéro de version.

### Installer Celeste-Instrumentation

Ce dépôt est un fork de hdrien0/Celeste-Instrumentation, qui fonctionnait avec XNA (un vieux framework Microsoft dont le SDK est difficile à trouver).

Commencer par clôner ce dépôt : ```git clone --recursive [Lien du dépôt].git```

[Optionnel] Se déplacer vers une version spécifique : ```git checkout tags/[Nom du tag]```

Ce dépôt contient plusieurs projets :
- CelesteInstrumentation : Le patch C# sur le jeu Celeste
- CelestePythonInterface : Bibliothèque python pour faire l'interface entre le patch C# et python.

> Note: CelestePythonInterface utilise une connexion socket pour communiquer. Il est donc possible d'utiliser n'importe quel langage si on réécrit l'interface.

## Étape 2 : Patcher le jeu

### Compiler le patch

Vérifier (éventuellement modifier) le chemin de l'exécutable dans `Directory.Build.props`. Pour une installation Steam sur Windows, le dossier du jeu se trouve ici `C:\Program Files (x86)\Steam\steamapps\common\Celeste`.

Ouvrir la solution CelesteInstrumentation dans Visual Studio (`CelesteInstrumentation.sln`), puis compiler le projet en utilisant le bouton `Régénérer la solution`.

Si il y a des problèmes à cette étape, m'envoyer un message pour que j'ajoute les éléments manquants (c'est probable).

Normalement, le projet est configuré pour copier la dll générée directement dans le dossier de jeu pour que le patch s'applique immédiatement.

### Placer les fichiers de patch dans le dossier du jeu

Pour une installation Steam sur Windows, le dossier du jeu contenant `Celeste.exe` se trouve ici `C:\Program Files (x86)\Steam\steamapps\common\Celeste`. Il faut placer plusieurs fichiers dans ce dossier :

- `CelesteInstrumentation.dll` se trouve normalement déjà dans ce dossier (il a été copié automatiquement à l'étape précédente, et sera copié de nouveau à chaque compilation).
- Copier l'assembly `0Harmony.dll` généré par le projet vers le dossier du jeu. Les assemblys sont normalement générés dans `CelesteInstrumentation\bin\x86\Debug\net4.8`.
- Copier le fichier `InstrumentationParameters.xml` présent dans le dépôt vers le dossier de jeu

### Patcher le code

1. Ouvrir `Celeste.exe` dans dnSpy.
2. Dans l'assembly explorer, naviguer vers `Celeste > Celeste.exe > Celeste > Celeste`
3. Clic droit sur le code → **Edit Class**
4. Clic sur l'icône de dossier (**Add reference**) puis ajouter `0Harmony.dll` et `CelesteInstrumentation.dll`
5. Dans la méthode `Main`, ajouter le code :
```csharp
Instrumentation.Entry.Execute();
```
6. Cliquer sur **Compile** (en bas à droite) puis `Ctrl+Shift+S` → **OK** pour sauvegarder
7. Si il y a erreur lors de la compilation sur `Microsoft.Xna.Framework` ou `Microsoft.Xna.Framework.Game`, c'est que Microsoft XNA Framework Redistributable 4.0 est nécessaire. L'installer, puis revenir dans dnSpy : clic sur l'icône de serveurs (**Add GAC reference**) puis ajouter `Microsoft.Xna.Framework` et `Microsoft.Xna.Framework.Game` et compiler.
8. Sauvegarder l'assembly modifié et quitter dnSpy

### Configuration du patch

Le patch peut-être configuré ou désactivé à partir du fichier `InstrumentationParameters.xml`.

## Étape 3 : installer les dépendances python

1. Se déplacer dans `PyCeleste`
2. Créer un environnement virtuel python :
`py -3.14 -m venv venv`
3. Activer l'environnement python. Dans la console :
`.\venv\Scripts\activate`
4. Installer les dépendances :
`python -m pip install -r requirements.txt`
5. Installer la dépendance CelestePythonInterface : se déplacer dans le dépôt vers `CelestePythonInterface` (il doit y avoir un `setup.py` dans le dossier), puis exécuter : `python -m pip install .`
6.  

## Étape 4 : faire tourner un agent minimal

Un exemple d'agent se trouve dans le dossier `PyCeleste`. Cet agent minimal écoute les entrées clavier pour les retranscrire dans le jeu en mouvement. À vous de remplacer ces entrées manuelles par un réel agent IA.

En ayant l'environnement python activé, lancer `python .\PyCeleste\main.py`.

**Ensuite**, démarrer le jeu Celeste (depuis Steam par exemple). Si tout c'est bien passé, le jeu démarre avec la version modifiée, et vous pourrez clairement voir les carrés rouges de debug sur l'écran.