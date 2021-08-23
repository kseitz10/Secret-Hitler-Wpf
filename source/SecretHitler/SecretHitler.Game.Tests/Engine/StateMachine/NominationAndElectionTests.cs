using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;
using SecretHitler.Game.Entities;
using System;
using System.Threading.Tasks;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    [TestClass]
    public class NominationAndElectionTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public async Task SpecialElectionUsesDesignatedPlayer()
        {
            Manipulator.Object.ResetGame();
            var currentPresident = Manipulator.Object.GetPresidentFromQueue();
            var expected = Players.First(_ => _.Identifier != currentPresident.Identifier);
            GameData.MachineState = StateMachineState.AwaitingSpecialElectionPick;
            await StateMachine.PlayerSelected(expected);
            Assert.AreEqual(expected, GameData.President, "President should be updated");
        }

        [TestMethod]
        public async Task SpecialElectionDoesNotAffectPresidentialQueue()
        {
            Manipulator.Object.ResetGame();
            var currentPresident = Manipulator.Object.GetPresidentFromQueue();
            var nextPresident = GameData.PresidentialQueue.Peek();
            var unexpected = Players.First(_ => _.Identifier != currentPresident.Identifier && _.Identifier != nextPresident);
            GameData.MachineState = StateMachineState.AwaitingSpecialElectionPick;
            await StateMachine.PlayerSelected(unexpected);
            Assert.AreEqual(nextPresident, GameData.PresidentialQueue.Peek(), "Next president should not change");
        }

        [TestMethod]
        public async Task ChancellorCandidateListOnlyContainsValidPlayers()
        {
            ResetPlayers(Constants.MaxPlayerCount);
            Manipulator.Object.ResetGame();

            // Fiddle with the data to look like we're mid-game and the next election fails.
            var temp = Players.ToArray();
            temp.Shuffle();
            var eligibleVoters = new Queue<PlayerData>(temp);
            var failedPresident = eligibleVoters.Dequeue();
            failedPresident.IsPresident = true;
            var failedChancellor = eligibleVoters.Dequeue();
            GameData.IneligibleChancellors.Add(eligibleVoters.Dequeue());
            GameData.IneligibleChancellors.Add(eligibleVoters.Dequeue());
            eligibleVoters.Dequeue().IsAlive = false;
            eligibleVoters.Dequeue().IsAlive = false;
            GameData.MachineState = StateMachineState.AwaitingNomination;
            await StateMachine.PlayerSelected(failedChancellor);
            eligibleVoters.Enqueue(failedPresident);
            eligibleVoters.Enqueue(failedChancellor);

            var expected = eligibleVoters.ToList();

            // TODO this whole test is kind of gross.
            var success = false;
            Director
                .Setup(_ => _.SelectPlayer(It.IsAny<Guid>(), GameState.ChancellorNomination, It.IsAny<IEnumerable<Guid>>()))
                .Callback(new Action<Guid, GameState, IEnumerable<Guid>>((pres, state, guids) =>
            {
                var candidates = guids.AsPlayers(Players);
                success = true;
                expected.Remove(GameData.President);

                if (candidates.Contains(GameData.President))
                    Assert.Fail("Should not contain president.");

                if (candidates.Any(_ => !_.IsAlive))
                    Assert.Fail("Should not contain dead players.");

                if (candidates.Any(c => GameData.IneligibleChancellors.Any(ic => ic.Equals(c))))
                    Assert.Fail("Should not contain ineligible players (term limits)");

                // This assertion would be sufficient on its own, but the checks above provide better test results in event of failure.
                if (!expected.OrderBy(_ => _.Identifier).SequenceEqual(candidates.OrderBy(_ => _.Identifier)))
                    Assert.Fail("Expected candidates sequence did not match actual. Some candidates may have been missing.");
            }));

            // Fail the election and investigate the resulting candidates offered to the next president.
            await VotesCollected(Enumerable.Range(0, Players.Count(_ => _.IsAlive)).Select(_ => false));

            if (!success)
                Assert.Fail("Did not get correct candidate list.");
        }

        [TestMethod]
        public async Task SelectingNominationTriggersVotingStage()
        {
            var president = Players.First();
            var nomination = Players.Skip(1).First();
            Players.Skip(2).First().IsAlive = false;
            GameData.MachineState = StateMachineState.AwaitingNomination;
            await StateMachine.PlayerSelected(nomination);
            Assert.AreEqual(nomination, GameData.Chancellor, "Chancellor should be assigned");
            Assert.AreEqual(GameData.MachineState, StateMachineState.AwaitingVotes);
            Director.Verify(prox => prox.GetVotes(It.Is<IEnumerable<Guid>>(voters => voters.Count() == Players.Count(_ => _.IsAlive) && voters.AsPlayers(Players).All(v => v.IsAlive))));
        }

        [TestMethod]
        public async Task MajorityVoteCompletesElectionAndDeliversPolicies()
        {
            const int Pass = 4;
            const int Fail = 3;
            ResetPlayers(Pass + Fail);

            var president = Players.First();
            president.IsPresident = true;
            GameData.MachineState = StateMachineState.AwaitingVotes;
            GameData.ElectionTracker = 1;

            await VotesCollected(Enumerable.Range(0, Pass).Select(_ => true).Concat(Enumerable.Range(0, Fail).Select(_ => false)));

            Assert.AreEqual(GameData.MachineState, StateMachineState.AwaitingPresidentialPolicies);
            Assert.AreEqual(1, GameData.ElectionTracker, "Election tracker should not reset until policy is passed.");
            Director.Verify(_ => _.GetPresidentialPolicies(president, It.Is<IEnumerable<PolicyType>>(d => d.Count() == Constants.PresidentialPolicyDrawCount)));
        }

        [TestMethod]
        public async Task NonMajorityVoteFailsElection()
        {
            const int Pass = 3;
            const int Fail = 3;
            ResetPlayers(Pass + Fail);
            Manipulator.Object.ResetGame();

            var president = Players.First();
            president.IsPresident = true;
            GameData.MachineState = StateMachineState.AwaitingVotes;
            GameData.ElectionTracker = 1;

            await VotesCollected(Enumerable.Range(0, Pass).Select(_ => true).Concat(Enumerable.Range(0, Fail).Select(_ => false)));

            Assert.AreEqual(GameData.MachineState, StateMachineState.AwaitingNomination);
            Manipulator.Verify(_ => _.UpdateElectionTracker(null));
            Manipulator.Verify(_ => _.GetPresidentFromQueue());
        }

        [TestMethod]
        public void ChaosHandlesLiberalPolicyWinCondition()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ChaosHandlesFascistPolicyWinCondition()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void ChaosEnactsPolicyWithoutPresidentialPower()
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
