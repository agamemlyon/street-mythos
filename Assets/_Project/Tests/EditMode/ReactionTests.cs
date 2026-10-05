using System.Linq;
using NUnit.Framework;
using StreetMythos.Battle;

namespace StreetMythos.Tests.EditMode
{
    public class ReactionTests
    {
        [Test]
        public void DodgeWindowIs250msCentered()
        {
            Assert.AreEqual(Reaction.Dodge, ReactionJudge.Judge(10.0, 10.12, null, Assist.None));
            Assert.AreEqual(Reaction.Dodge, ReactionJudge.Judge(10.0, 9.88, null, Assist.None));
            Assert.AreEqual(Reaction.None, ReactionJudge.Judge(10.0, 10.13, null, Assist.None));
            Assert.AreEqual(Reaction.None, ReactionJudge.Judge(10.0, 9.8, null, Assist.None));
        }

        [Test]
        public void ParryWindowIs120msAndWins()
        {
            Assert.AreEqual(Reaction.Parry, ReactionJudge.Judge(5.0, 5.0, 5.05, Assist.None));
            Assert.AreEqual(Reaction.Dodge, ReactionJudge.Judge(5.0, 5.0, 5.07, Assist.None), "parade ratée, esquive réussie");
            Assert.AreEqual(Reaction.None, ReactionJudge.Judge(5.0, null, 5.07, Assist.None));
        }

        [Test]
        public void WideWindowsDoubleTheTolerance()
        {
            Assert.AreEqual(Reaction.Parry, ReactionJudge.Judge(5.0, null, 5.11, Assist.WideWindows));
            Assert.AreEqual(Reaction.Dodge, ReactionJudge.Judge(5.0, 5.24, null, Assist.WideWindows));
        }

        [Test]
        public void AutoDodgeKeepsParryManual()
        {
            Assert.AreEqual(Reaction.Dodge, ReactionJudge.Judge(5.0, null, null, Assist.AutoDodge));
            Assert.AreEqual(Reaction.Parry, ReactionJudge.Judge(5.0, null, 5.02, Assist.AutoDodge));
        }

        [Test]
        public void SteppedEnemyTurnMatchesOneShotTurn()
        {
            UnitDef H() => new UnitDef { Id = "h", Team = Team.Heroes, Stats = new Stats(500, 50, 20, 10) };
            UnitDef E() => new UnitDef { Id = "e", Team = Team.Enemies, Stats = new Stats(500, 40, 20, 300),
                                         Moves = { new EnemyMove { Id = "m", Power = 60, Hits = 3 } } };

            var a = new BattleModel(new[] { H() }, new[] { E() }, 77);
            a.NextActor();
            var turn = a.StartEnemyTurn();
            int hits = 0;
            while (turn.NextHit(out var hit)) { hits++; turn.Resolve(hit.HitIndex == 1 ? Reaction.Parry : Reaction.None); }
            turn.Finish();

            var b = new BattleModel(new[] { H() }, new[] { E() }, 77);
            b.NextActor();
            int i = 0;
            b.RunEnemyTurn(new Seq(() => i++ == 1 ? Reaction.Parry : Reaction.None));

            Assert.AreEqual(3, hits);
            Assert.AreEqual(b.Units.First(u => u.Id == "h").Hp, a.Units.First(u => u.Id == "h").Hp);
            Assert.AreEqual(b.Units.First(u => u.Id == "e").Hp, a.Units.First(u => u.Id == "e").Hp);
        }

        [Test]
        public void ResolvingWithoutPendingHitThrows()
        {
            var m = new BattleModel(new[] { new UnitDef { Id = "h", Team = Team.Heroes, Stats = new Stats(100, 10, 10, 1) } },
                                    new[] { new UnitDef { Id = "e", Team = Team.Enemies, Stats = new Stats(100, 10, 10, 300) } }, 1);
            m.NextActor();
            var turn = m.StartEnemyTurn();
            Assert.Throws<System.InvalidOperationException>(() => turn.Resolve(Reaction.None));
        }

        sealed class Seq : IReactionSource
        {
            readonly System.Func<Reaction> _f;
            public Seq(System.Func<Reaction> f) => _f = f;
            public Reaction React(BattleUnit a, BattleUnit t, int i) => _f();
        }
    }
}
