# Street Mythos : Légendes du 69 · TECH_DESIGN

Version 0.1 du 4 octobre 2026, à valider par Samy. Game design : [SPEC.md](SPEC.md).

## 1. Moteur et cible web

| Sujet | Choix | Pourquoi |
|---|---|---|
| Moteur | Unity 6, dernière version LTS au démarrage (à lire dans Unity Hub) | Export web natif. Unreal n'en a plus et imposerait Pixel Streaming |
| Rendu | URP | HDRP ne tourne pas sur le web |
| API graphique | WebGL 2 | WebGPU reste expérimental dans Unity 6 ; on le testera après la démo |
| Plateforme | Web, navigateurs de bureau (SPEC D1) | Mémoire des navigateurs mobiles insuffisante |
| Hébergement | itch.io (SPEC D2), compression Brotli | Gratuit, public, fait pour les jeux web |
| Licence | Unity Personal | Gratuite sous le seuil de revenus fixé par Unity |

Pixel Streaming est écarté : il faut un GPU cloud par joueur connecté, la latence se sent, et la facture monte avec l'audience. Ça ne colle pas avec un public large.

## 2. Paquets

| Besoin | Paquet |
|---|---|
| Clavier, souris, manette, réassignation | Input System |
| Caméras d'exploration et de combat | Cinemachine 3 |
| Chargement par quartier | Addressables |
| Interface | UI Toolkit (UXML et USS, des fichiers texte que Claude écrit directement) |
| Cinématiques en jeu | Timeline |
| Dialogues et vannes | Ink, via ink-unity-integration (licence MIT) |
| Tests | Unity Test Framework |
| Import des personnages VRoid | UniVRM (compatibilité URP à vérifier au J0) |

Aucun autre paquet sans l'accord de Samy.

## 3. Budgets web

Machine de référence : portable à GPU intégré récent (type Intel Iris Xe ou Apple M1), Chrome à jour, écran 1080p.

