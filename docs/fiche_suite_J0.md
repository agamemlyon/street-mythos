# Fiche projet · Street Mythos, suite du J0 (5 octobre 2026)

**Projet** : JRPG 3D au tour par tour en cel-shading, Lyon de nuit, démo web de 45 minutes. Références : SPEC.md (fiche en tête), TECH_DESIGN.md, style_bible.md, asset_manifest.json, glossaire_lyonnais.md, dans `docs/` du repo.

**Repo** : github.com/agamemlyon/street-mythos, branche `j0-fondations` (pas encore fusionnée dans main). Clone local : `C:\Users\samyj\Street Mythos\StreetMythos`.

**Fait au J0** :
- Unity 6.3 LTS (6000.3.25f1) avec le module Web et une licence Personal active. Unity Hub installé (MSIX). butler 15.31 dans `%LOCALAPPDATA%\butler`, avec la clé `BUTLER_API_KEY` dans les variables utilisateur.
- Projet URP avec UniVRM 0.131.3. Shader toon maison (paliers d'ombre et contour par coque inversée), contrôle des budgets à l'import, scènes générées par script (Boot, J0_Test).
- Chaîne locale `Tools/ci-local.ps1` (scènes, tests EditMode, build Web) : 7/7 tests verts.
- Build Web de release : 16,8 Mo pour un budget de 25 Mo, avec décompression intégrée pour itch.io. Envoyé sur la page brouillon `agamemlyon/street-mythos`.
- Lampadaire Higgsfield : 2 610 triangles pour un budget de 3 000. Coût total en crédits du J0 : 31.

**Reste pour clore le J0** :
1. Fait le 4 octobre : page itch.io réglée, Samy y voit la rue de nuit.
2. Samy : créer Yanis dans VRoid → `Art/Incoming/hero_yanis.vrm`, puis clips Mixamo.
3. Claude : importer Yanis animé, corriger le halo des lampadaires et les 3 shaders URP non supportés, mesurer sur une machine à GPU intégré, créer le clone `StreetMythos-build`, ouvrir la PR vers main.
4. Bilan go/no-go sur D1 à D5, puis validation de Samy avant le J1.

**Règles** : annoncer le coût Higgsfield avant chaque lot ; demander avant toute installation ou tout push hors de la branche en cours ; dialogues dans le parler de cyclope_heritier (glossaire).
