# Créer Yanis dans VRoid Studio

Référence visuelle : la fiche Higgsfield retenue `char_yanis_street_a` (lien dans docs/asset_manifest.json). Couleurs et repères : docs/style_bible.md.

## 1. Installer
VRoid Studio est gratuit : vroid.com/studio ou Steam. Les deux versions sont identiques.

## 2. Partir d'une base
Nouveau modèle, puis base **masculine**. Ouvre la fiche de Yanis sur un second écran ou à côté de la fenêtre.

## 3. Visage (onglet Visage)
- Visage adulte, pas de « babyface » : yeux un peu moins grands que la valeur par défaut, mâchoire plus marquée.
- Expression de base décontractée, sourcils légèrement froncés, l'air malin.
- Teint et traits fidèles à la fiche : on caricature l'attitude et les fringues, jamais les traits (lignes rouges de la SPEC).

## 4. Cheveux (onglet Cheveux)
Reprends la coupe de la fiche. Les presets de coupes courtes suffisent, il suffit d'ajuster la longueur et le volume.

## 5. Corps (onglet Corps)
Style caricatural (environ 5 têtes de haut) : tête un peu plus grosse, jambes un peu plus courtes, mains plus grandes. VRoid limite les extrêmes, donc pousse les curseurs sans casser la silhouette.

## 6. Tenue (onglet Tenue)
- Haut : coupe-vent ou hoodie, **orange coucher de soleil**, avec des bandes réfléchissantes claires (tu peux les peindre dans l'éditeur de texture du vêtement).
- Bas : jogging sombre. Chaussures : grosses baskets, plus casquette si la fiche en a une.
- Aucun logo ni texte, ou alors le lion de Lyon (marque parodique).
- Le sac de livraison bleu canard sera un accessoire 3D séparé : ne le fais pas dans VRoid.

## 7. Exporter
Bouton Exporter, puis **VRM 1.0** :
- Réduction de polygones : vise environ **30 000 triangles**.
- Atlas de textures : **1024**. Coche la fusion des matériaux.
- Nom du fichier : `hero_yanis.vrm`, enregistré dans `Street Mythos\StreetMythos\Assets\_Project\Art\Incoming\`.

L'import vérifie le budget tout seul et signale ce qui dépasse. Les animations viendront ensuite de Mixamo : je te donnerai la liste des clips.
