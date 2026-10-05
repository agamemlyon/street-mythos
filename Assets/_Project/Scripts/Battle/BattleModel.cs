using System;
using System.Collections.Generic;
using System.Linq;

namespace StreetMythos.Battle
{
    // Réaction du joueur à un coup ennemi. La présentation la fournit à partir des entrées
    // horodatées (ReactionWindow) ; les tests la simulent.
    public interface IReactionSource
    {
        Reaction React(BattleUnit attacker, BattleUnit target, int hitIndex);
    }

    public enum Opening { Normal, HeroesFirst, EnemiesFirst }

    // Modèle de combat déterministe à graine fixe, sans dépendance à Unity (TECH_DESIGN 4.3)
    public sealed class BattleModel
    {
        readonly List<BattleUnit> _units = new List<BattleUnit>();
        readonly BattleRandom _rng;

        public event Action<BattleEvent> Emitted;
        public readonly List<BattleEvent> Log = new List<BattleEvent>();

        public BattleOutcome Outcome { get; private set; } = BattleOutcome.Ongoing;
        public IReadOnlyList<BattleUnit> Units => _units;
        public IEnumerable<BattleUnit> Heroes => _units.Where(u => u.Team == Team.Heroes);
        public IEnumerable<BattleUnit> Enemies => _units.Where(u => u.Team == Team.Enemies);
        public BattleUnit Current { get; private set; }

        public BattleModel(IEnumerable<UnitDef> heroes, IEnumerable<UnitDef> enemies, uint seed, Opening opening = Opening.Normal)
        {
            _rng = new BattleRandom(seed);
            foreach (var d in heroes.Concat(enemies)) _units.Add(new BattleUnit(d, _units.Count));

            // Premier tour gratuit : le camp qui surprend joue avant l'autre (SPEC § 4.2)
            foreach (var u in _units)
            {
                double start = u.DelayFor(Rules.DelayAttack) / 2;
                bool late = (opening == Opening.HeroesFirst && u.Team == Team.Enemies)
                         || (opening == Opening.EnemiesFirst && u.Team == Team.Heroes);
                u.NextTurnAt = late ? start + 1000 : start;
            }
        }

        // ---------- Timeline ----------

        static int Order(BattleUnit a, BattleUnit b)
        {
            int c = a.NextTurnAt.CompareTo(b.NextTurnAt);
            if (c != 0) return c;
            c = b.Speed.CompareTo(a.Speed);          // à égalité, le plus rapide d'abord
            return c != 0 ? c : a.Index.CompareTo(b.Index);
        }

        List<BattleUnit> AliveOrdered()
        {
            var list = _units.Where(u => u.IsAlive).ToList();
            list.Sort(Order);
            return list;
        }

        // Les prochains tours affichés sur la timeline, en supposant des actions de délai 100
        public List<BattleUnit> PreviewTimeline(int count = Rules.TimelinePreview)
        {
            var next = _units.Where(u => u.IsAlive).ToDictionary(u => u, u => u.NextTurnAt);
            var result = new List<BattleUnit>();
            while (result.Count < count && next.Count > 0)
            {
                var u = next.Keys.OrderBy(k => next[k]).ThenByDescending(k => k.Speed).ThenBy(k => k.Index).First();
                result.Add(u);
                next[u] += u.DelayFor(Rules.DelayAttack);
            }
            return result;
        }

        // Passe au prochain combattant qui peut agir (les tours perdus sont consommés ici)
        public BattleUnit NextActor()
        {
            while (Outcome == BattleOutcome.Ongoing)
            {
                var u = AliveOrdered().First();
                if (u.SkipNextTurn)
                {
                    u.SkipNextTurn = false;
                    Emit(new TurnSkipped { Unit = u });
                    u.TickEndOfTurn();
                    u.NextTurnAt += u.DelayFor(Rules.DelayAttack);
                    continue;
                }
                Current = u;
                Emit(new TurnStarted { Unit = u });
                return u;
            }
            return null;
        }

        void EndTurn(BattleUnit actor, int baseDelay)
        {
            actor.TickEndOfTurn();
            actor.NextTurnAt += actor.DelayFor(baseDelay);
            Current = null;
            CheckOutcome();
        }

