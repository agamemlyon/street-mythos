// Vannes de tchatche (SPEC § 4.5) : 3 par héros et par type, soit 27.
// Ton : parler lyonnais façon cyclope_heritier (docs/glossaire_lyonnais.md), lignes rouges de la SPEC § 3.6.
// Le jeu appelle le nœud héros_type ; {~…} tire une réplique au hasard.

=== yanis_clash ===
{~« Wesh pélo, je livre en douze minutes et toi tu tombes en trois. »|« T'es cher pas prêt, je t'ai rodave depuis la place. »|« Même la brume te met un vent, alors moi… »}
-> DONE

=== yanis_chambrage ===
{~« Tu te la racles, mais t'es fané depuis le début, ça se voit. »|« Chabe-moi ce style : on dirait une gâche de parking oubliée. »|« T'es pas une légende, t'es une rumeur de comptoir. »}
-> DONE

=== yanis_mytho ===
{~« Attention, derrière toi : Guignol avec son bâton ! … Non, je balnave. Mais t'as regardé. »|« J'ai livré le Gros Caillou ce matin, il dit que t'es son pire pote. »|« Ma prochaine commande, c'est toi. Livraison express, signée, tamponnée. »}
-> DONE

=== ines_clash ===
{~« Objection : ta présence ici est irrecevable. »|« Je plaide, tu perds. Le public est d'accord, la brume aussi. »|« T'as pas d'arguments, t'as que du vent. Et le vent, à Lyon, on connaît. »}
-> DONE

=== ines_chambrage ===
{~« Tu fais peur ? Mon prof de droit des contrats fait plus peur, et lui il sourit. »|« Regarde-toi, t'es tchalé de ta propre brume. »|« Une légende ? Une légende urbaine que personne ne croit, oui. »}
-> DONE

=== ines_mytho ===
{~« J'ai ton dossier complet. Article 69, alinéa Croix-Rousse : tu dégages. »|« La Métropole t'a classé monument fané. C'est officiel, j'ai le tampon. »|« J'ai déjà gagné ton procès en appel. T'étais même pas convoqué. »}
-> DONE

=== momo_clash ===
{~« Viens, viens. Dix ans de boxe contre dix ans de brume, on va rire. »|« Je t'emballe comme un kebab : salade, tomate, oignons, K.-O. »|« Y a cher moyen que tu finisses en garniture. »}
-> DONE

=== momo_chambrage ===
{~« Ma broche tourne plus vite que toi, pélo. »|« T'es pas méchant, t'as juste faim. Passe au kebab, je te fais moins vingt pour cent. »|« Il croit qu'il fait peur avec ses yeux qui brillent. Mes néons aussi, ils brillent. »}
-> DONE

=== momo_mytho ===
{~« Fais gaffe, ma sauce blanche est hantée. Une goutte et t'es possédé. »|« J'ai mis un K.-O. à un dragon l'an dernier. Il s'en souvient encore. »|« Le Gros Caillou, c'est mon voisin. Il me doit des sous. »}
-> DONE

// Répliques des ennemis selon le résultat de la vanne (efficace, neutre, ratee)

=== pigeon_possede_efficace ===
{~« Rrrou… » (les plumes en berne)|« Rrr… rou ? » (il regarde ailleurs, vexé)}
-> DONE
=== pigeon_possede_neutre ===
« Rrrou ? »
-> DONE
=== pigeon_possede_ratee ===
« RRROUUU ! » (il se gonfle, vénère)
-> DONE

=== brumeux_efficace ===
« Ssshh… » (la brume se dissipe un peu)
-> DONE
=== brumeux_neutre ===
« Sssssh. »
-> DONE
=== brumeux_ratee ===
« SSSHHHAAA ! » (la brume s'épaissit)
-> DONE

=== lion_de_pierre_efficace ===
« Grr… Ma crinière… elle est si mal taillée que ça ? »
-> DONE
=== lion_de_pierre_neutre ===
« Grrr. »
-> DONE
=== lion_de_pierre_ratee ===
« Un lion de Lyon ne recule JAMAIS. »
-> DONE

=== controleur_fantome_efficace ===
{~« Votre… votre titre est… valide ? »|« Je… je dois vérifier avec la centrale. »}
-> DONE
=== controleur_fantome_neutre ===
« Titre de transport, s'il vous plaît. »
-> DONE
=== controleur_fantome_ratee ===
« Amende majorée ! Et pour les trois ! »
-> DONE

=== gros_caillou_efficace ===
{~« Je… je suis pas GROS. Je suis CARACTÉRIEL. »|« Personne ne m'avait jamais parlé comme ça… »}
-> DONE
=== gros_caillou_neutre ===
« Hmpf. Je suis là depuis l'ère glaciaire, petit. »
-> DONE
=== gros_caillou_ratee ===
« On ne parle pas comme ça à un monument historique ! »
-> DONE
