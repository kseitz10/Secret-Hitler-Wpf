using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    [TestClass]
    public class NominationAndElectionTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public void ChaosEnactsPolicyWithoutPresidentialPower()
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
            var president = Players.First();
            var nomination = Players.Skip(1).First();
            StateMachine.MachineState = StateMachineState.AwaitingNomination;
            StateMachine.PlayerSelected(nomination);
            Assert.AreEqual(nomination, Game.Chancellor, "Chancellor should be assigned");
            Assert.AreEqual(StateMachine.MachineState, StateMachineState.AwaitingVotes);
            ClientProxy.Verify(_ => _.GetVotes(It.Is<IEnumerable<IPlayerInfo>>(voters => voters.All(v => v.IsAlive))));
        }

        [TestMethod]
        public void MajorityVoteCompletesElectionAndDeliversPolicies()
        {
            const int Pass = 4;
            const int Fail = 3;
            ResetPlayers(Pass + Fail);

            var president = Players.First();
            president.IsPresident = true;
            StateMachine.MachineState = StateMachineState.AwaitingVotes;

            StateMachine.VotesCollected(Enumerable.Range(0, Pass).Select(_ => true).Concat(Enumerable.Range(0, Fail).Select(_ => false)));

            Assert.AreEqual(StateMachine.MachineState, StateMachineState.AwaitingPresidentialPolicies);
            ClientProxy.Verify(_ => _.GetPresidentialPolicies(president, It.Is<IEnumerable<PolicyType>>(d => d.Count() == Constants.PresidentialPolicyDrawCount)));
        }

        [TestMethod]
        public void NonMajorityVoteFailsElection()
        {
            const int Pass = 3;
            const int Fail = 3;
            ResetPlayers(Pass + Fail);
            Manipulator.Object.ResetGame();

            var president = Players.First();
            president.IsPresident = true;
            StateMachine.MachineState = StateMachineState.AwaitingVotes;

            StateMachine.VotesCollected(Enumerable.Range(0, Pass).Select(_ => true).Concat(Enumerable.Range(0, Fail).Select(_ => false)));

            Assert.AreEqual(StateMachine.MachineState, StateMachineState.AwaitingNomination);
            Manipulator.Verify(_ => _.UpdateElectionTracker(null));
            Manipulator.Verify(_ => _.GetPresidentFromQueue());
        }

        [TestMethod]
        public void NonMajorityVoteFailsElectionAndHandlesLiberalPolicyWinCondition()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void NonMajorityVoteFailsElectionAndHandlesFascistPolicyWinCondition()
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
