using System.Collections.Generic;

namespace StreetMythos.Battle
{
    // Règles de combat de la SPEC § 4.3 à 4.6. Les valeurs de réglage sont regroupées ici.
    public static class Rules
    {
        public const int TimelinePreview = 8;
        public const int MaxFlow = 10;
        public const int MaxMoral = 100;

        public const int DelayAttack = 100;
        public const int DelayTchatche = 80;
        public const int DelayItem = 80;
        public const int DelayDefend = 60;

        public const float CritChance = 0.05f;
        public const float CritMultiplier = 1.5f;
        public const float DestabilizedMultiplier = 1.5f;
        public const float DefendingMultiplier = 0.5f;
        public const float EnragedAtkBonus = 0.10f;

        public const int MoralEffective = 40;
        public const int MoralNeutral = 20;
        public const int MoralFailed = 5;
        public const int MoralAfterDestabilized = 50;
        public const int EnragedTurns = 2;
        public const int BossBrokenTurns = 2;

        public const int FlowOnHit = 1;
        public const int FlowOnEffectiveVanne = 1;
        public const int FlowOnParry = 2;
        public const int ParryCounterPower = 50;
    }

    public enum Team { Heroes, Enemies }

    public enum VanneType { Clash, Chambrage, Mytho }

    public enum Reaction { None, Dodge, Parry }

    public enum TargetKind { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }

    public enum BattleOutcome { Ongoing, Victory, Defeat, Fled }

    public sealed class Stats
    {
        public int MaxHp, Atk, Def, Spd;

        public Stats(int maxHp, int atk, int def, int spd)
        {
            MaxHp = maxHp; Atk = atk; Def = def; Spd = spd;
        }
    }

    // Compétence décrite par des données (Data/Json), sans code spécifique
    public sealed class SkillDef
    {
        public string Id;
        public string Name;
        public int Delay = 120;
        public int FlowCost = 2;
        public TargetKind Target = TargetKind.SingleEnemy;
        public int Power;              // puissance par coup, 0 = pas de dégâts
        public int Hits = 1;
        public int MoralDamage;        // retiré au Moral de la ou des cibles
        public int HealPercent;        // soin en % des PV max
        public int FlowGrant;          // Flow donné à l'allié ciblé
        public int PushBack;           // places perdues sur la timeline par la cible
        public int TauntTurns;         // provocation : les ennemis visent le lanceur
        public int DefendTurns;        // garde prolongée
        public int StunIfMoralBelow;   // étourdit si le Moral de la cible est sous ce seuil
        public int TeamSpdBuffPercent; // bonus de VIT pour l'équipe
        public int BuffTurns;
    }

    // Une attaque ennemie : un ou plusieurs coups, chacun appelant une réaction du joueur
    public sealed class EnemyMove
    {
        public string Id;
        public string Name;
        public int Delay = 100;
        public int Power = 100;
        public int Hits = 1;
        public bool TargetsAll;
    }

    public sealed class UnitDef
    {
        public string Id;
        public string Name;
        public Team Team;
        public Stats Stats;
        public bool IsBoss;
        public bool IsElite;
        public bool IsInes;                     // +50 % de dégâts de Moral (SPEC § 4.5)
        public VanneType? Weakness;             // vanne efficace
        public VanneType? Resistance;           // vanne ratée
        public List<SkillDef> Skills = new List<SkillDef>();
        public List<EnemyMove> Moves = new List<EnemyMove>();
    }
}
