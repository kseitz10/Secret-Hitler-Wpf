﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Tests.Engine
{
    [TestClass]
    public class PolicyDeskTests
    {
        private PolicyDeck PolicyDeck { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            PolicyDeck = new PolicyDeck(new List<PolicyType>(), new List<PolicyType>(), true);
        }

        [TestMethod]
        public void PolicyDeckContainsCorrectNumberOfLiberalCardsTest()
        {
            var entireDeck = PolicyDeck.Peek(Int32.MaxValue);
            Assert.AreEqual(Constants.TotalLiberalPolicies, entireDeck.Count(_ => _ == PolicyType.Liberal));
        }

        [TestMethod]
        public void PolicyDeckContainsCorrectNumberOfFascistCardsTest()
        {
            var entireDeck = PolicyDeck.Peek(Int32.MaxValue);
            Assert.AreEqual(Constants.TotalFascistPolicies, entireDeck.Count(_ => _ == PolicyType.Fascist));
        }

        [TestMethod]
        public void NumberOfCardsInPileIsCorrectTest()
        {
            var totalExpected = Constants.TotalFascistPolicies + Constants.TotalLiberalPolicies;
            Assert.AreEqual(totalExpected, PolicyDeck.DrawPileCount, "Draw pile should be full.");
            Assert.AreEqual(0, PolicyDeck.DiscardPileCount, "Discard pile should be empty.");

            var drawn = PolicyDeck.Draw(2).ToList();
            Assert.AreEqual(totalExpected - 2, PolicyDeck.DrawPileCount, "Cards should have been removed.");
            Assert.AreEqual(0, PolicyDeck.DiscardPileCount, "Discard pile should be empty.");

            drawn.AddRange(PolicyDeck.Draw(PolicyDeck.DrawPileCount));
            Assert.AreEqual(0, PolicyDeck.DrawPileCount, "Draw pile should be empty.");
            Assert.AreEqual(0, PolicyDeck.DiscardPileCount, "Discard pile should be empty.");

            PolicyDeck.Discard(drawn);
            Assert.AreEqual(0, PolicyDeck.DrawPileCount, "Draw pile should be empty.");
            Assert.AreEqual(totalExpected, PolicyDeck.DiscardPileCount, "Discard pile should be full.");

            PolicyDeck.Reset();
            Assert.AreEqual(totalExpected, PolicyDeck.DrawPileCount, "Draw pile should be full.");
            Assert.AreEqual(0, PolicyDeck.DiscardPileCount, "Discard pile should be empty.");
        }

        [TestMethod]
        public void PeekingDoesNotAffectDrawPileTest()
        {
            var firstDraw = PolicyDeck.Peek(PolicyDeck.DrawPileCount);
            var secondDraw = PolicyDeck.Peek(PolicyDeck.DrawPileCount);
            var thirdDraw = PolicyDeck.Peek(PolicyDeck.DrawPileCount);
            Assert.IsTrue(firstDraw.SequenceEqual(secondDraw));
            Assert.IsTrue(firstDraw.SequenceEqual(thirdDraw));
        }

        [TestMethod]
        public void PolicyDeckIsShuffledWhenDrawPileEmptyTest()
        {
            var firstDraw = PolicyDeck.Draw(PolicyDeck.DrawPileCount);
            var secondDraw = PolicyDeck.Draw(PolicyDeck.DrawPileCount);
            Assert.IsFalse(firstDraw.SequenceEqual(secondDraw));
        }

        [TestMethod]
        public void PolicyDeckIsShuffledWhenResetTest()
        {
            var firstDraw = PolicyDeck.Peek(PolicyDeck.DrawPileCount);
            PolicyDeck.Reset();
            var secondDraw = PolicyDeck.Draw(PolicyDeck.DrawPileCount);
            Assert.IsFalse(firstDraw.SequenceEqual(secondDraw));
        }

        [TestMethod]
        public void PolicyDeckIsShuffledManuallyTest()
        {
            var firstDraw = PolicyDeck.Peek(Int32.MaxValue);
            PolicyDeck.Shuffle();
            var secondDraw = PolicyDeck.Peek(Int32.MaxValue);
            Assert.IsFalse(firstDraw.SequenceEqual(secondDraw));
        }

        [TestMethod]
        public void PolicyDeckDrawPileIsRefilledWhenInsufficientTest()
        {
            var totalDeckSize = Constants.TotalFascistPolicies + Constants.TotalLiberalPolicies;
            PolicyDeck.Discard(PolicyDeck.Draw(PolicyDeck.DrawPileCount - 2));
            Assert.AreEqual(2, PolicyDeck.DrawPileCount, "Deck should have 2 cards left after drawing all but 2 cards.");
            Assert.AreEqual(totalDeckSize - 2, PolicyDeck.DiscardPileCount, "Should have discarded all drawn cards.");
            PolicyDeck.Draw(3);
            Assert.AreEqual(totalDeckSize - 3, PolicyDeck.DrawPileCount, "Should have drawn 3 cards.");
        }

        [TestMethod]
        public void PolicyDeckDrawPileIsNotRefilledWhenSufficientTest()
        {
            var totalDeckSize = Constants.TotalFascistPolicies + Constants.TotalLiberalPolicies;
            PolicyDeck.Discard(PolicyDeck.Draw(PolicyDeck.DrawPileCount - 3));
            Assert.AreEqual(3, PolicyDeck.DrawPileCount, "Deck should have 3 cards left after drawing all but 3 cards.");
            Assert.AreEqual(totalDeckSize - 3, PolicyDeck.DiscardPileCount, "Should have discarded all drawn cards.");
            PolicyDeck.Draw(3);
            Assert.AreEqual(0, PolicyDeck.DrawPileCount, "Should have drawn 3 cards with 0 cards remaining.");
        }
    }
}