        // ---------- Actions des héros ----------

        public void Attack(BattleUnit target)
        {
            var actor = RequireHeroTurn();
            if (DealDamage(actor, target, 100, Reaction.None) > 0) GainFlow(actor, Rules.FlowOnHit);
            EndTurn(actor, Rules.DelayAttack);
        }

        public bool CanUse(SkillDef skill) => Current != null && Current.Flow >= skill.FlowCost;

        public void UseSkill(SkillDef skill, BattleUnit target)
        {
            var actor = RequireHeroTurn();
            if (actor.Flow < skill.FlowCost) throw new InvalidOperationException($"Flow insuffisant pour {skill.Id}");
            actor.Flow -= skill.FlowCost;
            Emit(new FlowChanged { Unit = actor, Flow = actor.Flow });

            var targets = ResolveTargets(actor, skill.Target, target);
            bool hit = false;
            foreach (var t in targets)
            {
                for (int h = 0; h < skill.Hits && skill.Power > 0 && t.IsAlive; h++)
                    hit |= DealDamage(actor, t, skill.Power, Reaction.None) > 0;
                if (skill.MoralDamage > 0 && t.IsAlive) ApplyMoral(actor, t, skill.MoralDamage, null, "compétence");
                if (skill.StunIfMoralBelow > 0 && t.IsAlive && t.Moral < skill.StunIfMoralBelow)
                {
                    t.SkipNextTurn = true;
                    Emit(new Stunned { Target = t });
                }
                if (skill.PushBack > 0 && t.IsAlive) PushBack(t, skill.PushBack);
                if (skill.HealPercent > 0) Heal(t, t.Def.Stats.MaxHp * skill.HealPercent / 100);
                if (skill.FlowGrant > 0) GainFlow(t, skill.FlowGrant);
                if (skill.TeamSpdBuffPercent > 0) { t.SpdBuffPercent = skill.TeamSpdBuffPercent; t.SpdBuffTurns = skill.BuffTurns; }
            }
            if (skill.TauntTurns > 0) actor.TauntTurns = skill.TauntTurns + 1;   // +1 : le tour en cours est décompté
            if (skill.DefendTurns > 0) actor.DefendTurns = skill.DefendTurns + 1;
            if (hit) GainFlow(actor, Rules.FlowOnHit);
            EndTurn(actor, skill.Delay);
        }

        // Vanne de tchatche (SPEC § 4.5)
        public void Tchatche(BattleUnit target, VanneType vanne)
        {
            var actor = RequireHeroTurn();
            int amount; string result;
            if (target.Def.Weakness == vanne) { amount = Rules.MoralEffective; result = "efficace"; }
            else if (target.Def.Resistance == vanne) { amount = Rules.MoralFailed; result = "ratée"; }
            else { amount = Rules.MoralNeutral; result = "neutre"; }

            ApplyMoral(actor, target, amount, vanne, result);
            if (result == "efficace") GainFlow(actor, Rules.FlowOnEffectiveVanne);
            if (result == "ratée" && target.IsAlive)
            {
                target.EnragedTurns = Rules.EnragedTurns;
                Emit(new Enraged { Target = target });
            }
            EndTurn(actor, Rules.DelayTchatche);
        }

        public void Defend()
        {
            var actor = RequireHeroTurn();
            actor.DefendTurns = 2; // jusqu'à son prochain tour (décompté une fois maintenant)
            EndTurn(actor, Rules.DelayDefend);
        }

        public void UseItem(int healPercent, BattleUnit target)
        {
            var actor = RequireHeroTurn();
            Heal(target, target.Def.Stats.MaxHp * healPercent / 100);
            EndTurn(actor, Rules.DelayItem);
        }

        public bool CanFlee => !Enemies.Any(e => e.IsAlive && (e.Def.IsBoss || e.Def.IsElite));

        public bool Flee()
        {
            RequireHeroTurn();
            if (!CanFlee) return false;
            Outcome = BattleOutcome.Fled;
            Emit(new BattleEnded { Outcome = Outcome });
            return true;
        }

        // ---------- Tour ennemi ----------

