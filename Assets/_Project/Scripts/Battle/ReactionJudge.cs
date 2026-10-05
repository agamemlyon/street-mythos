using System;

namespace StreetMythos.Battle
{
    public enum Assist { None, WideWindows, AutoDodge }

    // Juge une réaction à partir des instants d'appui et de l'instant d'impact, en secondes de temps réel
    // (non mis à l'échelle) : le résultat ne dépend pas de la fréquence d'image (TECH_DESIGN 4.3, SPEC § 4.4)
    public static class ReactionJudge
    {
        public const double DodgeWindow = 0.250; // fenêtre totale, centrée sur l'impact
        public const double ParryWindow = 0.120;

        // Seul le premier appui de chaque touche compte : marteler ne sert à rien
        public static Reaction Judge(double impactTime, double? firstDodgePress, double? firstParryPress, Assist assist)
        {
            double scale = assist == Assist.WideWindows ? 2.0 : 1.0;
            bool Inside(double? press, double window) =>
                press.HasValue && Math.Abs(press.Value - impactTime) <= window * scale / 2;

            if (Inside(firstParryPress, ParryWindow)) return Reaction.Parry;
            if (Inside(firstDodgePress, DodgeWindow)) return Reaction.Dodge;
            return assist == Assist.AutoDodge ? Reaction.Dodge : Reaction.None;
        }
    }
}
