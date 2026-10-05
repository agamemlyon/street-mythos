using System;
using NUnit.Framework;
using StreetMythos.Core;

namespace StreetMythos.Tests.EditMode
{
    public class GameStateMachineTests
    {
        [Test]
        public void StartsInBoot() => Assert.AreEqual(GameState.Boot, new GameStateMachine().Current);

        [Test]
        public void BootGoesToMenuThenExploration()
        {
            var sm = new GameStateMachine();
            sm.Enter(GameState.Menu);
            sm.Enter(GameState.Exploration);
            Assert.AreEqual(GameState.Exploration, sm.Current);
        }

        [Test]
        public void ForbiddenTransitionThrows()
        {
            var sm = new GameStateMachine();
            Assert.Throws<InvalidOperationException>(() => sm.Enter(GameState.Combat));
        }

        [Test]
        public void PauseReturnsToPreviousState()
        {
            var sm = new GameStateMachine();
            sm.Enter(GameState.Menu);
            sm.Enter(GameState.Exploration);
            sm.Enter(GameState.Combat);
            sm.Enter(GameState.Pause);
            Assert.IsFalse(sm.CanEnter(GameState.Exploration));
            sm.Enter(GameState.Combat);
            Assert.AreEqual(GameState.Combat, sm.Current);
        }
    }
}
