namespace StreetMythos.Battle
{
    // État d'un combattant pendant un combat
    public sealed class BattleUnit
    {
        public readonly UnitDef Def;
        public readonly int Index; // ordre d'entrée, sert à départager les égalités

        public int Hp;
        public int Moral = Rules.MaxMoral;
        public int Flow;
        public double NextTurnAt;

        public int DefendTurns;     // en défense tant que > 0
        public int EnragedTurns;
        public int DestabilizedTurns;
        public bool SkipNextTurn;   // déstabilisé ou étourdi : perd son prochain tour
        public int TauntTurns;
        public int SpdBuffPercent;
        public int SpdBuffTurns;

        public BattleUnit(UnitDef def, int index)
        {
            Def = def;
            Index = index;
            Hp = def.Stats.MaxHp;
        }

        public string Id => Def.Id;
        public Team Team => Def.Team;
        public bool IsAlive => Hp > 0;
        public bool IsDefending => DefendTurns > 0;
        public bool IsDestabilized => DestabilizedTurns > 0;

        public int Speed => System.Math.Max(1, Def.Stats.Spd * (100 + SpdBuffPercent) / 100);

        public float AttackValue => Def.Stats.Atk * (EnragedTurns > 0 ? 1f + Rules.EnragedAtkBonus : 1f);

        // Délai avant le prochain tour : délai de base × 100 / VIT (SPEC § 4.3)
        public double DelayFor(int baseDelay) => baseDelay * 100.0 / Speed;

        public void GainFlow(int amount)
        {
            Flow = System.Math.Min(Rules.MaxFlow, Flow + amount);
        }

        // Décompte des effets à la fin du tour de l'unité
        public void TickEndOfTurn()
        {
            if (DefendTurns > 0) DefendTurns--;
            if (EnragedTurns > 0) EnragedTurns--;
            if (TauntTurns > 0) TauntTurns--;
            if (SpdBuffTurns > 0 && --SpdBuffTurns == 0) SpdBuffPercent = 0;
            if (DestabilizedTurns > 0 && --DestabilizedTurns == 0) Moral = Rules.MoralAfterDestabilized;
        }
    }
}
