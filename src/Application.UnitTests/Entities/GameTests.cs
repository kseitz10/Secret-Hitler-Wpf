using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using SecretHitler.Application.LegacyEngine;

namespace SecretHitler.Application.UnitTests.Entities
{
    public class GameTests
    {
        [Test]
        public void CollectionsInitializedTest()
        {
            var game = new GameData();
            Assert.IsNotNull(game.IneligibleChancellors, nameof(game.IneligibleChancellors));
            Assert.AreEqual(0, game.IneligibleChancellors.Count, nameof(game.IneligibleChancellors));
            Assert.IsNotNull(game.Players, nameof(game.Players));
            Assert.AreEqual(0, game.Players.Count, nameof(game.Players));
            Assert.IsNotNull(game.PresidentialQueue, nameof(game.PresidentialQueue));
            Assert.AreEqual(0, game.PresidentialQueue.Count, nameof(game.PresidentialQueue));
            Assert.IsNotNull(game.DrawPile, nameof(game.DrawPile));
            Assert.AreEqual(0, game.DrawPile.Count, nameof(game.DrawPile));
            Assert.IsNotNull(game.DiscardPile, nameof(game.DiscardPile));
            Assert.AreEqual(0, game.DiscardPile.Count, nameof(game.DiscardPile));
        }

        [Test]
        public void DefaultValuesTest()
        {
            var game = new GameData();
            Assert.AreEqual(0, game.ElectionTracker, nameof(game.ElectionTracker));
            Assert.AreEqual(0, game.EnactedFascistPolicyCount, nameof(game.EnactedFascistPolicyCount));
            Assert.AreEqual(0, game.EnactedLiberalPolicyCount, nameof(game.EnactedLiberalPolicyCount));
        }
    }
}
