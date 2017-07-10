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
            var president = Players.First();
            president.IsPresident = true;
            var chancellor = Players.Skip(1).First();
            chancellor.IsChancellor = true;
            StateMachine.DrawnPolicies = new List<PolicyType>() { PolicyType.Fascist, PolicyType.Fascist };
            StateMachine.MachineState = StateMachineState.AwaitingPresidentialPolicies;
            GameData.EnactedFascistPolicyCount = Constants.MinFascistPolicyCountForVeto - 1;
            StateMachine.PoliciesSelected(new[] { PolicyType.Fascist, PolicyType.Liberal });
            Director.Verify(_ => _.GetEnactedPolicy(chancellor, It.IsAny<IEnumerable<PolicyType>>(), false));
        }

        [TestMethod]
        public void VetoPowerIsAvailableWithFiveFascistPolicies()
        {
            var president = Players.First();
            president.IsPresident = true;
            var chancellor = Players.Skip(1).First();
            chancellor.IsChancellor = true;
            StateMachine.DrawnPolicies = new List<PolicyType>() { PolicyType.Fascist, PolicyType.Fascist };
            StateMachine.MachineState = StateMachineState.AwaitingPresidentialPolicies;
            GameData.EnactedFascistPolicyCount = Constants.MinFascistPolicyCountForVeto;
            StateMachine.PoliciesSelected(new[] { PolicyType.Fascist, PolicyType.Liberal });
            Director.Verify(_ => _.GetEnactedPolicy(chancellor, It.IsAny<IEnumerable<PolicyType>>(), true));
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
