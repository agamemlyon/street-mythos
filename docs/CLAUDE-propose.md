# CLAUDE.md · Street Mythos : Légendes du 69 (JRPG 3D, démo web Unity)

Lis ce fichier en premier, puis SPEC.md et TECH_DESIGN.md (dans ce Projet : docs/street-mythos-3d/ ; dans le dépôt Unity : docs/). Pour le détail, ces deux docs priment sur ce fichier. Une demande explicite de Samy prime sur tout.

## Règles tokens (détail : docs/CHECKLIST-TOKENS.md)

- **Concision** : réponses courtes, mises à jour ciblées (jamais de régénération complète). Planifier avant d'exécuter.
- **Conversations** : 15–20 messages max, puis fiche projet (300–400 tokens) et nouveau chat. Le suggérer à l'utilisateur.
- **Prompts** : une tâche = un prompt structuré (contexte, objectifs, livrables). Contexte flou → poser une question.
- **Documents** : PDF → Markdown ; docs réutilisés dans le Projet (RAG), rangés en sous-dossiers.
- **Modèles** : Haiku = simple/massif · Sonnet = intermédiaire · Opus = architecture, gros refactor, audits. Sessions lourdes en off-peak (minuit–14 h Paris).
- **Outils** : connecteurs/MCP/skills désactivés sauf besoin ; skills courts à déclenchement strict ; Caveman pour les grosses sessions de code.

## Le projet

- JRPG 3D au tour par tour en cel-shading. Lyon de nuit, où une brume réveille les légendes oubliées. Humour de quartier façon Lascars.
- Démo : mini-chapitre de 45 minutes, la Croix-Rousse puis les quais de Saône et le Vieux-Lyon, équipe de 3 (Yanis, Inès, Momo), 8 combats dont 2 boss (Gros Caillou, Mâchecroute).
- Combat : timeline façon FFX, esquive et parade pendant les tours ennemis, jauge de tchatche.
- Le résumé de départ d'un nouveau fil est la fiche projet en tête de SPEC.md.

## Stack figée

- Unity 6 LTS, URP, build Web (WebGL 2), C#. Navigateurs de bureau uniquement.
- Paquets autorisés : Input System, Cinemachine, Addressables, UI Toolkit, Timeline, Ink, Unity Test Framework, UniVRM. Tout autre paquet demande l'accord de Samy.
- Interdits : HDRP, Unreal, Pixel Streaming, ray tracing, éclairage global temps réel.

## Où tourne quoi

- Unity tourne sur le PC de Samy. Claude y travaille via une session Remote Control, et compile ou teste en batchmode dans le clone StreetMythos-build, jamais dans le clone ouvert dans l'éditeur.
- Un fil cloud écrit code, données, dialogues et docs, puis pousse sur une branche. Il ne prétend jamais avoir compilé ou testé dans Unity.

## Règles de code et de contenu

- Noms en anglais, commentaires en français, courts et utiles.
- La logique de combat reste en C# pur, testée en EditMode. Aucune PR ne touche Scripts/Battle/ sans tests verts.
- Données en JSON (Data/Json), dialogues et vannes en Ink. Aucun texte de jeu en dur dans le code.
- Tout asset entre par Art/Incoming, passe le contrôle de budget et figure dans docs/asset_manifest.json avec sa licence.
- Chaque texte respecte les lignes rouges de SPEC § 3.6.

## Workflow

1. Plan court, puis exécution. Rien hors SPEC sans demande de Samy.
2. Une tâche par branche, une PR courte, main toujours buildable.
3. Avant de pousser : compilation, tests EditMode, et si le runtime change, build Web, test navigateur et budgets (TECH_DESIGN § 3).
4. Génération Higgsfield ou achat d'asset : annoncer le coût et attendre l'accord de Samy.
5. Jalons J0, J1, J2 dans l'ordre. Samy valide chaque jalon avant le suivant.

## Fin de la démo

Les critères sont dans SPEC § 6.2. Tant qu'un critère n'est pas vrai, la démo n'est pas finie.
