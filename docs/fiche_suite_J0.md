# Fiche projet · Street Mythos, démarrage du J1 (5 octobre 2026)

**Projet** : JRPG 3D au tour par tour en cel-shading, Lyon de nuit, démo web de 45 minutes. Références dans `docs/` du repo : SPEC.md (fiche en tête, D3 révisé), TECH_DESIGN.md (§ 7 jalons), style_bible.md, asset_manifest.json, glossaire_lyonnais.md, bilan_J0.md.

**Repo** : github.com/agamemlyon/street-mythos. Le J0 est sur la branche `j0-fondations`, PR vers main à fusionner. Clone local : `C:\Users\samyj\Street Mythos\StreetMythos`.

**J0 validé par Samy le 5 octobre 2026** :
- Unity 6000.3.25f1 avec le module Web, licence Personal, butler avec `BUTLER_API_KEY`, page itch.io brouillon `agamemlyon/street-mythos` (version 0.0.3, vérifiée par Samy).
- URP, UniVRM 0.131.3, shader toon, contrôle des budgets à l'import (les textures embarquées dans les GLB sont réduites par ToonConverter).
- `Tools/ci-local.ps1` : scènes, 7 tests EditMode, build Web de 10,1 Mo.
- Héros via Higgsfield (D3 révisé) : Yanis animé (marche), 24 os, sans doigts.
- Coût Higgsfield du J0 : 84,5 crédits.

**J1 Croix-Rousse** (TECH_DESIGN § 7) : exploration, dialogues Ink, combat complet (timeline, réactions, tchatche), 5 combats dont le Gros Caillou, intro vidéo (déjà faite), sauvegarde.

**À reprendre du J0** : casquette et reflets de Yanis, halos des lampadaires, 3 shaders URP non supportés, mesure sur une machine à GPU intégré, Inès et Momo à générer comme Yanis (environ 54 crédits chacun).

**Règles** : annoncer le coût Higgsfield et attendre l'accord de Samy avant chaque lot ; demander avant toute installation et tout push hors de la branche en cours ; dialogues dans le parler de cyclope_heritier (glossaire) ; 15 à 20 messages par fil. Piège connu : ne pas lancer Unity avec `Start-Process -Wait` (blocage par le client de licence).
