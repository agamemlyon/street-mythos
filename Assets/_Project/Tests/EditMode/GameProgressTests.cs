using NUnit.Framework;
using StreetMythos.Core;
using UnityEngine;

namespace StreetMythos.Tests.EditMode
{
    public class GameProgressTests
    {
        [Test]
        public void XpLevelsUpAndCarriesRemainder()
        {
            var p = new GameProgress();
            int ups = p.GainXp(GameProgress.XpToNext(1) + 5);
            Assert.AreEqual(1, ups);
            Assert.AreEqual(2, p.level);
            Assert.AreEqual(5, p.xp);
        }

        [Test]
        public void LevelIsCappedAtTen()
        {
            var p = new GameProgress();
            p.GainXp(100000);
            Assert.AreEqual(GameProgress.MaxLevel, p.level);
        }

        [Test]
        public void JsonRoundTripKeepsEverything()
        {
            var p = new GameProgress { level = 3, xp = 12, balles = 40, reput = 2 };
            p.defeated.Add("zone_pigeons");
            p.Set("vu_guignol");
            p.SetPosition(new Vector3(1, 2, 3), 90);
            var q = GameProgress.FromJson(p.ToJson());
            Assert.AreEqual(3, q.level);
            Assert.IsTrue(q.IsDefeated("zone_pigeons"));
            Assert.IsTrue(q.Has("vu_guignol"));
            Assert.AreEqual(new Vector3(1, 2, 3), q.Position);
            Assert.AreEqual(GameProgress.Version, q.version);
        }
    }
}
