# Fiche projet · Street Mythos, suite du J1 (5 octobre 2026, fin d'après-midi)

**Projet** : JRPG 3D au tour par tour en cel-shading, Lyon de nuit, démo web de 45 minutes. Références dans `docs/` : SPEC.md (D3 révisé : héros via Higgsfield), TECH_DESIGN.md, style_bible.md (alignée sur les fiches retenues), asset_manifest.json, glossaire_lyonnais.md, plan_J1.md (état des 10 lots), bilan_J0.md.

**Repo** : github.com/agamemlyon/street-mythos. PR **#5 à #9** ouvertes, chacune empilée sur la précédente : **fusionner dans l'ordre 5, 6, 7, 8, 9** (chacune contient les commits des précédentes). Règle : toujours ouvrir les PR vers main.

**Ce qui marche (build 0.5.1 sur la page itch.io brouillon)** :
- Menu titre (Nouvelle partie, Continuer), vidéo d'intro passable, maquette texturée de la Croix-Rousse (place, montée, traboule, plateau), Yanis jouable en 3e personne.
- Scènes de l'acte 1 en Ink (rencontre, Guignol, Gros Caillou, fin « À suivre »), dans le parler lyonnais façon cyclope_heritier.
- 5 ennemis visibles qui lancent les 5 combats ; combat complet (timeline, menus, esquive et parade horodatées, tchatche avec bulles de vannes, 12 compétences), puis retour sur la carte avec XP, balles, niveau et sauvegarde automatique.
- Tests : 62 EditMode et 1 PlayMode verts. Chaîne : `Tools/ci-local.ps1`. Debug : `?auto=1`, `?combat=<id>&niveau=<n>`.

**À faire pour clore le J1** :
1. Samy : jouer une partie complète sur itch.io et juger l'esquive et la parade (250 et 120 ms).
2. Budget de démarrage à 24,9 Mo sur 25 : passer le décor et les ennemis en Addressables.
3. Guignol en 3D (Higgsfield, environ 45 crédits), Inès et Momo qui suivent Yanis sur la carte, animations de combat (coups, dégâts).
4. Équilibrage du boss (jugé facile en simulation), halos des lampadaires, 3 shaders URP non supportés.
5. Mesure sur une machine à GPU intégré, puis bilan du J1 et validation de Samy.

**Crédits Higgsfield** : J0 84,5 ; J1 285 (héros 111,5, ennemis 167,5, textures 6). Solde : 5 107,5 crédits.

**Règles** : chiffrer avant chaque lot Higgsfield (sauf autonomie accordée) ; demander avant toute installation ; 15 à 20 messages par fil ; pas de `Start-Process -Wait` avec Unity.
