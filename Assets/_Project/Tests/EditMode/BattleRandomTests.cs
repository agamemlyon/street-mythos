using NUnit.Framework;
using StreetMythos.Battle;

namespace StreetMythos.Tests.EditMode
{
    public class BattleRandomTests
    {
        [Test]
        public void SameSeedSameSequence()
        {
            var a = new BattleRandom(42);
            var b = new BattleRandom(42);
            for (int i = 0; i < 100; i++) Assert.AreEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void RangeStaysInBounds()
        {
            var r = new BattleRandom(7);
            for (int i = 0; i < 1000; i++)
                Assert.That(r.Range(3, 9), Is.InRange(3, 8));
        }
    }
}
