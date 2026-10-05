using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StreetMythos.Battle;

namespace StreetMythos.Tests.EditMode
{
    public class BattleModelTests
    {
        sealed class FixedReaction : IReactionSource
        {
            readonly Reaction _r;
            public int Calls;
            public FixedReaction(Reaction r) => _r = r;
            public Reaction React(BattleUnit a, BattleUnit t, int i) { Calls++; return _r; }
        }

        static UnitDef Hero(string id, int spd = 100, int atk = 50, int hp = 200, bool ines = false) =>
            new UnitDef { Id = id, Name = id, Team = Team.Heroes, Stats = new Stats(hp, atk, 20, spd), IsInes = ines };

        static UnitDef Enemy(string id, int spd = 100, int hp = 100, VanneType? weak = null, VanneType? resist = null, bool boss = false, int hits = 1) =>
            new UnitDef
            {
                Id = id, Name = id, Team = Team.Enemies, Stats = new Stats(hp, 40, 20, spd), IsBoss = boss,
                Weakness = weak, Resistance = resist,
                Moves = { new EnemyMove { Id = "coup", Power = 100, Hits = hits } }
            };

        static BattleUnit U(BattleModel m, string id) => m.Units.First(u => u.Id == id);

        // ---------- Formule de dégâts ----------

        [Test]
        public void DamageFormulaMatchesSpec()
        {
            // 100 × 100 / 100 × 100 / (100 + 0) × 1 = 100
            Assert.AreEqual(100, BattleModel.ComputeDamage(100, 100, 0, 1f, false, false, false));
            // DEF 100 divise par deux
            Assert.AreEqual(50, BattleModel.ComputeDamage(100, 100, 100, 1f, false, false, false));
            Assert.AreEqual(150, BattleModel.ComputeDamage(100, 100, 0, 1f, true, false, false));
            Assert.AreEqual(150, BattleModel.ComputeDamage(100, 100, 0, 1f, false, true, false));
            Assert.AreEqual(50, BattleModel.ComputeDamage(100, 100, 0, 1f, false, false, true));
            Assert.AreEqual(1, BattleModel.ComputeDamage(1, 1, 500, 0.95f, false, false, false), "au moins 1 dégât");
        }

        // ---------- Timeline ----------

        [Test]
        public void FasterUnitActsMoreOften()
        {
            var m = new BattleModel(new[] { Hero("rapide", spd: 200) }, new[] { Enemy("lent", spd: 100) }, 1);
            var preview = m.PreviewTimeline(9);
            Assert.AreEqual(6, preview.Count(u => u.Id == "rapide"));
            Assert.AreEqual(3, preview.Count(u => u.Id == "lent"));
        }

        [Test]
        public void TimelineShowsEightTurns()
        {
            var m = new BattleModel(new[] { Hero("a"), Hero("b") }, new[] { Enemy("e") }, 1);
            Assert.AreEqual(8, m.PreviewTimeline().Count);
        }

        [Test]
        public void ShortActionsComeBackSooner()
        {
            var m = new BattleModel(new[] { Hero("h") }, new[] { Enemy("e", hp: 9999) }, 1);
            var h = m.NextActor();
            double before = h.NextTurnAt;
            m.Defend(); // délai 60
            Assert.AreEqual(before + 60, h.NextTurnAt, 1e-6);
        }

        [Test]
        public void HeroesFirstOpeningGivesHeroesTheFirstTurns()
        {
            var m = new BattleModel(new[] { Hero("a", spd: 50), Hero("b", spd: 50) }, new[] { Enemy("e", spd: 300) }, 1, Opening.HeroesFirst);
            var first = m.PreviewTimeline(2);
            Assert.IsTrue(first.All(u => u.Team == Team.Heroes));
        }

        // ---------- Attaque, Flow, victoire ----------

        [Test]
        public void AttackHitGivesOneFlow()
        {
            var m = new BattleModel(new[] { Hero("h", spd: 200) }, new[] { Enemy("e", hp: 9999) }, 3);
            var h = m.NextActor();
            m.Attack(U(m, "e"));
            Assert.AreEqual(1, h.Flow);
            Assert.Less(U(m, "e").Hp, 9999);
        }

