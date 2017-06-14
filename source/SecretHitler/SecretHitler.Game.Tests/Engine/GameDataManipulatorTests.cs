using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;

namespace SecretHitler.Game.Tests.Engine
{
    [TestClass]
    public class GameDataManipulatorTests
    {
        private static Random Random = new Random();

        private GameData Game { get; set; }

        private GameDataManipulator Util { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Game = new GameData();
            Util = new GameDataManipulator(Game);
        }

        [TestMethod]
        public void ResetWith5Players() => AssertReset(5);

        [TestMethod]
        public void ResetWith6Players() => AssertReset(6);

        [TestMethod]
        public void ResetWith7Players() => AssertReset(7);

        [TestMethod]
        public void ResetWith8Players() => AssertReset(8);

        [TestMethod]
        public void ResetWith9Players() => AssertReset(9);

        [TestMethod]
        public void ResetWith10Players() => AssertReset(10);

        [TestMethod]
        public void ResetResetsPolicyDecks()
        {
            ConfigureGame(Constants.MinPlayerCount);

            Game.DrawPile.Clear();
            Game.DrawPile.AddRange(Enumerable.Range(1, 5).Select(_ => PolicyType.Fascist));
            Game.DiscardPile.AddRange(Enumerable.Range(1, 5).Select(_ => PolicyType.Fascist));

            Util.ResetGame();

            Assert.AreEqual(Constants.TotalFascistPolicies + Constants.TotalLiberalPolicies, Game.DrawPile.Count, "Draw pile count");
            Assert.AreEqual(0, Game.DiscardPile.Count, "Discard pile count");
        }

        [TestMethod]
        public void TermLimitsClearedWhenInvokingUpdateTermLimits()
        {
            ConfigureGame(5);

            (var origPresident, var origChancellor) = SetAndGetLeadership();

            Game.IneligibleChancellors.Add(origPresident);
            Game.IneligibleChancellors.Add(origChancellor);

            (var president2, var chancellor2) = SetAndGetLeadership(1);

            Util.UpdateTermLimits();

            Assert.IsFalse(Game.IneligibleChancellors.Contains(origPresident), "Old president should be removed");
            Assert.IsFalse(Game.IneligibleChancellors.Contains(origChancellor), "Old chancellor should be removed");
        }

        [TestMethod]
        public void TermLimitNotTrackedForPresidentsWhenFivePlayers()
        {
            ConfigureGame(5);
            (var president, var chancellor) = SetAndGetLeadership();

            Util.UpdateTermLimits();

            Assert.IsFalse(Game.IneligibleChancellors.Contains(president), "President term limits");
            Assert.IsTrue(Game.IneligibleChancellors.Contains(chancellor), "Chancellor should always be term-limited");
        }

        [TestMethod]
        public void TermLimitNotTrackedForPresidentsWhenFivePlayersAlive()
        {
            ConfigureGame(6);
            (var president, var chancellor) = SetAndGetLeadership();
            Game.Players.Last().IsAlive = false;

            Util.UpdateTermLimits();

            Assert.IsFalse(Game.IneligibleChancellors.Contains(president), "President term limits");
            Assert.IsTrue(Game.IneligibleChancellors.Contains(chancellor), "Chancellor should always be term-limited");
        }

        [TestMethod]
        public void TermLimitsTrackedForPresidentWhenMoreThanFivePlayersAlive()
        {
            ConfigureGame(7);
            (var president, var chancellor) = SetAndGetLeadership();
            Game.Players.Last().IsAlive = false;

            Util.UpdateTermLimits();

            Assert.IsTrue(Game.IneligibleChancellors.Contains(president), "President term limits");
            Assert.IsTrue(Game.IneligibleChancellors.Contains(chancellor), "Chancellor should always be term-limited");
        }

        [TestMethod]
        public void UpdatingElectionTrackerOnlyIncreasesElectionTracker()
        {
            var expected = 0;
            ConfigureGame(Constants.MinPlayerCount);
            (var president, var chancellor) = SetAndGetLeadership();
            Util.UpdateTermLimits();
            var originalTermLimits = Game.IneligibleChancellors.ToArray();

            var mock = new Mock<ICardDeck<PolicyType>>();

            for (var i = 1; i < Constants.FailedElectionThreshold; i++)
            {
                Util.UpdateElectionTracker(mock.Object);
                Assert.AreEqual(++expected, Game.ElectionTracker, "Election tracker should be updated");
                Assert.IsTrue(Game.IneligibleChancellors.SequenceEqual(originalTermLimits), "Term limits should not change");
                mock.Verify(_ => _.Draw(It.IsAny<int>()), Times.Never, "Cards should not be drawn");
            }
        }

