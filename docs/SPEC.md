# Street Mythos : Légendes du 69 · SPEC de game design

Version 0.1 du 4 octobre 2026, à valider par Samy. Détail technique : [TECH_DESIGN.md](TECH_DESIGN.md).

## Fiche projet

Street Mythos : Légendes du 69 est un JRPG 3D au tour par tour en cel-shading. La nuit, une brume avale Lyon et réveille les légendes que la ville a oubliées. Trois jeunes du 69 sont les seuls à les voir, et ils les affrontent avec leurs poings, leur débrouille et leur tchatche. Humour de quartier façon Lascars, combats inspirés de Final Fantasy X et de Clair Obscur : Expedition 33.

La première démo est un mini-chapitre d'environ 45 minutes, jouable dans un navigateur de bureau : la Croix-Rousse, puis les quais de Saône et le Vieux-Lyon, une équipe de trois héros, 8 combats dont deux boss. Elle sort en deux jalons, chacun publiable.

Stack : Unity 6 LTS, URP, build Web (WebGL 2), hébergement itch.io. Unity tourne sur le PC de Samy, que Claude pilote via une session Remote Control. Les fils cloud écrivent code, données et docs sur le dépôt GitHub. Higgsfield produit le concept art, les décors et accessoires 3D, et les cinématiques.

## 1. Décisions

### 1.1 Tranchées par Samy

| Sujet | Décision | Conséquence |
|---|---|---|
| Public de la démo | Public large, lien ouvert à tous | Unity en WebGL, hébergement statique, pas de Pixel Streaming |
| Direction artistique | Cel-shading à contours | URP, un shader toon unique pour tous les assets |
| Fantastique | Légendes lyonnaises qui se réveillent la nuit | Quotidien réaliste et drôle, combats mythiques |
| Combat | Réactif léger | Timeline façon FFX, esquive et parade pendant les tours ennemis, aides activables |
| Scope | Mini-chapitre de 45 min | 2 quartiers, 3 héros, 8 combats dont 2 boss, XP et équipement |
| Où tourne Unity | Sur le PC de Samy, piloté par Claude | Remote Control, builds locaux, GitHub pour le versionnage |

### 1.2 Défauts choisis par Claude, à valider ou corriger

