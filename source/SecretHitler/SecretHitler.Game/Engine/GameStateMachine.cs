using System;
using System.Collections.Generic;
using System.Linq;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;

namespace SecretHitler.Game.Engine
{
    /// <summary>
    /// State machine for the Secret Hitler game.
    /// </summary>
    public partial class GameStateMachine : IClientResponseReceiver
    {
        #region Properties

        /// <summary>
        /// The data backing the state machine.
        /// </summary>
        public Entities.Game GameData { get; private set; } = new Entities.Game();

        /// <summary>
        /// Gets the object responsible for sending a message to one or more clients.
        /// </summary>
        public IClientProxy ClientProxy { get; private set; }

        /// <summary>
        /// The number of fascist roles (not including Hitler) to be assigned given the
        /// current number of players.
        /// </summary>
        public int FascistCount => GameData.Players.Count - 4 + ((GameData.Players.Count - 5) / 2);

        /// <summary>
        /// The current state of the machine.
        /// </summary>
        public StateMachineState MachineState { get; internal set; } = StateMachineState.None;

        #endregion

        #region Client Response Methods

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        public void Acknowledge(bool? acknowledge)
        {
            switch (MachineState)
            {
                case StateMachineState.AwaitingVetoResponse:
                    if (!acknowledge.HasValue)
                    {
                        throw new GameStateException("Expecting true or false response for veto approval, not null.");
                    }
                    else if (acknowledge.Value)
                    {

                    }
                    else
                    {

                    }

                    break;

                case StateMachineState.AwaitingPolicyPeekConfirmation:
                    PresidentialSuccession();
                    break;

                default:
                    throw new GameStateException($"{nameof(Acknowledge)} called for invalid state {MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that a player has been selected by the client that was last issued a request.
        /// </summary>
        /// <param name="player">The selected player.</param>
        public void PlayerSelected(IPlayerInfo player)
        {
            switch (MachineState)
            {
                case StateMachineState.AwaitingNomination:

                    break;
                case StateMachineState.AwaitingSpecialElectionPick:

                    break;
                case StateMachineState.AwaitingExecution:

                    break;
                default:
                    throw new GameStateException($"{nameof(PlayerSelected)} called for invalid state {MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that one or more policies were selected by the client asked to select policies.
        /// </summary>
        /// <param name="policies">The selected policies.</param>
        public void PoliciesSelected(IEnumerable<PolicyType> policies)
        {
            switch (MachineState)
            {
                case StateMachineState.AwaitingEnactedPolicy:

                    break;
                case StateMachineState.AwaitingPresidentialPolicies:

                    break;
                default:
                    throw new GameStateException($"{nameof(PoliciesSelected)} called for invalid state {MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that votes have been collected from all active players.
        /// </summary>
        /// <param name="votes">The collected votes.</param>
        public void VotesCollected(IEnumerable<bool> votes)
        {
            if (MachineState != StateMachineState.AwaitingVotes)
                throw new GameStateException($"{nameof(VotesCollected)} called for invalid state {MachineState}.");
        }

        #endregion

        #region Private Methods

        private void PrepareGame()
        {
            // Mark all players as alive and assign them some roles
            foreach (var player in GameData.Players)
            {
                player.Role = PlayerRole.Liberal;
                player.IsAlive = true;
            }

            var liberals = GameData.Players.ToArray();
            liberals.Shuffle();

            foreach (var fascist in liberals.Take(FascistCount))
                fascist.Role = PlayerRole.Fascist;

            liberals.Skip(FascistCount).First().Role = PlayerRole.Hitler;

            // Set up the presidential queue
            var shuffled = GameData.Players.Select(_ => _.Identifier).ToList();
            shuffled.Shuffle();
            GameData.PresidentialQueue = new Queue<Guid>(shuffled);

            GameData.IneligibleChancellors = new List<Guid>();
            GameData.EnactedFascistPolicyCount = 0;
            GameData.EnactedLiberalPolicyCount = 0;
            GameData.ElectionTracker = 0;
            GameData.PolicyDeck.Reset();
        }

        private void PresidentialSuccession(bool failedElection = false, IPlayerInfo specialElectionPresident = null)
        {
            if (failedElection)
            {
                HandleFailedElection();
            }
            else
            {
                GameData.IneligibleChancellors.Clear();

                if (GameData.Players.Count(_ => _.IsAlive) > 5)
                    GameData.IneligibleChancellors.Add(GameData.President);

                GameData.IneligibleChancellors.Add(GameData.Chancellor);
            }

            GameData.Chancellor = null;

            if (specialElectionPresident != null)
            {
                GameData.President = GameData.Players.First(_ => _.Identifier == specialElectionPresident.Identifier);
            }
            else
            {
                var nextPresidentGuid = GameData.PresidentialQueue.Dequeue();
                GameData.PresidentialQueue.Enqueue(nextPresidentGuid);

                var nextPresident = GameData.Players.First(_ => _.Identifier == nextPresidentGuid);
                GameData.President = nextPresident;
            }

            var candidates = GameData.Players.Where(_ => _.IsAlive && !_.IsPresident && !GameData.IneligibleChancellors.Contains(_.Identifier)).ToArray();
            MachineState = StateMachineState.AwaitingNomination;
            ClientProxy.SelectPlayer(GameData.President, GameState.ChancellorNomination, candidates);
        }

        private void HandleFailedElection()
        {
            GameData.ElectionTracker++;

            if (GameData.ElectionTracker >= 3)
            {
                GameData.ElectionTracker = 0;
                GameData.IneligibleChancellors.Clear();
                var drawnPolicy = GameData.PolicyDeck.Draw();
                Enact(drawnPolicy.Single(), true);
                
                // TODO Verify additional work to do here
            }
        }

        private void Enact(PolicyType policy, bool ignorePower)
        {
            if (policy == PolicyType.None)
                throw new ArgumentException("Cannot use none.", nameof(policy));

            GameData.EnactedPolicies.Add(policy);

            if (!ignorePower)
            {
                // TODO Use power.
            }
            
            // TODO Check win condition?
        }

        #endregion

        private enum Trigger
        {
            StartGame,
            VetoApproved,
            VetoDenied,
            PoliciesSelected,
            VotesCollected,
            PlayerSelected
        }
    }
}