        // Tour ennemi d'un coup, réactions fournies au fil de l'eau (tests, simulations)
        public void RunEnemyTurn(IReactionSource reactions)
        {
            var turn = StartEnemyTurn();
            while (turn.NextHit(out var hit))
                turn.Resolve(reactions?.React(hit.Attacker, hit.Target, hit.HitIndex) ?? Reaction.None);
            turn.Finish();
        }

        // Tour ennemi pas à pas : la présentation attend l'instant d'impact de chaque coup
        // et la réaction du joueur avant d'appeler Resolve
        public EnemyTurn StartEnemyTurn()
        {
            var actor = Current;
            if (actor == null || actor.Team != Team.Enemies) throw new InvalidOperationException("Ce n'est pas le tour d'un ennemi");

            var move = actor.Def.Moves.Count > 0
                ? actor.Def.Moves[_rng.Range(0, actor.Def.Moves.Count)]
                : new EnemyMove { Id = "attack", Name = "Attaque" };

            var heroes = Heroes.Where(h => h.IsAlive).ToList();
            var taunting = heroes.Where(h => h.TauntTurns > 0).ToList();
            var targets = move.TargetsAll ? heroes
                        : new List<BattleUnit> { taunting.Count > 0 ? taunting[0] : heroes[_rng.Range(0, heroes.Count)] };
            return new EnemyTurn(this, actor, move, targets);
        }

        public sealed class EnemyTurn
        {
            readonly BattleModel _m;
            readonly List<BattleUnit> _targets;
            int _targetIndex, _hitIndex;
            HitIncoming _pending;
            bool _finished;

            public readonly BattleUnit Actor;
            public readonly EnemyMove Move;
            public IReadOnlyList<BattleUnit> Targets => _targets;

            internal EnemyTurn(BattleModel m, BattleUnit actor, EnemyMove move, List<BattleUnit> targets)
            {
                _m = m; Actor = actor; Move = move; _targets = targets;
            }

            // Prochain coup à jouer ; faux quand l'attaque est terminée
            public bool NextHit(out HitIncoming hit)
            {
                if (_pending != null) throw new InvalidOperationException("Le coup précédent n'a pas été résolu");
                while (_targetIndex < _targets.Count)
                {
                    var target = _targets[_targetIndex];
                    if (_hitIndex < Move.Hits && target.IsAlive && Actor.IsAlive)
                    {
                        _pending = hit = new HitIncoming { Attacker = Actor, Target = target, HitIndex = _hitIndex, HitCount = Move.Hits };
                        _m.Emit(hit);
                        return true;
                    }
                    _targetIndex++;
                    _hitIndex = 0;
                }
                hit = null;
                return false;
            }

            public void Resolve(Reaction reaction)
            {
                if (_pending == null) throw new InvalidOperationException("Aucun coup en attente");
                var target = _pending.Target;
                switch (reaction)
                {
                    case Reaction.Dodge:
                        _m.Emit(new Damaged { Source = Actor, Target = target, Amount = 0, Reaction = reaction });
                        break;
                    case Reaction.Parry:
                        _m.Emit(new Damaged { Source = Actor, Target = target, Amount = 0, Reaction = reaction });
                        _m.GainFlow(target, Rules.FlowOnParry);
                        _m.DealDamage(target, Actor, Rules.ParryCounterPower, Reaction.None); // contre-attaque
                        break;
                    default:
                        _m.DealDamage(Actor, target, Move.Power, Reaction.None);
                        break;
                }
                _pending = null;
                _hitIndex++;
            }

            public void Finish()
            {
                if (_finished) return;
                if (_pending != null) throw new InvalidOperationException("Un coup n'a pas été résolu");
                _finished = true;
                _m.EndTurn(Actor, Move.Delay);
            }
        }

        // ---------- Règles internes ----------

        BattleUnit RequireHeroTurn()
        {
            if (Outcome != BattleOutcome.Ongoing) throw new InvalidOperationException("Le combat est terminé");
            if (Current == null || Current.Team != Team.Heroes) throw new InvalidOperationException("Ce n'est pas le tour d'un héros");
            return Current;
        }

