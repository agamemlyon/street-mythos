using System.IO;
using System.Linq;
using NUnit.Framework;
using StreetMythos.Battle;
using StreetMythos.Data;
using UnityEngine;

namespace StreetMythos.Tests.EditMode
{
    public class GameDataTests
    {
        static GameData Load()
        {
            string dir = Path.Combine(Application.dataPath, "_Project/Data/Json");
            string R(string f) => File.ReadAllText(Path.Combine(dir, f));
            return new GameData(R("skills.json"), R("heroes.json"), R("enemies.json"), R("encounters.json"));
        }

        [Test]
        public void JsonLoadsWithoutInconsistencies()
        {
            var data = Load();
            CollectionAssert.IsEmpty(data.Validate());
            Assert.AreEqual(3, data.Heroes.Count);
            Assert.AreEqual(12, data.Skills.Count, "4 compétences par héros (SPEC § 4.6)");
        }

        [Test]
        public void SkillsUnlockAtLevelsOneThreeSix()
        {
            var data = Load();
            Assert.AreEqual(2, data.BuildHero("yanis", 1).Skills.Count);
            Assert.AreEqual(3, data.BuildHero("yanis", 3).Skills.Count);
            Assert.AreEqual(4, data.BuildHero("yanis", 6).Skills.Count);
        }

        [Test]
        public void InesHasMoralBonusFlag()
        {
            var data = Load();
            Assert.IsTrue(data.BuildHero("ines", 1).IsInes);
            Assert.IsFalse(data.BuildHero("momo", 1).IsInes);
        }

        [Test]
        public void EncounterBuildsUniqueEnemyIds()
        {
            var data = Load();
            var ids = data.BuildEncounter("q1_tuto_pigeons").Select(e => e.Id).ToList();
            Assert.AreEqual(3, ids.Distinct().Count());
        }

        [Test]
        public void BossAndEliteFlagsAreSet()
        {
            var data = Load();
            Assert.IsTrue(data.BuildEnemy("gros_caillou").IsBoss);
            Assert.IsTrue(data.BuildEnemy("controleur_fantome").IsElite);
        }

        // ---------- Équilibrage : chaque combat du quartier 1 doit se gagner, sans être expédié ----------

        sealed class HalfReactions : IReactionSource
        {
            int _n;
            public Reaction React(BattleUnit a, BattleUnit t, int i) => (_n++ % 2 == 0) ? Reaction.Dodge : Reaction.None;
        }

        // IA simple de test : vanne efficace si possible, sinon la meilleure compétence payable, sinon attaque
        static (BattleOutcome outcome, int turns, int heroHpLost) Simulate(GameData data, string encounter, int level, uint seed)
        {
            var heroes = new[] { "yanis", "ines", "momo" }.Select(h => data.BuildHero(h, level)).ToList();
            var m = new BattleModel(heroes, data.BuildEncounter(encounter), seed);
            int maxHp = m.Heroes.Sum(h => h.Hp);
            var reactions = new HalfReactions();
            int turns = 0;
            while (m.Outcome == BattleOutcome.Ongoing && turns < 300)
            {
                var a = m.NextActor();
                if (a == null) break;
                turns++;
                if (a.Team == Team.Enemies) { m.RunEnemyTurn(reactions); continue; }

                var target = m.Enemies.Where(e => e.IsAlive).OrderBy(e => e.Hp).First();
                var wounded = m.Heroes.Where(h => h.IsAlive).OrderBy(h => h.Hp * 100 / h.Def.Stats.MaxHp).First();
                var skill = a.Def.Skills.Where(s => m.CanUse(s)).OrderByDescending(s => s.FlowCost).FirstOrDefault();

                if (target.Def.Weakness.HasValue && target.Moral > 0 && !target.IsDestabilized && turns % 3 == 0)
                    m.Tchatche(target, target.Def.Weakness.Value);
                else if (skill != null && (skill.Power > 0 || skill.MoralDamage > 0 || skill.PushBack > 0))
                    m.UseSkill(skill, target);
                else if (skill != null && skill.HealPercent > 0 && wounded.Hp * 2 < wounded.Def.Stats.MaxHp)
                    m.UseSkill(skill, wounded);
                else
                    m.Attack(target);
            }
            return (m.Outcome, turns, maxHp - m.Heroes.Sum(h => h.Hp));
        }

        [TestCase("q1_tuto_pigeons", 1)]
        [TestCase("q1_brume_montee", 2)]
        [TestCase("q1_lion_place", 2)]
        [TestCase("q1_elite_ficelle", 3)]
        [TestCase("q1_boss_gros_caillou", 4)]
        public void Q1EncountersAreWinnableAndNotTrivial(string encounter, int level)
        {
            var data = Load();
            int wins = 0, totalTurns = 0;
            for (uint seed = 1; seed <= 20; seed++)
            {
                var (outcome, turns, lost) = Simulate(data, encounter, level, seed);
                if (outcome == BattleOutcome.Victory) wins++;
                totalTurns += turns;
            }
            float avgTurns = totalTurns / 20f;
            Debug.Log($"[Équilibrage] {encounter} niv. {level} : {wins}/20 victoires, {avgTurns:0.0} tours en moyenne");
            Assert.GreaterOrEqual(wins, 18, "un joueur moyen doit gagner");
            Assert.GreaterOrEqual(avgTurns, 6, "le combat ne doit pas être expédié");
        }
    }
}
