namespace StreetMythos.Battle
{
    // Événements émis par le modèle ; la présentation (animations, UI, sons) les écoute
    public abstract class BattleEvent { }

    public sealed class TurnStarted : BattleEvent { public BattleUnit Unit; }
    public sealed class TurnSkipped : BattleEvent { public BattleUnit Unit; }

    // Instant d'impact d'un coup ennemi : la présentation ouvre ici la fenêtre de réaction
    public sealed class HitIncoming : BattleEvent { public BattleUnit Attacker, Target; public int HitIndex, HitCount; }

    public sealed class Damaged : BattleEvent
    {
        public BattleUnit Source, Target;
        public int Amount;
        public bool Critical;
        public Reaction Reaction;
    }

    public sealed class Healed : BattleEvent { public BattleUnit Target; public int Amount; }
    public sealed class MoralChanged : BattleEvent { public BattleUnit Target; public int Delta; public VanneType? Vanne; public string Result; }
    public sealed class Destabilized : BattleEvent { public BattleUnit Target; }
    public sealed class Enraged : BattleEvent { public BattleUnit Target; }
    public sealed class Stunned : BattleEvent { public BattleUnit Target; }
    public sealed class Knocked : BattleEvent { public BattleUnit Target; }
    public sealed class PushedBack : BattleEvent { public BattleUnit Target; public int Places; }
    public sealed class FlowChanged : BattleEvent { public BattleUnit Unit; public int Flow; }
    public sealed class BattleEnded : BattleEvent { public BattleOutcome Outcome; }
}