        [TestMethod]
        public void ChaosResetsCounterAndTermLimits()
        {
            ConfigureGame(Constants.MinPlayerCount);
            Game.ElectionTracker = Constants.FailedElectionThreshold - 1;
            (var president, var chancellor) = SetAndGetLeadership();
            Util.UpdateTermLimits();

            var mock = new Mock<ICardDeck<PolicyType>>();
            mock.Setup(_ => _.Draw(1)).Returns(new[] { PolicyType.Liberal });
            Util.UpdateElectionTracker(mock.Object);

            Assert.AreEqual(0, Game.ElectionTracker, "Election tracker should be reset");
            Assert.AreEqual(0, Game.IneligibleChancellors.Count, "Term limits should be forgotten");
        }

        [TestMethod]
        public void ChaosCanEnactLiberalPolicy()
        {
            ConfigureGame(Constants.MinPlayerCount);
            Game.EnactedLiberalPolicyCount = 1;
            Game.ElectionTracker = Constants.FailedElectionThreshold - 1;
            var mock = new Mock<ICardDeck<PolicyType>>();
            mock.Setup(_ => _.Draw(1)).Returns(new[] { PolicyType.Liberal });
            Util.UpdateElectionTracker(mock.Object);
            Assert.AreEqual(2, Game.EnactedLiberalPolicyCount, "Enact liberal policy");
            mock.Verify(_ => _.Draw(1), Times.Once, "Card should be drawn");
        }

        [TestMethod]
        public void ChaosCanEnactFascistPolicy()
        {
            ConfigureGame(Constants.MinPlayerCount);
            Game.EnactedFascistPolicyCount = 1;
            Game.ElectionTracker = Constants.FailedElectionThreshold - 1;
            var mock = new Mock<ICardDeck<PolicyType>>();
            mock.Setup(_ => _.Draw(1)).Returns(new[] { PolicyType.Fascist });
            Util.UpdateElectionTracker(mock.Object);
            Assert.AreEqual(2, Game.EnactedFascistPolicyCount, "Enact fascist policy");
            mock.Verify(_ => _.Draw(1), Times.Once, "Card should be drawn");
        }

        protected void ConfigureGame(int playerCt = Constants.MinPlayerCount)
        {
            for (var i = 0; i < playerCt; i++)
            {
                var player = new PlayerData()
                {
                    Identifier = Guid.NewGuid(),
                };
                player.Name = player.Identifier.ToString();
                player.IsAlive = true;

                Game.Players.Add(player);
            }

            Util.ResetGame();
        }

        private (PlayerData president, PlayerData chancellor) SetAndGetLeadership(int offset = 0)
        {
            foreach (var p in Game.Players)
            {
                p.IsPresident = false;
                p.IsChancellor = false;
            }

            var president = Game.Players.Skip(offset * 2).First();
            president.IsPresident = true;
            var chancellor = Game.Players.Skip(offset * 2 + 1).First();
            chancellor.IsChancellor = true;
            return (president, chancellor);
        }

        private void AssertReset(int playerCount)
        {
            ConfigureGame(playerCount);

            // Dirty up the players.
            foreach (var p in Game.Players)
            {
                p.IsAlive = Random.Next(2) == 1;
                p.IsChancellor = true;
                p.IsPresident = true;
                p.Role = PlayerRole.Hitler;
            }

            var util = new GameDataManipulator(Game);
            util.ResetGame();

            Assert.IsTrue(Game.Players.All(p => p.IsAlive), "All players should be alive.");

            int expectedFascists;
            switch (Game.Players.Count)
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

            Assert.AreEqual(expectedFascists, Game.Players.Count(p => p.Role == PlayerRole.Fascist), "Expected number of fascists.");
            Assert.AreEqual(1, Game.Players.Count(p => p.Role == PlayerRole.Hitler), "One player should be Hitler.");
            Assert.AreEqual(playerCount - expectedFascists - 1, Game.Players.Count(p => p.Role == PlayerRole.Liberal), "Everyone else should be liberals");
        }
    }
}
