# Installation du poste de Samy (J0)

Ce qui est déjà en place le 4 octobre 2026 : Git 2.55, Git LFS 3.7, Node 24, winget. Disque C: : environ 25 Go libres, à surveiller (Unity et ses modules en prennent 12 à 15).

## Ce que Claude fait, après ton accord

1. Il installe Unity Hub : `winget install Unity.UnityHub`.
2. Il installe Unity 6 LTS et le module Web Build Support par la ligne de commande du Hub.
3. Il crée le projet URP dans `Street Mythos\StreetMythos`, puis le second clone de compilation `Street Mythos\StreetMythos-build` (TECH_DESIGN 5.4).
4. Il active Git LFS dans le dépôt (`git lfs install`).

## Ce que toi seul peux faire

1. **Licence Unity** : ouvre Unity Hub, connecte-toi à ton compte Unity (ou crée-le), puis active une licence Personal gratuite (Préférences > Licences > Ajouter).
2. **VRoid Studio** (gratuit, sur le Microsoft Store ou vroid.com) : crée Yanis à partir de sa fiche Higgsfield retenue, puis exporte-le en `.vrm` dans `Assets/_Project/Art/Incoming/hero_yanis.vrm`. Vise 30 000 triangles et des textures en 1024 (option de réduction à l'export).
3. **Mixamo** (gratuit, compte Adobe) : Claude te donnera la liste exacte des clips à télécharger.
4. **itch.io**, avant la première publication : crée un compte, puis une page de jeu en mode brouillon, type HTML. Ensuite, génère une clé d'API (Paramètres > Clés d'API) et enregistre-la toi-même dans la variable d'environnement `BUTLER_API_KEY`. Claude ne la verra pas.

## Commandes utiles

```
node Tools/serve.mjs Builds/Web 8080
Unity -batchmode -projectPath ../StreetMythos-build -runTests -testPlatform EditMode -testResults results.xml
Unity -batchmode -quit -projectPath ../StreetMythos-build -executeMethod StreetMythos.Editor.BuildScript.BuildWeb
```