        [Test]
        public void FlowIsCappedAtTen()
        {
            var u = new BattleUnit(Hero("h"), 0);
            u.GainFlow(25);
            Assert.AreEqual(Rules.MaxFlow, u.Flow);
        }

        [Test]
        public void KillingLastEnemyIsVictory()
        {
            var m = new BattleModel(new[] { Hero("h", spd: 300, atk: 500) }, new[] { Enemy("e", hp: 10) }, 5);
            m.NextActor();
            m.Attack(U(m, "e"));
            Assert.AreEqual(BattleOutcome.Victory, m.Outcome);
            Assert.IsTrue(m.Log.OfType<BattleEnded>().Any());
        }

        [Test]
        public void SameSeedSameBattle()
        {
            int Run()
            {
                var m = new BattleModel(new[] { Hero("h") }, new[] { Enemy("e", hp: 500) }, 42);
                var r = new FixedReaction(Reaction.None);
                for (int i = 0; i < 6 && m.Outcome == BattleOutcome.Ongoing; i++)
                {
                    var a = m.NextActor();
                    if (a.Team == Team.Heroes) m.Attack(U(m, "e")); else m.RunEnemyTurn(r);
                }
                return U(m, "e").Hp * 1000 + U(m, "h").Hp;
            }
            Assert.AreEqual(Run(), Run());
        }

        // ---------- Réactions ----------

        static BattleModel EnemyTurnSetup(int hits = 1)
        {
            var m = new BattleModel(new[] { Hero("h", spd: 10) }, new[] { Enemy("e", spd: 300, hp: 9999, hits: hits) }, 7);
            var a = m.NextActor();
            Assert.AreEqual(Team.Enemies, a.Team);
            return m;
        }

        [Test]
        public void DodgeAvoidsAllDamage()
        {
            var m = EnemyTurnSetup();
            m.RunEnemyTurn(new FixedReaction(Reaction.Dodge));
            Assert.AreEqual(200, U(m, "h").Hp);
        }

        [Test]
        public void ParryAvoidsDamageCountersAndGivesTwoFlow()
        {
            var m = EnemyTurnSetup();
            m.RunEnemyTurn(new FixedReaction(Reaction.Parry));
            Assert.AreEqual(200, U(m, "h").Hp);
            Assert.AreEqual(2, U(m, "h").Flow);
            Assert.Less(U(m, "e").Hp, 9999, "contre-attaque");
        }

        [Test]
        public void MissedReactionTakesDamage()
        {
            var m = EnemyTurnSetup();
            m.RunEnemyTurn(new FixedReaction(Reaction.None));
            Assert.Less(U(m, "h").Hp, 200);
        }

        [Test]
        public void MultiHitAsksOneReactionPerHit()
        {
            var m = EnemyTurnSetup(hits: 3);
            var r = new FixedReaction(Reaction.Dodge);
            m.RunEnemyTurn(r);
            Assert.AreEqual(3, r.Calls);
            Assert.AreEqual(3, m.Log.OfType<HitIncoming>().Count());
        }

        [Test]
        public void DefendingHalvesDamage()
        {
            int TakeHit(bool defend)
            {
                var m = new BattleModel(new[] { Hero("h", spd: 100) }, new[] { Enemy("e", spd: 99, hp: 9999) }, 11);
                m.NextActor();
                if (defend) m.Defend(); else m.Attack(U(m, "e"));
                while (m.NextActor().Team != Team.Enemies) m.Attack(U(m, "e"));
                m.RunEnemyTurn(new FixedReaction(Reaction.None));
                return 200 - U(m, "h").Hp;
            }
            // ATQ 40 contre DEF 20 : 33,3 de base. En défense, au plus 33,3 × 1,05 × 1,5 (critique) × 0,5 = 26 ;
            // sans défense, au moins 33,3 × 0,95 = 32
            Assert.LessOrEqual(TakeHit(true), 26);
            Assert.GreaterOrEqual(TakeHit(false), 32);
        }