        List<BattleUnit> ResolveTargets(BattleUnit actor, TargetKind kind, BattleUnit chosen)
        {
            var foes = _units.Where(u => u.IsAlive && u.Team != actor.Team).ToList();
            var allies = _units.Where(u => u.IsAlive && u.Team == actor.Team).ToList();
            switch (kind)
            {
                case TargetKind.AllEnemies: return foes;
                case TargetKind.AllAllies: return allies;
                case TargetKind.Self: return new List<BattleUnit> { actor };
                default: return new List<BattleUnit> { chosen ?? (kind == TargetKind.SingleAlly ? actor : foes.First()) };
            }
        }

        public static int ComputeDamage(float atk, int power, int def, float variance, bool critical, bool destabilized, bool defending)
        {
            // Dégâts = ATQ × puissance / 100 × 100 / (100 + DEF) × aléa (SPEC § 4.3)
            double dmg = atk * power / 100.0 * 100.0 / (100 + def) * variance;
            if (critical) dmg *= Rules.CritMultiplier;
            if (destabilized) dmg *= Rules.DestabilizedMultiplier;
            if (defending) dmg *= Rules.DefendingMultiplier;
            return Math.Max(1, (int)Math.Round(dmg));
        }

        int DealDamage(BattleUnit source, BattleUnit target, int power, Reaction reaction)
        {
            if (!target.IsAlive) return 0;
            float variance = 0.95f + 0.10f * (_rng.Range(0, 1001) / 1000f);
            bool crit = _rng.Range(0, 1000) < Rules.CritChance * 1000;
            int amount = ComputeDamage(source.AttackValue, power, target.Def.Stats.Def, variance, crit, target.IsDestabilized, target.IsDefending);
            target.Hp = Math.Max(0, target.Hp - amount);
            Emit(new Damaged { Source = source, Target = target, Amount = amount, Critical = crit, Reaction = reaction });
            if (!target.IsAlive) Emit(new Knocked { Target = target });
            return amount;
        }

        void ApplyMoral(BattleUnit source, BattleUnit target, int amount, VanneType? vanne, string result)
        {
            if (source.Def.IsInes) amount = amount * 3 / 2;
            int before = target.Moral;
            target.Moral = Math.Max(0, target.Moral - amount);
            Emit(new MoralChanged { Source = source, Target = target, Delta = target.Moral - before, Vanne = vanne, Result = result });
            if (target.Moral == 0 && !target.IsDestabilized)
            {
                // Déstabilisé : perd son prochain tour, prend ×1,5 ; un boss reste Brisé 2 tours
                target.DestabilizedTurns = target.Def.IsBoss ? Rules.BossBrokenTurns : 1;
                target.SkipNextTurn = true;
                Emit(new Destabilized { Target = target });
            }
        }

        void PushBack(BattleUnit target, int places)
        {
            var order = AliveOrdered();
            order.Remove(target);
            int pos = Math.Max(0, AliveOrdered().IndexOf(target));
            int newPos = Math.Min(order.Count - 1, pos + places - 1);
            if (order.Count == 0) return;
            target.NextTurnAt = order[newPos].NextTurnAt + 0.001;
            Emit(new PushedBack { Target = target, Places = places });
        }

        void Heal(BattleUnit target, int amount)
        {
            if (!target.IsAlive) return;
            int before = target.Hp;
            target.Hp = Math.Min(target.Def.Stats.MaxHp, target.Hp + amount);
            Emit(new Healed { Target = target, Amount = target.Hp - before });
        }

        void GainFlow(BattleUnit unit, int amount)
        {
            if (unit.Team != Team.Heroes) return;
            unit.GainFlow(amount);
            Emit(new FlowChanged { Unit = unit, Flow = unit.Flow });
        }

        void CheckOutcome()
        {
            if (Outcome != BattleOutcome.Ongoing) return;
            if (!Enemies.Any(e => e.IsAlive)) Outcome = BattleOutcome.Victory;
            else if (!Heroes.Any(h => h.IsAlive)) Outcome = BattleOutcome.Defeat;
            if (Outcome != BattleOutcome.Ongoing) Emit(new BattleEnded { Outcome = Outcome });
        }

        void Emit(BattleEvent e)
        {
            Log.Add(e);
            Emitted?.Invoke(e);
        }
    }
}
