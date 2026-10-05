// Acte 1, la Croix-Rousse (SPEC § 3.5). Ton : glossaire lyonnais, lignes rouges § 3.6.
// Tags : # speaker:<id> pour l'orateur, # combat:<rencontre> pour lancer un combat, # quete:<id>.

VAR reput = 0
VAR vu_guignol = false

=== rencontre_ines_momo ===
# speaker:yanis
Wesh, il est quelle heure ? Je suis encore à la bourre pour la livraison du kebab…
# speaker:momo
Yanis ! Ta commande, c'est pour moi, pélo. Et t'as vingt minutes de retard.
# speaker:yanis
La montée de la Grande Côte, frère. Elle m'a fané.
# speaker:ines
Vous deux, vous avez vu la brume ? Elle est pas normale. Elle bouge toute seule.
# speaker:momo
C'est cher bizarre, ouais. Et les pigeons de la place… ils ont les yeux qui brillent.
* [Rigoler] -> rigoler
* [S'approcher des pigeons] -> approcher

= rigoler
# speaker:yanis
Des pigeons fluo ? Arrête de balnaver, Momo.
-> pigeons

= approcher
# speaker:yanis
Attends… Chabe-moi ça. Ils nous regardent.
~ reput += 1
-> pigeons

= pigeons
# speaker:ines
Euh… ils nous foncent dessus, là.
# speaker:momo
Mettez-vous derrière moi. Le kebab, il ferme pas pour des pigeons.
# combat:q1_tuto_pigeons
-> DONE

=== apres_pigeons ===
# speaker:ines
On vient de se battre contre des pigeons. Des pigeons POSSÉDÉS.
# speaker:yanis
Je suis refait. Et j'ai même pas fait tomber la commande.
# speaker:momo
Y a cher moyen que ce soit que le début. Venez, on passe par la traboule.
-> DONE

=== traboule_guignol ===
~ vu_guignol = true
# speaker:guignol
Ah ! Des gones ! Enfin du monde qui me voit !
# speaker:yanis
C'est… une marionnette qui parle ?
# speaker:guignol
Une marionnette, une marionnette… Je suis Guignol, mon p'tit ! Né chez les canuts, j'ai fait rire tout Lyon avant que ta grand-mère sache marcher.
# speaker:ines
Et vous êtes quoi, maintenant ? Un fantôme ?
# speaker:guignol
Un esprit, ma fenotte. Comme les légendes que la brume réveille. Sauf que moi, je suis du bon côté.
# speaker:guignol
Écoutez bien : ces bestioles, on les cogne, mais surtout on les CHAMBRE. Une légende sans moral, c'est une légende qui doute. Et une légende qui doute, elle se fait déstabiliser.
# speaker:momo
Donc on les vanne ?
# speaker:guignol
Tu as tout compris, le costaud. Clash, Chambrage ou Mytho : chaque bestiole craint un type. Écoutez ce qu'elles disent, ça trahit toujours.
# speaker:guignol
Et attendre son tour pour se battre… quelle idée ridicule. Mais bon, c'est la règle, paraît-il.
-> DONE

=== avant_gros_caillou ===
# speaker:ines
Le Gros Caillou… Il bouge.
# speaker:gros_caillou
Qui ose piétiner MON boulevard ?
# speaker:yanis
Wesh, on passe juste, on livre un kebab.
# speaker:gros_caillou
Des siècles que je suis là. Des siècles qu'on me traite de « gros ». Ça suffit !
# speaker:momo
Il est susceptible, le pélo. Je sens que la tchatche va marcher.
# combat:q1_boss_gros_caillou
-> DONE

=== fin_j1 ===
# speaker:guignol
Bravo, les gones. La Croix-Rousse respire. Mais la brume descend vers la Saône…
# speaker:ines
Alors on descend aussi.
# speaker:yanis
À suivre, comme on dit.
-> DONE
