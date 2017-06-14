using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecretHitler.Game.Entities;

namespace SecretHitler.Game.Tests.Entities
{
    [TestClass]
    public class GameTests
    {
        [TestMethod]
        public void CollectionsInitializedTest()
        {
            var game = new GameData();
            Assert.IsNotNull(game.EnactedPolicies, nameof(game.EnactedPolicies));
            Assert.AreEqual(0, game.EnactedPolicies.Count, nameof(game.EnactedPolicies));
            Assert.IsNotNull(game.IneligibleChancellors, nameof(game.IneligibleChancellors));
            Assert.AreEqual(0, game.IneligibleChancellors.Count, nameof(game.IneligibleChancellors));
            Assert.IsNotNull(game.Players, nameof(game.Players));
            Assert.AreEqual(0, game.Players.Count, nameof(game.Players));
            Assert.IsNotNull(game.PresidentialQueue, nameof(game.PresidentialQueue));
            Assert.AreEqual(0, game.PresidentialQueue.Count, nameof(game.PresidentialQueue));
            Assert.IsNotNull(game.EnactedPolicies, nameof(game.EnactedPolicies));
            Assert.AreEqual(0, game.EnactedPolicies.Count, nameof(game.EnactedPolicies));
            Assert.IsNotNull(game.DrawPile, nameof(game.DrawPile));
            Assert.IsNotNull(game.DiscardPile, nameof(game.DiscardPile));
        }

        [TestMethod]
        public void DefaultValuesTest()
        {
            var game = new GameData();
            Assert.AreEqual(0, game.ElectionTracker, nameof(game.ElectionTracker));
            Assert.AreEqual(0, game.EnactedFascistPolicyCount, nameof(game.EnactedFascistPolicyCount));
            Assert.AreEqual(0, game.EnactedLiberalPolicyCount, nameof(game.EnactedLiberalPolicyCount));
        }
    }
}
