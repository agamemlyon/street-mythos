# Plan du J1 Croix-Rousse · mis à jour le 5 octobre 2026

Sortie attendue (TECH_DESIGN § 7) : le quartier 1 publiable seul, avec l'écran « À suivre ». Une branche et une PR par lot. Samy valide le J1 à la fin.

| Lot | Contenu | Coût Higgsfield | État |
|---|---|---|---|
| 1. Modèle de combat | C# pur et testé : timeline, actions, dégâts, Flow, Moral et tchatche, statuts, victoire et défaite | 0 | fait (PR #2) |
| 2. Données | JSON des héros, compétences, ennemis et rencontres, chargeur validé, simulation d'équilibrage | 0 | fait (PR #2) |
| 3. Héros | Inès et Momo en 3D animés, comme Yanis | 111,5 | fait (PR #3, puis #4 vers main) |
| 4. Présentation du combat | Arène, timeline (UI Toolkit), menus, réactions horodatées, pilote automatique | 0 | fait (PR #5) |
| 5. Ennemis du quartier 1 | Pigeon, lion, Contrôleur, Gros Caillou (Higgsfield, animés par code) et Brumeux (shader de brume) | ≈ 168 | fait (PR #6) |
| 6. Exploration | Contrôleur 3e personne, caméra orbitale, ennemis visibles qui lancent le combat, retour sur la carte | 0 | maquette jouable (PR #8) |
| 7. Croix-Rousse | Kit de façades modulaires (textures Higgsfield), traboule, place, montée | à chiffrer | maquette en formes simples (PR #8) ; textures à faire |
| 8. Dialogues | Ink, 27 vannes, répliques ennemies, scènes de l'acte 1, test des mots interdits | 0 | fait : en combat (PR #7) et en exploration (PR #8) |
| 9. Intro, sauvegarde, menus | Sauvegarde JSON, menu titre, écran « À suivre », vidéo d'intro lue par URL | 0 | sauvegarde, menu titre et « À suivre » faits (PR #8) ; vidéo d'intro à brancher |
| 10. Passe finale | Budgets, build, test navigateur, publication privée, partie complète par Samy | 0 | à faire |

Points ouverts : build à 24,3 Mo pour 25 Mo de budget (passer le décor en Addressables), équipement et boutiques (J2 selon la SPEC), quêtes secondaires, Guignol encore en forme provisoire, mesure sur une machine à GPU intégré.
