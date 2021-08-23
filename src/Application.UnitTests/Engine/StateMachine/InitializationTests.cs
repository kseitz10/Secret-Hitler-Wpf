using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using SecretHitler.Application.Common;
using SecretHitler.Application.LegacyEngine;
using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.UnitTests.Engine.StateMachine
{
    public class InitializationTests : GameStateMachineTestFixture
    {
        [Test]
        public void StateMachineInitializesWithDefaultValues()
        {
            Assert.AreEqual(StateMachineState.None, StateMachine.GameData.MachineState, nameof(StateMachine.GameData.MachineState));
            Assert.AreSame(Director.Object, StateMachine.Director, nameof(StateMachine.Director));
        }

        [Test]
        public async Task StartGameResetsGameData()
        {
            AddPlayers(Constants.MaxPlayerCount);
            await StateMachine.Start();
            Manipulator.Verify(_ => _.ResetGame());
        }

        [Test]
        public async Task PolicyDeckUsesDtoDrawPile()
        {
            var originalValue = GameData.DrawPile.Count;
            const int numToDraw = 2;
            StateMachine.PolicyDeck.Draw(numToDraw);
            Assert.AreEqual(originalValue - numToDraw, StateMachine.PolicyDeck.DrawPileCount, "Deck draw pile");
            Assert.AreEqual(originalValue - numToDraw, GameData.DrawPile.Count, "DTO draw pile");
        }

        [Test]
        public async Task PolicyDeckUsesDtoDiscardPile()
        {
            var originalValue = GameData.DrawPile.Count;
            const int numToDraw = 2;
            var drawn = StateMachine.PolicyDeck.Draw(numToDraw);
            StateMachine.PolicyDeck.Discard(drawn);
            Assert.AreEqual(numToDraw, StateMachine.PolicyDeck.DiscardPileCount, "Deck discard pile");
            Assert.AreEqual(numToDraw, GameData.DiscardPile.Count, "DTO discard pile");
        }

        [Test]
        public async Task StartGameEntersNominationState()
        {
            await StateMachine.Start();
            Assert.AreEqual(StateMachineState.AwaitingNomination, StateMachine.GameData.MachineState, "Machine state");
            Assert.AreEqual(1, Players.Count(p => p.IsPresident), "One player should be presidential candidate.");
            Assert.AreEqual(0, Players.Count(p => p.IsChancellor), "The chancellor should not be assigned.");
            Director.Verify(_ => _.SelectPlayer(
                GameData.President,
                GameState.ChancellorNomination,
                It.Is<IEnumerable<Guid>>(candidates => candidates.AsPlayers(Players).All(c => c.IsAlive && !c.IsPresident))));
        }
    }
}
