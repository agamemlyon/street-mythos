# Bilan du J0 Fondations · 5 octobre 2026

Sortie attendue (TECH_DESIGN § 7) : go ou no-go sur les décisions D1 à D5, chiffres réels à l'appui.

## Ce qui est livré

| Élément | État | Preuve |
|---|---|---|
| Installation | Unity 6.3 LTS (6000.3.25f1) avec le module Web, licence Personal, butler | ProjectVersion.txt, butler v15.31 |
| Dépôt | Branche `j0-fondations` poussée, Git LFS actif | github.com/agamemlyon/street-mythos |
| Squelette du projet | Dossiers, asmdef, machine à états, générateur aléatoire de combat déterministe | 7 tests EditMode verts |
| Shader toon | Paliers d'ombre, reflet en aplat, liseré, contour par coque inversée | captures dans docs/captures |
| Accessoire Higgsfield | Lampadaire lyonnais, 2 610 triangles pour 3 000 | journal d'import |
| Héros animé | Yanis via Higgsfield (D3 révisé), squelette de 24 os, marche propre | j0_yanis_marche.mp4 |
| Build Web | 10,1 Mo compressés pour un budget de 25 Mo | Builds/web-size.txt |
| Publication privée | Page brouillon itch.io agamemlyon/street-mythos, version 0.0.3 | Samy y a vu la scène sur la version 0.0.2 |

## Chiffres mesurés

- **Taille de démarrage** : 10,1 Mo, avec Yanis, 6 lampadaires et le moteur.
- **Images par seconde** : environ 128 dans Chrome sur RTX 5080 (version 0.0.1). **Non mesuré sur la machine de référence** (GPU intégré) : aucune n'est disponible ici.
- **Mémoire** : non mesurée de façon fiable. Le tas JavaScript fait 29 Mo, mais la mémoire du moteur n'est pas exposée.
- **Coût Higgsfield du J0** : 31 crédits (lampadaire) et 53,5 crédits (Yanis), soit 84,5 crédits.

## Verdict par décision

| Décision | Verdict | Pourquoi |
|---|---|---|
| D1 Navigateurs de bureau | Go | WebGL 2 tourne. L'écran « jouer sur ordinateur » existe dans le gabarit, mais n'a pas été testé sur mobile. |
| D2 itch.io | Go | Publication par butler. Il a fallu activer la décompression intégrée (Brotli mal servi). |
| D3 Héros | Go, révisé | Higgsfield remplace VRoid, validé par Samy. |
| D4 Ennemis animés par code | Pas encore testé | Prévu au J1. |
| D5 Budget de 100 € | Go | 0 € dépensé en packs. |

## Défauts connus, à reprendre au J1

1. Yanis : casquette perdue, visage peu détaillé, reflets trop brillants, 31 140 triangles pour 30 000.
2. Halos des lampadaires : les taches de lumière au sol ne tombent pas sous les lampadaires.
3. Trois shaders internes d'URP sont signalés non supportés en WebGL (sans effet visible).
4. Mesure sur une machine à GPU intégré : à faire avant la publication publique.
5. Clone `StreetMythos-build` pas encore créé (TECH_DESIGN 5.4) : inutile tant que l'éditeur n'est pas ouvert à côté.
6. PR de `j0-fondations` vers main : pas encore ouverte.
