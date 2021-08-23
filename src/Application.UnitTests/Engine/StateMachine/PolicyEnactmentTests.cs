using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using SecretHitler.Application.LegacyEngine;
using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.UnitTests.Engine.StateMachine
{
    public class PolicyEnactmentTests : GameStateMachineTestFixture
    {
        [Test]
        public void VetoingPolicyIncreasesElectionTracker()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void VetoingPolicyCausesChaosIfThresholdExceeded()
        {
            Assert.Inconclusive();
        }

        [Test]
        public async Task VetoPowerIsUnavailableWithFewerThanFiveFascistPolicies()
        {
            var president = Players.First();
            president.IsPresident = true;
            var chancellor = Players.Skip(1).First();
            chancellor.IsChancellor = true;
            GameData.DrawnPolicies = new List<PolicyType>() { PolicyType.Fascist, PolicyType.Fascist };
            GameData.MachineState = StateMachineState.AwaitingPresidentialPolicies;
            GameData.EnactedFascistPolicyCount = Constants.MinFascistPolicyCountForVeto - 1;
            await StateMachine.PoliciesSelected(new[] { PolicyType.Fascist, PolicyType.Liberal });
            Director.Verify(_ => _.GetEnactedPolicy(chancellor, It.IsAny<IEnumerable<PolicyType>>(), false));
        }

        [Test]
        public async Task VetoPowerIsAvailableWithFiveFascistPolicies()
        {
            var president = Players.First();
            president.IsPresident = true;
            var chancellor = Players.Skip(1).First();
            chancellor.IsChancellor = true;
            GameData.DrawnPolicies = new List<PolicyType>() { PolicyType.Fascist, PolicyType.Fascist };
            GameData.MachineState = StateMachineState.AwaitingPresidentialPolicies;
            GameData.EnactedFascistPolicyCount = Constants.MinFascistPolicyCountForVeto;
            await StateMachine.PoliciesSelected(new[] { PolicyType.Fascist, PolicyType.Liberal });
            Director.Verify(_ => _.GetEnactedPolicy(chancellor, It.IsAny<IEnumerable<PolicyType>>(), true));
        }

        [Test]
        public void ExecutedPlayerIsNoLongerAlive()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void PolicyPeekPowerDoesNotAffectDrawPile()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void InvestigateLoyaltyProvidesPlayersLoyaltyCardOnly()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void PlayerCannotBeChosenForInvestigationMoreThanOnce()
        {
            Assert.Inconclusive();
        }
    }
}
