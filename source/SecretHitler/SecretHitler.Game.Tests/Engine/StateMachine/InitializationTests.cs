using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    [TestClass]
    public class InitializationTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public void StateMachineInitializesWithDefaultValues()
        {
            Assert.AreEqual(StateMachineState.None, StateMachine.MachineState, nameof(StateMachine.MachineState));
            Assert.AreSame(ClientProxy.Object, StateMachine.ClientProxy, nameof(StateMachine.ClientProxy));
        }

        [TestMethod]
        public void StartGameResetsGameData()
        {
            AddPlayers(Constants.MaxPlayerCount);
            StateMachine.Start();
            Manipulator.Verify(_ => _.ResetGame());
        }

        [TestMethod]
        public void PolicyDeckUsesDtoDrawPile()
        {
            var originalValue = Game.DrawPile.Count;
            const int numToDraw = 2;
            StateMachine.PolicyDeck.Draw(numToDraw);
            Assert.AreEqual(originalValue - numToDraw, StateMachine.PolicyDeck.DrawPileCount, "Deck draw pile");
            Assert.AreEqual(originalValue - numToDraw, Game.DrawPile.Count, "DTO draw pile");
        }

        [TestMethod]
        public void PolicyDeckUsesDtoDiscardPile()
        {
            var originalValue = Game.DrawPile.Count;
            const int numToDraw = 2;
            var drawn = StateMachine.PolicyDeck.Draw(numToDraw);
            StateMachine.PolicyDeck.Discard(drawn);
            Assert.AreEqual(numToDraw, StateMachine.PolicyDeck.DiscardPileCount, "Deck discard pile");
            Assert.AreEqual(numToDraw, Game.DiscardPile.Count, "DTO discard pile");
        }

        [TestMethod]
        public void StartGameEntersNominationState()
        {
            StateMachine.Start();
            Assert.AreEqual(StateMachineState.AwaitingNomination, StateMachine.MachineState, "Machine state");
            Assert.AreEqual(1, Players.Count(p => p.IsPresident), "One player should be presidential candidate.");
            Assert.AreEqual(0, Players.Count(p => p.IsChancellor), "The chancellor should not be assigned.");
            ClientProxy.Verify(_ => _.SelectPlayer(
                Game.President,
                GameState.ChancellorNomination,
                It.Is<IEnumerable<IPlayerInfo>>(candidates => candidates.All(c => c.IsAlive && !c.IsPresident))));
        }
    }
}