        // ---------- Tchatche et Moral ----------

        static BattleModel TchatcheSetup(bool ines = false, bool boss = false)
        {
            var m = new BattleModel(new[] { Hero("h", spd: 300, ines: ines) },
                new[] { Enemy("e", spd: 10, hp: 9999, weak: VanneType.Clash, resist: VanneType.Mytho, boss: boss) }, 9);
            m.NextActor();
            return m;
        }

        [Test]
        public void EffectiveVanneRemovesFortyMoralAndGivesFlow()
        {
            var m = TchatcheSetup();
            m.Tchatche(U(m, "e"), VanneType.Clash);
            Assert.AreEqual(60, U(m, "e").Moral);
            Assert.AreEqual(1, U(m, "h").Flow);
        }

        [Test]
        public void NeutralVanneRemovesTwenty()
        {
            var m = TchatcheSetup();
            m.Tchatche(U(m, "e"), VanneType.Chambrage);
            Assert.AreEqual(80, U(m, "e").Moral);
        }

        [Test]
        public void FailedVanneRemovesFiveAndEnrages()
        {
            var m = TchatcheSetup();
            m.Tchatche(U(m, "e"), VanneType.Mytho);
            Assert.AreEqual(95, U(m, "e").Moral);
            Assert.AreEqual(Rules.EnragedTurns, U(m, "e").EnragedTurns);
            Assert.AreEqual(40 * 1.1f, U(m, "e").AttackValue, 1e-3);
        }

        [Test]
        public void InesDealsFiftyPercentMoreMoralDamage()
        {
            var m = TchatcheSetup(ines: true);
            m.Tchatche(U(m, "e"), VanneType.Clash);
            Assert.AreEqual(40, U(m, "e").Moral);
        }

        [Test]
        public void ZeroMoralDestabilizesThenRecoversToFifty()
        {
            var m = TchatcheSetup();
            var e = U(m, "e");
            for (int i = 0; i < 3; i++) { m.Tchatche(e, VanneType.Clash); m.NextActor(); }
            Assert.AreEqual(0, e.Moral);
            Assert.IsTrue(e.IsDestabilized);
            Assert.IsTrue(m.Log.OfType<Destabilized>().Any());

            // L'ennemi perd son prochain tour, puis son Moral remonte à 50
            while (!m.Log.OfType<TurnSkipped>().Any()) { m.Attack(e); m.NextActor(); }
            Assert.AreEqual(50, e.Moral);
            Assert.IsFalse(e.IsDestabilized);
        }

        [Test]
        public void BossStaysBrokenTwoTurns()
        {
            var m = TchatcheSetup(boss: true);
            var e = U(m, "e");
            for (int i = 0; i < 3; i++) { m.Tchatche(e, VanneType.Clash); m.NextActor(); }
            Assert.AreEqual(Rules.BossBrokenTurns, e.DestabilizedTurns);
        }

        // ---------- Compétences ----------

        [Test]
        public void SkillCostsFlowAndRefusesWhenShort()
        {
            var skill = new SkillDef { Id = "livraison", FlowCost = 2, Power = 60, Hits = 2, Delay = 80 };
            var m = new BattleModel(new[] { Hero("h", spd: 300) }, new[] { Enemy("e", spd: 10, hp: 9999) }, 2);
            var h = m.NextActor();
            Assert.IsFalse(m.CanUse(skill));
            Assert.Throws<System.InvalidOperationException>(() => m.UseSkill(skill, U(m, "e")));
            h.Flow = 3;
            m.UseSkill(skill, U(m, "e"));
            Assert.AreEqual(2, h.Flow, "3 − 2 + 1 pour le coup qui touche");
            Assert.AreEqual(2, m.Log.OfType<Damaged>().Count(d => d.Target.Id == "e"));
        }