- **D1 Appareils** : navigateurs de bureau seulement (Chrome, Edge, Firefox, Safari récents). Sur mobile, un écran invite à jouer sur ordinateur. Un JRPG 3D de 45 minutes dépasse la mémoire que les navigateurs mobiles accordent à une page.
- **D2 Hébergement** : page HTML5 gratuite sur itch.io.
- **D3 Héros et PNJ humains, révisé et validé par Samy le 5 octobre 2026** : modèles 3D générés par Higgsfield (image en pied, puis conversion en 3D texturé avec un squelette de 24 os), animés par les clips Higgsfield ou Mixamo. Le test du J0 sur Yanis a montré une marche propre, sans déchirure. Limites connues : pas d'os de doigts, détail du visage faible de près, accessoires parfois perdus à la conversion. VRoid reste le plan B.
- *Version initiale de D3, remplacée* : créés dans VRoid Studio (gratuit, rendu anime, squelette humanoïde prêt) à partir des fiches Higgsfield, puis animés avec des clips Mixamo. Higgsfield sort des modèles 3D statiques : parfaits pour le décor, risqués pour des corps qui doivent se plier. C'est ta principale part de travail manuel. Alternative si tu n'en veux pas : un pack de personnages anime de l'Asset Store, zéro modélisation mais des héros moins uniques.
- **D4 Ennemis** : conçus pour coûter peu en animation. Statues, pierres et esprits de brume bougent par code, sans squelette. Seul le Mâchecroute a besoin d'un squelette : on achète un dragon animé sur l'Asset Store et on le restyle.
- **D5 Budget** : plafond de 100 € de packs payants, plus tes crédits Higgsfield. Chaque lot de génération est chiffré avant lancement.
- **D6 Progression** : XP et niveaux (plafond 10), équipement sur 3 emplacements, réput' de quartier simple. L'arbre de compétences attend le jeu complet : en 45 minutes, personne n'aurait le temps de le sentir.
- **D7 Dialogues** : écrits en Ink (format texte d'inkle, open source), lisibles et modifiables sans Unity.
- **D8 Voix** : pas de doublage, texte et quelques onomatopées. La voix pèse lourd au téléchargement et coûte cher à produire.
- **D9 Classification visée** : PEGI 12 (bagarre cartoon sans sang, langage familier sans insultes graves).
- **D10 Noms** : héros, PNJ, lieux et ennemis du § 3 sont provisoires. Avant d'écrire les dialogues, Claude vérifie les sources des légendes utilisées (Mâchecroute, Gros Caillou, Ficelle).
- **D11 Code** : noms en anglais, cohérents avec l'API Unity ; commentaires et docs en français.
- **D12 Langue** : français seulement, textes externalisés pour traduire plus tard.

## 2. Vision

### 2.1 Pitch

Quand la brume tombe sur Lyon, les légendes que la ville a oubliées se réveillent. Et elles ont faim. Yanis, Inès et Momo, trois jeunes que personne ne regarde, sont les seuls à les voir.

### 2.2 Piliers

1. **Le quartier est un personnage.** On reconnaît Lyon (pentes, traboules, quais, bouchons), en version stylisée. On y vit, on y vanne, on s'y entraide.
2. **Un combat lisible et nerveux.** On planifie sur la timeline, puis on réagit au bon moment pendant les attaques adverses.
3. **L'humour est une mécanique.** Les vannes servent en combat (§ 4.5), pas seulement dans les dialogues.
4. **Des mythes vrais.** Chaque boss vient d'une légende ou d'un symbole réel de Lyon.

### 2.3 Ambition visuelle

« AAA » veut dire ici une direction artistique et une mise en scène fortes, pas du photoréalisme.

- Cel-shading à 2 ou 3 paliers d'ombre, contours épais sur les personnages, plus fins sur le décor.
- Personnages caricaturaux et très street (survêt', sacoches, casquettes, claquettes-chaussettes), habillés de marques parodiques du jeu : aucune vraie marque, pour pouvoir publier sans risque juridique.
- Nuit lyonnaise : base indigo et violet, lampadaires orange sodium, néons de commerces en accent (rose, vert, cyan), brume au ras des rues.
- Lumière précalculée, plus quelques lumières dynamiques pour les effets.
- Combats filmés par des caméras cinématiques, avec des effets dessinés : impacts, traits de vitesse, onomatopées à l'écran façon BD.
- Hors démo : ray tracing, éclairage global temps réel, foules, eau simulée, météo dynamique.

### 2.4 Inspirations

| Référence | On prend | On laisse |
|---|---|---|
| Final Fantasy X | Timeline des tours visible, équipe de 3, boss à phases | La lenteur des menus |
| Clair Obscur : Expedition 33 | Esquive et parade pendant les tours ennemis, mise en scène | Visée libre, QTE sur chaque compétence |
| Persona 5 | Quotidien le jour, surnaturel la nuit, interface stylée | Calendrier et liens sociaux |
| Yakuza : Like a Dragon | Tour par tour urbain et absurde, héros attachants | Le milieu criminel |
| Les Lascars | Ton des dialogues, débrouille, situations absurdes | Vulgarité gratuite |

### 2.5 Public et ton

- Cible : 16 à 30 ans, francophones, joueurs de JRPG et amateurs d'humour urbain.
- Ton : une comédie de quartier qui sait devenir sérieuse. À peu près deux tiers de rire, un tiers d'émotion.
- Thème : les légendes oubliées et les jeunes qu'on ne regarde pas veulent la même chose, exister aux yeux de la ville. La démo pose ce parallèle, le jeu complet le développe.

## 3. Univers et narration

### 3.1 La ville

Lyon de nuit, stylisée. Les lieux réels se reconnaissent, mais aucun commerce ni aucune personne réelle n'est nommé. La cité des héros est fictive : « les Mûriers », clin d'œil aux arbres qui nourrissaient les vers à soie des canuts. Elle n'apparaît que dans la cinématique d'intro.

**Quartier 1, la Croix-Rousse (jalon J1)**
- La place centrale, qui sert de hub : kebab, tabac, bouchon, friperie.
- La montée de la Grande Côte et ses escaliers.
- Une traboule : petit donjon linéaire avec raccourcis.
- Le tunnel de l'ancienne Ficelle, le funiculaire disparu, où une Ficelle fantôme roule la nuit.
- Le belvédère du Gros Caillou, arène du boss 1.

**Quartier 2, quais de Saône et Vieux-Lyon (jalon J2)**
- Ruelles et traboules du Vieux-Lyon.
- Une passerelle sur la Saône, noyée de brume.
- Le petit théâtre de Guignol, refuge du groupe (sauvegarde, boutique).
- Les quais bas, arène du boss 2.

### 3.2 La menace

Lyon a toujours eu ses brouillards. Celui-ci, la Grande Brume, avale les légendes que plus personne ne raconte et les recrache en monstres. Il efface aussi, peu à peu, les gens que la ville ne regarde plus.

### 3.3 Personnages (provisoires, D10)

| Héros | Qui | Rôle en combat | Arc (jeu complet) |
|---|---|---|---|
| Yanis | 19 ans, livreur à vélo, toujours en retard | Rapide : attaques multiples, joue souvent | Veut quitter le quartier, choisit de le défendre |
| Inès | 18 ans, étudiante en droit, reine du clash | Tchatche et soutien : vannes renforcées, « Objection ! » repousse un ennemi sur la timeline | Découvre que sa parole peut réparer, pas seulement blesser |
| Momo | 24 ans, ex-boxeur, tient le kebab | Tank : garde, provocation, parades rentables | A raté sa carrière, retrouve un combat qui compte |

- **Guignol** : la marionnette lyonnaise, née chez les canuts, devenue esprit. Mentor qui enseigne la tchatche et commente les règles du jeu avec une mauvaise foi totale. Héros populaire qui se moquait des puissants, il se reconnaît dans le trio.
- **Tonton Jojo** : ancien de la Croix-Rousse qui connaît toutes les légendes. Il donne une quête et des indices sur les boss.
- Une dizaine de PNJ de quartier (commerçants, gamins, mamies du marché) pour la vie et les vannes.

### 3.4 Ennemis de la démo

| Ennemi | Idée | Animation | Où |
|---|---|---|---|
| Pigeons possédés | Les pigeons de la place, version brume | Nuée animée par code | Q1, tutoriel |
| Brumeux | Esprits de la Grande Brume | Shader et particules, sans squelette | Q1 et Q2 |
| Lions de pierre | Le lion, emblème de Lyon, en statue qui s'anime | Mouvements rigides par code | Q1 et Q2 |
| Contrôleurs fantômes | L'équipage de la Ficelle fantôme : « Titre de transport ! » | Humanoïdes flottants, clips Mixamo | Q1, élite |
| Gones de l'ombre | Doubles d'ombre des jeunes que la ville oublie | Squelette et clips des héros, shader silhouette | Q2 |
| Boss 1 : le Gros Caillou | Le rocher du boulevard de la Croix-Rousse, réveillé en golem de pavés | Blocs rigides animés par code | Q1 |
| Boss 2 : le Mâchecroute | Le dragon de la légende lyonnaise, surgi de la Saône | Modèle animé acheté (D4) | Q2 |

### 3.5 Déroulé du mini-chapitre

| Séquence | Contenu | Durée cible |
|---|---|---|
| Intro | Vidéo Higgsfield de 60 à 90 s : la cité des Mûriers, la brume monte, Yanis part livrer à la Croix-Rousse | 2 min |
| Acte 1, Croix-Rousse | Rencontre d'Inès et Momo, combat tutoriel contre les pigeons, traboule et rencontre de Guignol (tutoriel tchatche), 2 combats, élite des Contrôleurs fantômes, boss du Gros Caillou | 20 min |
| Fin du jalon J1 | Écran « À suivre » dans la version J1 | |
| Acte 2, Saône et Vieux-Lyon | Passerelle dans la brume, 2 combats dont les Gones de l'ombre, quête de Tonton Jojo, boss du Mâchecroute | 20 min |
| Fin | « La légende commence… », teaser du prochain quartier (la Guillotière) | 2 min |

Deux quêtes secondaires courtes, une par quartier.

### 3.6 Humour et lignes rouges

On rit de :
- l'autodérision des héros et de leurs galères (retards, fins de mois, débrouille) ;
- l'absurde des légendes, comme un rocher susceptible ou des contrôleurs fantômes qui verbalisent un dragon ;
- Lyon et ses clichés : quenelles, bouchons, « gone », brouillard, la colline qui prie et celle qui travaille ;
- les codes du JRPG : Guignol trouve ridicule qu'on attende son tour pour se battre.

Lignes rouges, sans exception :
- aucune vanne sur l'origine, la religion, le genre, l'orientation sexuelle, le handicap ou le physique ;
- aucune insulte discriminante, aucun contenu sexuel ;
- ni drogue, ni arme à feu, ni gang réel, ni délinquance glorifiée ;
- ni police, ni élus, ni personnes réelles dans le rôle d'ennemis ;
- violence cartoon sans sang : les ennemis humains s'enfuient ou tombent dans les pommes ;
- jamais de banlieue misérabiliste. Les héros sont compétents et drôles, pas des clichés.

Claude passe chaque texte au crible d'une liste de mots interdits (test automatique) et de cette grille. Samy tranche les cas limites.

## 4. Gameplay

### 4.1 Boucle

Explorer, parler et accepter des quêtes, combattre les ennemis visibles sur la carte, gagner XP, « balles » (la monnaie) et objets, s'équiper, puis avancer jusqu'au boss du quartier.

### 4.2 Exploration

- Troisième personne, caméra orbitale libre, collisions de caméra.
- Marche et course. Pas de saut libre : les niveaux restent simples à construire et à tester.
- Les ennemis patrouillent à l'écran. Les frapper avant le contact donne un premier tour gratuit à l'équipe ; se faire surprendre le donne aux ennemis.
- Interactions : PNJ, objets à ramasser, portes de traboule, bancs de sauvegarde.
- Une boussole d'objectif en haut de l'écran remplace la minicarte.

### 4.3 Combat : règles de base

- 3 héros contre 1 à 4 ennemis, dans une arène propre au quartier, chargée par-dessus l'exploration.
- La timeline affiche les 8 prochains tours. Délai avant le prochain tour d'une unité = délai de base de l'action × 100 / VIT.
- Délais de base : Attaque 100, Compétence 120 à 150, Tchatche 80, Objet 80, Défense 60.
- Actions : Attaque, Compétence, Tchatche, Objet, Défense, Fuite (impossible contre une élite ou un boss).
- Ressource **Flow** (max 10) : +1 par attaque qui touche, +1 par vanne efficace, +2 par parade réussie. Une compétence coûte 2 à 5 Flow. Parer rapporte, comme dans Clair Obscur.
- Dégâts = ATQ × puissance / 100 × 100 / (100 + DEF) × aléa de 0,95 à 1,05. Critique ×1,5, cible déstabilisée ×1,5, cible en défense ×0,5.
- Défaite : le combat recommence avec l'état de l'équipe d'avant le combat, sans pénalité.

### 4.4 Réactions pendant les tours ennemis

- Chaque coup ennemi s'annonce par un signal visuel (éclair et icône) et un son, juste avant l'impact.
- **Esquive** (Espace ou bouton Sud) : fenêtre de 250 ms centrée sur l'impact, aucun dégât.
- **Parade** (F ou bouton Ouest) : fenêtre de 120 ms, aucun dégât, contre-attaque automatique et +2 Flow.
- Raté : dégâts normaux. Une attaque à plusieurs coups demande une réaction par coup.
- Aide 1 : fenêtres doublées. Aide 2 : esquive automatique, la parade reste manuelle.
- Ces fenêtres sont des valeurs de départ, réglées en playtest.

### 4.5 Tchatche

- Chaque ennemi a une jauge de Moral de 0 à 100.
- L'action Tchatche propose trois vannes : Clash (frontale), Chambrage (moqueuse), Mytho (bluff). Chaque ennemi craint un type, que ses répliques et Tonton Jojo laissent deviner.
- Vanne efficace : −40 Moral. Neutre : −20. Ratée : −5, et l'ennemi s'énerve (+10 % ATQ pendant 2 tours).
- Moral à 0 : l'ennemi est Déstabilisé. Il perd son prochain tour et prend ×1,5 dégâts, puis son Moral remonte à 50. Un boss reste Brisé 2 tours.
- Inès fait +50 % de dégâts de Moral.
- Pour la démo : 3 vannes par type et par héros, soit 27, plus les répliques des ennemis, toutes en Ink.

### 4.6 Compétences

Quatre par héros : deux au départ, une au niveau 3, une au niveau 6.

| Héros | Compétences |
|---|---|
| Yanis | Livraison express (2 coups, délai 80), Dérapage (dégâts à tous), Coursier de nuit (+VIT équipe, 3 tours), Express légendaire (gros coup) |
| Inès | Punchline (dégâts et −20 Moral), Objection ! (repousse un ennemi de 3 places), Pep talk (soin léger et +2 Flow à un allié), Plaidoirie (−30 Moral à tous) |
| Momo | Garde du boxeur (provocation et défense, 2 tours), Uppercut (étourdit une cible à moins de 50 Moral), Sauce blanche (soin d'équipe), Crochet du droit (très gros coup) |

Le Coup de légende, attaque combinée à trois avec cinématique, est en priorité 2 : il entre dans la démo seulement si le budget d'animation le permet.

### 4.7 Progression (D6)

- XP partagée, niveau maximum 10 dans la démo.
- Équipement : Tenue (DEF), Sneakers (VIT), Accessoire (effet, par exemple « Ticket TCL porte-bonheur » : +1 Flow en début de combat). Une douzaine d'objets.
- Réput' de quartier : elle monte avec les quêtes et certains choix de dialogue. Au palier, le kebab fait −20 % et un PNJ livre un secret.
- Boutiques : kebab (soins), tabac (objets), friperie (équipement).
- Sauvegarde automatique à chaque entrée de zone et après chaque combat, plus les bancs. Un seul emplacement.

### 4.8 Démo et jeu complet

| | Démo | Jeu complet (vision) |
|---|---|---|
| Quartiers | 2 | 6 à 8 du 69 : Guillotière, Part-Dieu, Confluence, Vaulx-en-Velin… |
| Héros | 3 | 5 ou 6, équipe de 3 interchangeable |
| Progression | XP, équipement, réput' simple | Arbres de compétences, réput' par quartier, liens entre héros |
| Légendes | 2 boss | Une légende majeure par quartier, invocations |
| Durée | 45 min | 20 à 30 h |

## 5. UX, contrôles, accessibilité

### 5.1 Caméras

- Exploration : troisième personne orbitale, angles fixes dans les traboules étroites.
- Combat : caméras automatiques avec Cinemachine. Plan large pendant le choix, épaule à l'attaque, gros plan sur les compétences et les vannes.
- Dialogues : champ-contrechamp avec portraits.

### 5.2 Contrôles

| Action | Clavier et souris | Manette |
|---|---|---|
| Se déplacer | ZQSD, WASD ou flèches | Stick gauche |
| Caméra | Souris | Stick droit |
| Interagir, valider | E, Entrée | Sud (A, Croix) |
| Annuler | Échap | Est (B, Rond) |
| Esquive | Espace | Sud |
| Parade | F | Ouest (X, Carré) |
| Menu | Tab | Start |

Le jeu lit la position physique des touches : AZERTY et QWERTY marchent sans réglage. Toutes les touches sont réassignables.

### 5.3 Feedback en combat

Intention de l'ennemi affichée avant son tour, signal de réaction, « PARFAIT ! » et arrêt sur image à la parade, chiffres de dégâts, aperçu sur la timeline de l'effet d'une action avant de la valider.

### 5.4 Accessibilité

- Textes toujours affichés, deux tailles de texte.
- Contraste AA (WCAG) sur les textes d'interface.
- Signaux de réaction par forme, couleur et son, jamais par la couleur seule.
- Aides de réaction (§ 4.4) et mode Détente : dégâts ennemis −40 %.
- Jamais plus de 3 flashs par seconde.
- Pause partout hors cinématique, cinématiques passables.

## 6. Critères d'acceptation

### 6.1 La SPEC est prête pour l'implémentation quand

- [x] Les 6 décisions structurantes sont prises (§ 1.1).
- [x] Les règles de combat sont chiffrées assez pour écrire des tests (§ 4.3 à 4.5).
- [x] Les assets sont comptés avec une source chacun (TECH_DESIGN § 6).
- [x] Les budgets web sont chiffrés (TECH_DESIGN § 3).
- [ ] Samy a validé ou corrigé les défauts D1 à D12.
- [ ] Samy a donné le signal « OK, passe à l'implémentation de la démo web ».

### 6.2 La démo est finie quand

- Elle se lance depuis sa page itch.io sur Chrome, Edge, Firefox et Safari récents (bureau), sans erreur bloquante en console.
- Un joueur la termine, de l'intro à « La légende commence… », en 35 à 55 minutes, en mode Normal comme en Détente.
- Les 8 combats se gagnent, et une défaite relance le combat proprement.
- Elle tient 30 images par seconde ou plus, en exploration comme en combat, sur la machine de référence (TECH_DESIGN § 3).
- Clavier, souris et manette fonctionnent, en AZERTY comme en QWERTY.
- La sauvegarde survit à la fermeture de l'onglet.
- Les budgets de téléchargement et de mémoire sont tenus.
- Tous les textes passent la grille des lignes rouges.
