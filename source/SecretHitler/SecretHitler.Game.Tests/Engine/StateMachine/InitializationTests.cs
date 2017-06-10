using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;
using System.Linq;
using System;
using Moq;
using SecretHitler.Game.Interfaces;
using System.Collections.Generic;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    [TestClass]
    public class InitializationTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public void StateMachineInitializesWithDefaultValuesTest()
        {
            Assert.AreEqual(StateMachineState.None, StateMachine.MachineState, nameof(StateMachine.MachineState));
            Assert.AreSame(ClientProxy.Object, StateMachine.ClientProxy, nameof(StateMachine.ClientProxy));
        }

        [TestMethod]
        public void StartGameResetsPlayerProperties()
        {
            for (var i = MinPlayerCount; i <= MaxPlayerCount; i++)
            {
                TestInitialize();
                ResetPlayers(i);

                // Dirty up the players.
                foreach (var p in Players)
                {
                    p.IsAlive = Random.Next(2) == 1;
                    p.IsChancellor = true;
                    p.IsPresident = true;
                    p.Role = PlayerRole.Hitler;
                }

                StateMachine.Start();

                Assert.IsTrue(Players.All(p => p.IsAlive), "All players should be alive.");
                Assert.AreEqual(1, Players.Count(p => p.IsPresident), "One player should be presidential candidate.");
                Assert.AreEqual(0, Players.Count(p => p.IsChancellor), "The chancellor should not be assigned.");

                int expectedFascists;
                switch (Players.Count)
                {
                    case 5:
                    case 6:
                        expectedFascists = 1;
                        break;
                    case 7:
                    case 8:
                        expectedFascists = 2;
                        break;
                    case 9:
                    case 10:
                        expectedFascists = 3;
                        break;
                    default:
                        throw new Exception("Invalid player count");
                }

                Assert.AreEqual(expectedFascists, Players.Count(p => p.Role == PlayerRole.Fascist), "Expected number of fascists.");
                Assert.AreEqual(1, Players.Count(p => p.Role == PlayerRole.Hitler), "One player should be Hitler.");
                Assert.AreEqual(i - expectedFascists - 1, Players.Count(p => p.Role == PlayerRole.Liberal), "Everyone else should be liberals");
            }
        }

        [TestMethod]
        public void PolicyDeckUsesDtoDrawPileTest()
        {
            var originalValue = GameData.DrawPile.Count;
            const int numToDraw = 2;
            StateMachine.PolicyDeck.Draw(numToDraw);
            Assert.AreEqual(originalValue - numToDraw, StateMachine.PolicyDeck.DrawPileCount, "Deck draw pile");
            Assert.AreEqual(originalValue - numToDraw, GameData.DrawPile.Count, "DTO draw pile");
        }

        [TestMethod]
        public void PolicyDeckUsesDtoDiscardPileTest()
        {
            var originalValue = GameData.DrawPile.Count;
            const int numToDraw = 2;
            var drawn = StateMachine.PolicyDeck.Draw(numToDraw);
            StateMachine.PolicyDeck.Discard(drawn);
            Assert.AreEqual(numToDraw, StateMachine.PolicyDeck.DiscardPileCount, "Deck discard pile");
            Assert.AreEqual(numToDraw, GameData.DiscardPile.Count, "DTO discard pile");
        }

        [TestMethod]
        public void StartGameResetsPolicyDecks()
        {
            var deckMock = new Mock<ICardDeck<PolicyType>>();
            StateMachine.PolicyDeck = deckMock.Object;
            StateMachine.Start();
            deckMock.Verify(_ => _.Reset(), "Deck was not reset.");
        }

        [TestMethod]
        public void StartGameEntersNominationState()
        {
            StateMachine.Start();
            Assert.AreEqual(StateMachineState.AwaitingNomination, StateMachine.MachineState, "Machine state");
            ClientProxy.Verify(_ => _.SelectPlayer(
                GameData.President,
                GameState.ChancellorNomination,
                It.IsAny<IEnumerable<IPlayerInfo>>()));
        }
    }
}
