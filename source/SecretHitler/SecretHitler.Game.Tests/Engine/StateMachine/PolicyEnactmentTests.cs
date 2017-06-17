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
    public class PolicyEnactmentTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public void VetoingPolicyIncreasesElectionTracker()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void VetoingPolicyCausesChaosIfThresholdExceeded()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void VetoPowerIsUnavailableWithFewerThanFiveFascistPolicies()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void VetoPowerIsAvailableWithFiveFascistPolicies()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ExecutedPlayerIsNoLongerAlive()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void PolicyPeekPowerDoesNotAffectDrawPile()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void InvestigateLoyaltyProvidesPlayersLoyaltyCardOnly()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void PlayerCannotBeChosenForInvestigationMoreThanOnce()
        {
            Assert.Inconclusive();
        }
    }
}
