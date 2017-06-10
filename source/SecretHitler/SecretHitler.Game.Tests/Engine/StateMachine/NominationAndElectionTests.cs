using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    [TestClass]
    public class NominationAndElectionTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public void NoTermLimitsForPresidentsWhenFivePlayersAlive()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void TermLimitsForPresidentsWhenMoreThanFivePlayersAlive()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void TermLimitsForChancellor()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void FailedElectionIncreasesElectionTracker()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ChaosEnactsPolicyWithoutPresidentialPower()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ChaosClearsTermLimits()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ChaosResetsElectionTracker()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void SuccessfulElectionResetsElectionTracker()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void RegularElectionAdvancesPresidentialQueue()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void SpecialElectionUsesDesignatedPlayer()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void SpecialElectionDoesNotAffectPresidentialQueue()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ChancellorCandidateListOnlyContainsValidPlayers()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void SelectingNominationTriggersVotingStage()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void MajorityVoteCompletesElection()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void NonMajorityVoteFailsElection()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ElectingHitlerChancellorWithThreeFascistPoliciesGivesFascistVictory()
        {
            Assert.Inconclusive();
        }
    }
}