        [Test]
        public void ObjectionPushesEnemyBackThreePlaces()
        {
            var objection = new SkillDef { Id = "objection", FlowCost = 0, PushBack = 3, Delay = 120 };
            var m = new BattleModel(new[] { Hero("ines", spd: 300), Hero("b", spd: 100), Hero("c", spd: 100) },
                                    new[] { Enemy("e", spd: 150, hp: 9999) }, 4);
            m.NextActor();
            var before = m.PreviewTimeline(4).Select(u => u.Id).ToList();
            int posBefore = before.IndexOf("e");
            m.UseSkill(objection, U(m, "e"));
            var after = m.PreviewTimeline(6).Select(u => u.Id).ToList();
            Assert.Greater(after.IndexOf("e"), posBefore);
            Assert.IsTrue(m.Log.OfType<PushedBack>().Any());
        }

        [Test]
        public void TauntForcesEnemiesToTargetMomo()
        {
            var garde = new SkillDef { Id = "garde", FlowCost = 0, Target = TargetKind.Self, TauntTurns = 2, DefendTurns = 2 };
            var m = new BattleModel(new[] { Hero("momo", spd: 300), Hero("yanis", spd: 1), Hero("ines", spd: 1) },
                                    new[] { Enemy("e", spd: 200, hp: 9999) }, 8);
            m.NextActor();
            m.UseSkill(garde, null);
            var r = new FixedReaction(Reaction.None);
            for (int i = 0; i < 2; i++)
            {
                var a = m.NextActor();
                if (a.Team == Team.Enemies) m.RunEnemyTurn(r); else m.Defend();
            }
            Assert.IsTrue(m.Log.OfType<HitIncoming>().All(h => h.Target.Id == "momo"));
        }

        [Test]
        public void UppercutStunsOnlyBelowFiftyMoral()
        {
            var uppercut = new SkillDef { Id = "uppercut", FlowCost = 0, Power = 80, StunIfMoralBelow = 50 };
            var m = new BattleModel(new[] { Hero("momo", spd: 300) }, new[] { Enemy("e", spd: 10, hp: 9999) }, 6);
            m.NextActor();
            m.UseSkill(uppercut, U(m, "e"));
            Assert.IsFalse(U(m, "e").SkipNextTurn);
            U(m, "e").Moral = 40;
            m.NextActor();
            m.UseSkill(uppercut, U(m, "e"));
            Assert.IsTrue(U(m, "e").SkipNextTurn);
        }

        [Test]
        public void TeamHealRestoresAllies()
        {
            var sauce = new SkillDef { Id = "sauce", FlowCost = 0, Target = TargetKind.AllAllies, HealPercent = 30 };
            var m = new BattleModel(new[] { Hero("momo", spd: 300), Hero("y") }, new[] { Enemy("e", spd: 1, hp: 9999) }, 6);
            m.NextActor();
            U(m, "momo").Hp = 50; U(m, "y").Hp = 190;
            m.UseSkill(sauce, null);
            Assert.AreEqual(110, U(m, "momo").Hp);
            Assert.AreEqual(200, U(m, "y").Hp, "pas au-delà du max");
        }

        // ---------- Fuite ----------

        [Test]
        public void CannotFleeFromBossOrElite()
        {
            var m = new BattleModel(new[] { Hero("h", spd: 300) }, new[] { Enemy("boss", boss: true) }, 1);
            m.NextActor();
            Assert.IsFalse(m.Flee());
            Assert.AreEqual(BattleOutcome.Ongoing, m.Outcome);

            var m2 = new BattleModel(new[] { Hero("h", spd: 300) }, new[] { Enemy("pigeon") }, 1);
            m2.NextActor();
            Assert.IsTrue(m2.Flee());
            Assert.AreEqual(BattleOutcome.Fled, m2.Outcome);
        }

        [Test]
        public void AllHeroesDownIsDefeat()
        {
            var m = new BattleModel(new[] { Hero("h", spd: 1, hp: 5) }, new[] { Enemy("e", spd: 300, hp: 9999) }, 1);
            var r = new FixedReaction(Reaction.None);
            while (m.Outcome == BattleOutcome.Ongoing)
            {
                var a = m.NextActor();
                if (a.Team == Team.Enemies) m.RunEnemyTurn(r); else m.Defend();
            }
            Assert.AreEqual(BattleOutcome.Defeat, m.Outcome);
        }
    }
}
