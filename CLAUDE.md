# CLAUDE.md – Règles tokens (détail : docs/CHECKLIST-TOKENS.md)

- **Concision** : réponses courtes, mises à jour ciblées (jamais de régénération complète). Planifier avant d'exécuter.
- **Conversations** : 15–20 messages max, puis fiche projet (300–400 tokens) et nouveau chat. Le suggérer à l'utilisateur.
- **Prompts** : une tâche = un prompt structuré (contexte, objectifs, livrables). Contexte flou → poser une question.
- **Documents** : PDF → Markdown ; docs réutilisés dans le Projet (RAG), rangés en sous-dossiers.
- **Modèles** : Haiku = simple/massif · Sonnet = intermédiaire · Opus = architecture, gros refactor, audits. Sessions lourdes en off-peak (minuit–14 h Paris).
- **Outils** : connecteurs/MCP/skills désactivés sauf besoin ; skills courts à déclenchement strict ; Caveman pour les grosses sessions de code.