| Poste | Budget |
|---|---|
| Démarrage (moteur et menu) | 25 Mo compressés au plus |
| Quartier 1 | 80 Mo au plus, téléchargé après le menu |
| Quartier 2 | 80 Mo au plus, téléchargé en fond pendant l'acte 1 |
| Mémoire au pic | 1,5 Go au plus |
| Images par seconde | 30 minimum sur la machine de référence, 60 visées sur GPU dédié |
| Triangles à l'écran | 400 000 au plus |
| Draw calls | 250 en exploration, 200 en combat |
| Héros | 30 000 triangles, textures 1024 (réduction à l'export VRoid) |
| PNJ | 15 000 triangles |
| Accessoires | 3 000 triangles |
| Textures | DXT avec compression Crunch, 2048 maximum (atlas de décor) |
| Lumière | 2 à 4 lightmaps 2048 par quartier, light probes, 4 lumières temps réel visibles au plus |
| Post-traitement | Bloom léger, étalonnage par LUT, vignette. Ni SSAO ni reflets temps réel |
| Musique | 4 morceaux compressés, lus en streaming |
| Vidéos | Intro et fin hébergées à côté du build, lues par URL (sur le web, le VideoPlayer de Unity ne lit que par URL), passables |

On mesure au J0 sur une scène test, puis à chaque jalon. Un budget dépassé bloque la publication.

## 4. Architecture du projet

### 4.1 Dossiers

```
StreetMythos/                      # dépôt Git avec Git LFS
  Assets/_Project/
    Art/Incoming/                  # assets bruts, traités automatiquement à l'import
    Art/Characters/  Art/Environments/Q1_CroixRousse/  Art/Environments/Q2_Saone/
    Art/Props/  Art/VFX/  Art/UI/
    Shaders/                       # shader toon et contours, en HLSL
    Audio/Music/  Audio/SFX/
    Data/Json/                     # source de vérité : persos, compétences, ennemis, objets, rencontres
    Data/Generated/                # ScriptableObjects générés depuis le JSON
    Dialogues/                     # fichiers .ink
    Scenes/                        # Boot, MainMenu, Q1_CroixRousse, Q2_Saone, Arena_Q1, Arena_Q2
    Scripts/Core/  Exploration/  Battle/  Dialogue/  Progression/  UI/  Save/  Editor/
    Tests/EditMode/  Tests/PlayMode/
    UI/                            # UXML et USS
    Timeline/
  Assets/AddressableAssetsData/
  Assets/WebGLTemplates/StreetMythos/   # page de chargement aux couleurs du jeu
  Tools/                           # scripts de build, de déploiement, de contrôle des budgets
  docs/                            # SPEC.md, TECH_DESIGN.md, style_bible.md, asset_manifest.json
```

### 4.2 Scènes et états

- Boot lance les services persistants (sauvegarde, audio, entrées, état du jeu), puis le menu.
- Les quartiers se chargent en additif via Addressables. L'arène de combat se charge par-dessus, et le quartier se met en pause.
- Machine à états : Boot, Menu, Exploration, Dialogue, Combat, Cinématique, Pause.

### 4.3 Code

- **Combat en deux couches.** Le modèle, en C# pur sans MonoBehaviour, gère timeline, actions, dégâts, statuts, Moral, victoire et défaite. Il est déterministe à graine fixe, donc testable en EditMode. La présentation (animations, caméras, effets, UI) écoute ses événements.
- **Réactions.** Le modèle émet l'instant d'impact de chaque coup. Un composant ReactionWindow compare l'horodatage de l'entrée, en temps non mis à l'échelle, à cet instant. Les fenêtres ne dépendent donc pas de la fréquence d'image.
- **Données.** Claude écrit le JSON ; un importeur éditeur régénère les ScriptableObjects à chaque modification. Aucun texte de jeu en dur dans le code.
- **Dialogues.** Ink, avec des variables partagées avec le jeu (réput', quêtes).
- **Sauvegarde.** JSON versionné dans PlayerPrefs, que Unity range dans IndexedDB sur le web. Un emplacement.
- **Conventions.** Namespaces StreetMythos.Core, StreetMythos.Battle, etc. Noms en anglais, commentaires en français (SPEC D11).
- **Debug.** Menu F1 absent du build public : téléportation, XP, victoire instantanée, affichage des images par seconde et de la mémoire.

### 4.4 Rendu cel-shading

- Un shader toon maison en HLSL pour URP : 2 ou 3 paliers d'ombre par rampe, reflet en aplat, liseré de lumière.
- Contours par coque inversée sur les personnages et les gros accessoires, moins chers qu'un contour plein écran.
- L'import applique ce shader à tout asset. C'est lui qui fait tenir ensemble VRoid, Higgsfield et l'Asset Store.

## 5. Pipeline Claude, Higgsfield et Samy

### 5.1 Qui fait quoi

| Acteur | Fait | Ne fait pas |
|---|---|---|
| Claude, fil cloud | Docs, JSON, dialogues Ink, code C# et tests, shaders, UI Toolkit, scripts éditeur et de build, prompts et lots Higgsfield | Ouvrir Unity, compiler, builder |
| Claude, session sur le PC de Samy | Compilation, tests, assemblage des scènes par scripts éditeur, import des assets, builds Web, test du build dans le navigateur, publication sur itch.io | Valider seul le rendu artistique |
| Higgsfield | Concept art et fiches personnages, accessoires 3D (GLB statiques), textures de façades, fonds peints (Fourvière, ciels), vidéos d'intro et de fin, trailer | Personnages animés (SPEC D3) |
| Samy | Installation (§ 5.3), validation visuelle par lots, création des héros et PNJ dans VRoid, téléchargement des clips Mixamo listés par Claude, accord sur les dépenses | Assembler les scènes à la main |

### 5.2 Orchestration

1. Claude écrit la bible de style (docs/style_bible.md) : palette, proportions (6 têtes), épaisseur des contours, ambiance. Chaque prompt Higgsfield la cite.
2. Une fiche personnage Higgsfield par héros et par boss sert de référence unique au concept art, à VRoid et aux vidéos.
3. Claude tient le manifeste des assets (docs/asset_manifest.json) : source, prompt, licence, budget et statut de chaque asset.
4. Claude lance les générations Higgsfield par lots, en annonçant le coût ; Samy donne son accord avant chaque lot.
5. La session sur le PC télécharge les fichiers dans Art/Incoming. Un AssetPostprocessor applique échelle, shader toon, compression, colliders et LOD, puis refuse tout asset hors budget.
6. Claude poste des captures dans le fil du projet. Samy valide ou demande une reprise.

### 5.3 Installation sur le PC de Samy

À faire une fois, avec un guide pas à pas que Claude écrit au J0 :
- Unity Hub, Unity 6 LTS et le module Web Build Support.
- Git, Git LFS, et un dépôt GitHub privé « street-mythos ».
- Node.js, pour le serveur local et le test automatique du build.
- butler, l'outil d'envoi d'itch.io, avec sa clé d'API en variable d'environnement (avant la première publication).
- VRoid Studio et un compte Adobe pour Mixamo, gratuits tous les deux.
- Remote Control activé sur le dossier du projet.

### 5.4 Cohabitation avec l'éditeur Unity

Unity refuse d'ouvrir un projet déjà ouvert ailleurs. Claude compile et teste donc dans un second clone du dépôt (StreetMythos-build), pendant que Samy garde l'éditeur ouvert sur le clone principal.

```
Unity -batchmode -projectPath ../StreetMythos-build -runTests -testPlatform EditMode -testResults results.xml
Unity -batchmode -quit -projectPath ../StreetMythos-build -executeMethod StreetMythos.Editor.BuildScript.BuildWeb
butler push Builds/Web <compte-itch>/street-mythos:html5
```

À évaluer au J0 : un serveur MCP pour Unity (il en existe des projets communautaires), qui laisserait Claude agir directement dans l'éditeur ouvert.

### 5.5 Git

- Sérialisation Force Text et méta-fichiers visibles : scènes et prefabs restent lisibles et comparables.
- Git LFS pour .fbx, .glb, .vrm, .png, .psd, .wav, .ogg et .mp4.
- Une tâche par branche, une PR courte, et main toujours buildable.
- Quand le PC est éteint, les fils cloud poussent code et données ; la session PC récupère, compile et teste ensuite.

### 5.6 Compilation sur GitHub

Hors démo. Si le PC devient un goulot, on ajoutera GameCI sur GitHub Actions avec la licence Unity de Samy.

## 6. Assets de la démo

| Catégorie | Quantité | Source |
|---|---|---|
| Héros | 3 | VRoid et Mixamo |
| Guignol | 1 | GLB Higgsfield animé par code : une marionnette qui bouge raide, c'est le personnage |
| PNJ | 10, sur 4 modèles de base recolorés | VRoid et Mixamo |
| Ennemis normaux | 5 types | Code et shaders (pigeons, Brumeux, lions), Mixamo (Contrôleurs), squelettes des héros (Gones de l'ombre) |
| Boss | 2 | Gros Caillou : GLB Higgsfield découpé en blocs. Mâchecroute : dragon animé de l'Asset Store |
| Kit Croix-Rousse | environ 40 modules | Façades modulaires générées par script avec textures Higgsfield, accessoires GLB Higgsfield |
| Kit Saône et Vieux-Lyon | environ 40 modules | Même méthode, 30 % repris du kit 1 |
| Fonds peints | 4 | Images Higgsfield |
| Animations | environ 50 clips | Mixamo (humanoïdes), code (ennemis rigides), pack (dragon) |
| Effets visuels | environ 25 | Particules et textures dessinées (images Higgsfield) |
| Interface | Menus, HUD, timeline, portraits | UI Toolkit, portraits Higgsfield |
| Musique | 4 morceaux | Bibliothèque libre de droits (Higgsfield ne génère pas de musique hors de son outil de jeux) |
| Bruitages | environ 60 | Bibliothèque libre de droits |
| Vidéos | Intro, fin, trailer | Higgsfield vidéo |

## 7. Jalons

| Jalon | Contenu | Sortie |
|---|---|---|
| J0 Fondations | Installation, dépôt, squelette du projet, shader toon, un héros VRoid animé, un accessoire Higgsfield, build Web vide publié en privé sur itch.io, mesures | Go ou no-go sur D1 à D5, chiffres réels à l'appui |
| J1 Croix-Rousse | Exploration, dialogues Ink, combat complet (timeline, réactions, tchatche), 5 combats dont le Gros Caillou, intro vidéo, sauvegarde | Quartier 1 publiable seul |
| J2 Saône et Vieux-Lyon | Quartier 2 en Addressables, 3 combats dont le Mâchecroute, équipement, réput', fin, passes performance et accessibilité, trailer | Démo complète publique |

Un jalon ne démarre qu'après validation du précédent par Samy.

## 8. Risques

| Risque | Parade |
|---|---|
| Le mini-chapitre est trop gros | J1 publiable seul, ennemis animés par code, kit de décor réutilisé à 30 % |
| Mémoire ou fluidité insuffisantes dans le navigateur | Budgets du § 3 mesurés dès J0, un quartier chargé à la fois, lumière précalculée |
| Assets IA incohérents ou mal construits | Shader toon unique, bible de style, contrôle à l'import, personnages animés hors Higgsfield |
| Timing des réactions instable | Entrées horodatées indépendamment de l'image, fenêtres larges, aides |
| Dépendance au PC de Samy | Les fils cloud avancent sur le dépôt quand le PC est éteint ; GameCI possible plus tard |
| Licences (Mixamo, VRoid, Asset Store, Higgsfield) | Licence notée par asset dans le manifeste ; conditions d'usage commercial Higgsfield vérifiées avant publication |
| Dérapage de ton | Grille des lignes rouges et test automatique de mots interdits |
| Build refusé ou mal servi par itch.io | Publication test dès J0 ; option Decompression Fallback si itch.io sert mal le Brotli |

## 9. Tests

- **EditMode** : formules de dégâts, ordre de la timeline, statuts, Moral, victoire et défaite, cohérence des données (références, budgets), mots interdits.
- **PlayMode** : démarrage, entrée dans le quartier 1, combat gagné via le debug, retour en exploration, sauvegarde puis rechargement.
- **Build Web** : test automatique dans Chromium (chargement, menu, aucune erreur console), puis une partie complète par Samy à chaque jalon.
- **Mesures** : taille, mémoire et images par seconde relevées à chaque build publié.
