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

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    [TestClass]
    public class NominationAndElectionTests : GameStateMachineTestFixture
    {
        [TestMethod]
        public void SpecialElectionUsesDesignatedPlayer()
        {
            Manipulator.Object.ResetGame();
            var currentPresident = Manipulator.Object.GetPresidentFromQueue();
            var expected = Players.First(_ => _.Identifier != currentPresident.Identifier);
            StateMachine.MachineState = StateMachineState.AwaitingSpecialElectionPick;
            StateMachine.PlayerSelected(expected);
            Assert.AreEqual(expected, StateMachine.GameData.President, "President should be updated");
        }

        [TestMethod]
        public void SpecialElectionDoesNotAffectPresidentialQueue()
        {
            Manipulator.Object.ResetGame();
            var currentPresident = Manipulator.Object.GetPresidentFromQueue();
            var nextPresident = StateMachine.GameData.PresidentialQueue.Peek();
            var unexpected = Players.First(_ => _.Identifier != currentPresident.Identifier && _.Identifier != nextPresident);
            StateMachine.MachineState = StateMachineState.AwaitingSpecialElectionPick;
            StateMachine.PlayerSelected(unexpected);
            Assert.AreEqual(nextPresident, StateMachine.GameData.PresidentialQueue.Peek(), "Next president should not change");
        }

        [TestMethod]
        public void ChancellorCandidateListOnlyContainsValidPlayers()
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
            StateMachine.MachineState = StateMachineState.AwaitingNomination;
            StateMachine.PlayerSelected(failedChancellor);
            eligibleVoters.Enqueue(failedPresident);
            eligibleVoters.Enqueue(failedChancellor);

            var expected = eligibleVoters.ToList();

            // TODO this whole test is kind of gross.
            var success = false;
            ClientProxy
                .Setup(_ => _.SelectPlayer(It.IsAny<IPlayerInfo>(), GameState.ChancellorNomination, It.IsAny<IEnumerable<IPlayerInfo>>()))
                .Callback(new Action<IPlayerInfo, GameState, IEnumerable<IPlayerInfo>>((pres, state, candidates) =>
            {
                success = true;
                expected.Remove(GameData.President);

                if (candidates.Contains(GameData.President))
                    Assert.Fail("Should not contain president.");

                if (candidates.Any(_ => !_.IsAlive))
                    Assert.Fail("Should not contain dead players.");

                if (candidates.Any(_ => GameData.IneligibleChancellors.Any(__ => __.Equals(_))))
                    Assert.Fail("Should not contain ineligible players (term limits)");

                // This assertion would be sufficient on its own, but the checks above provide better test results in event of failure.
                if (!expected.OrderBy(_ => _.Identifier).SequenceEqual(candidates.OrderBy(_ => _.Identifier)))
                    Assert.Fail("Expected candidates sequence did not match actual. Some candidates may have been missing.");
            }));

            // Fail the election and investigate the resulting candidates offered to the next president.
            StateMachine.VotesCollected(Enumerable.Range(0, Players.Count(_ => _.IsAlive)).Select(_ => false));

            if (!success)
                Assert.Fail("Did not get correct candidate list.");
        }

        [TestMethod]
        public void SelectingNominationTriggersVotingStage()
        {
            var president = Players.First();
            var nomination = Players.Skip(1).First();
            Players.Skip(2).First().IsAlive = false;
            StateMachine.MachineState = StateMachineState.AwaitingNomination;
            StateMachine.PlayerSelected(nomination);
            Assert.AreEqual(nomination, GameData.Chancellor, "Chancellor should be assigned");
            Assert.AreEqual(StateMachine.MachineState, StateMachineState.AwaitingVotes);
            ClientProxy.Verify(prox => prox.GetVotes(It.Is<IEnumerable<IPlayerInfo>>(voters => voters.Count() == Players.Count(_ => _.IsAlive) && voters.All(v => v.IsAlive))));
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
            GameData.ElectionTracker = 1;

            StateMachine.VotesCollected(Enumerable.Range(0, Pass).Select(_ => true).Concat(Enumerable.Range(0, Fail).Select(_ => false)));

            Assert.AreEqual(StateMachine.MachineState, StateMachineState.AwaitingPresidentialPolicies);
            Assert.AreEqual(1, GameData.ElectionTracker, "Election tracker should not reset until policy is passed.");
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
            GameData.ElectionTracker = 1;

            StateMachine.VotesCollected(Enumerable.Range(0, Pass).Select(_ => true).Concat(Enumerable.Range(0, Fail).Select(_ => false)));

            Assert.AreEqual(StateMachine.MachineState, StateMachineState.AwaitingNomination);
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
