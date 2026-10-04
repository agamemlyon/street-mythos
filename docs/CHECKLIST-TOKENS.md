# Checklist optimisation de tokens – Claude & Claude Code

## 1. Conversations et contexte
- [ ] Limiter chaque chat à 15–20 messages max avant de repartir sur une nouvelle conversation.
- [ ] À la fin d’un « gros » chat, demander :
      « Résume tout ce que nous avons fait d’important sous forme de fiche projet (300–400 tokens max). »
- [ ] Coller cette fiche projet comme contexte de démarrage dans le nouveau chat.
- [ ] Éditer les prompts existants plutôt que créer un nouveau message pour corriger une erreur.

## 2. Pièces jointes (PDF, PPT, specs)
- [ ] Convertir les PDF volumineux en Markdown (.md) avant de les envoyer à Claude.
- [ ] Importer les documents réutilisés (design system, specs produit, docs techniques) dans un Projet Claude.
- [ ] Structurer le projet avec des sous-dossiers pour éviter qu’un dossier unique ait trop de fichiers.
- [ ] Vérifier que les tâches d’analyse de documents passent par le Projet (RAG) plutôt que par un chat « classique ».

## 3. Mémoire et préférences
- [ ] Configurer une mémoire courte et ciblée (métier, style d’écriture, objectifs).
- [ ] Indiquer dans les instructions globales :
      « Sois conscient que je veux économiser au maximum mes tokens, sois concis dans tes réponses
       et suggère-moi quand je dois créer une nouvelle conversation. »
- [ ] Désactiver la génération automatique de nouveaux souvenirs à partir de l’historique si non indispensable.

## 4. Structuration des prompts
- [ ] Grouper toutes les demandes liées à une même tâche dans un seul prompt structuré (contexte, objectifs, livrables).
- [ ] Demander systématiquement une étape de planification avant l’exécution (surtout pour Claude Code / Cowork).
- [ ] Pour les modifications de texte ou de code, demander seulement une mise à jour ciblée
      sur la section concernée, pas une régénération complète.
- [ ] Utiliser AskUserQuestion lorsque le contexte est flou pour éviter des prompts trop verbeux.

## 5. Modèles et horaires
- [ ] Utiliser Haiku pour les tâches simples et massives (résumés, extraction d’info, petits scripts).
- [ ] Utiliser Sonnet pour les tâches intermédiaires (design d’API, specs + code, docs un peu complexes).
- [ ] Réserver Opus aux tâches à fort raisonnement (architecture, refactoring complexe, gros audits).
- [ ] Planifier les sessions lourdes sur les plages off-peak (par ex. minuit–14 h en heure de Paris).

## 6. Skills, connecteurs et artefacts
- [ ] Désactiver tous les connecteurs et MCP non essentiels par défaut.
- [ ] Activer un skill ou connecteur uniquement pour les chats qui en ont besoin.
- [ ] Raccourcir les fichiers de règles des skills (instructions minimalistes, très peu d’exemples).
- [ ] Définir des conditions de déclenchement strictes pour chaque skill
      (ex. se déclencher uniquement si le prompt contient « email », « YouTube title », etc.).
- [ ] Désactiver les artefacts et visualisations intégrées si tu n’utilises pas ces sorties.

## 7. Caveman et Claude Code
- [ ] Installer Caveman dans Claude Code :
      `claude plugin marketplace add JuliusBrussee/caveman && claude plugin install caveman@caveman`
- [ ] L’utiliser pour les sessions de coding où les explications sont très longues
      (gros contextes, refactoring massif, pas pour les petites questions ponctuelles).
- [ ] Vérifier la lisibilité des réponses caveman pour ton équipe (devs, designers, stakeholders).
- [ ] Compléter Caveman par des patterns de travail « token-discipline »
      (investigate-first, surgical-patch, safe-refactor, etc.).

## 8. Monitoring et gouvernance
- [ ] Surveiller régulièrement la consommation de tokens par modèle (Haiku / Sonnet / Opus).
- [ ] Identifier les chats qui explosent les tokens et les archiver en créant une fiche projet + nouveau chat.
- [ ] Mettre à jour une page « bonnes pratiques Claude » dans la doc du Quartz Design System.
- [ ] Former l’équipe (design, dev, UX) à ces pratiques pour homogénéiser les usages.
