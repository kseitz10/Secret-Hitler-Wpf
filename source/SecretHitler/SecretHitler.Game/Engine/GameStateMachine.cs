using System;
using System.Collections.Generic;
using System.Linq;
using SecretHitler.Game.Entities;
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
        /// <summary>
        /// Constructs a new instance of the game state machine.
        /// </summary>
        public GameStateMachine(IClientProxy clientProxy, GameData gameData = null)
        {
            ClientProxy = clientProxy;
            GameData = gameData ?? new GameData();
            GameDataManipulator = new GameDataManipulator(GameData);
            PolicyDeck = new PolicyDeck(GameData.DrawPile, GameData.DiscardPile, gameData == null);
        }

        #region Properties

        /// <summary>
        /// The data backing the state machine.
        /// </summary>
        public Entities.GameData GameData { get; }

        /// <summary>
        /// Gets the object responsible for sending a message to one or more clients.
        /// </summary>
        public IClientProxy ClientProxy { get; set; }

        /// <summary>
        /// The current state of the machine.
        /// </summary>
        public StateMachineState MachineState { get; internal set; } = StateMachineState.None;

        /// <summary>
        /// The policy draw and discard pile manager.
        /// </summary>
        internal ICardDeck<PolicyType> PolicyDeck { get; set; }

        /// <summary>
        /// Utility for managing the game state data.
        /// </summary>
        internal GameDataManipulator GameDataManipulator { get; set; }

        #endregion

        #region Client Response Methods

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Start()
        {
            if (MachineState != StateMachineState.None)
                throw new GameStateException("Game already in progress.");

            PrepareGame();
        }

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
                    GameData.Chancellor = CoercePlayer(player);
                    MachineState = StateMachineState.AwaitingVotes;
                    ClientProxy.GetVotes(GameData.Players.Where(_ => _.IsAlive));
                    break;

                case StateMachineState.AwaitingSpecialElectionPick:
                    PresidentialSuccession(false, player);
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
            var myPolicies = policies.ToList();
            switch (MachineState)
            {
                case StateMachineState.AwaitingEnactedPolicy:
                    // TODO Validate policy was actually drawn
                    if (myPolicies.Count != 1)
                        throw new GameStateException("Too many policies selected for the current game state.");

                    var policy = myPolicies.First();
                    if (policy == PolicyType.Fascist)
                    {
                        GameData.EnactedFascistPolicyCount++;
                    }
                    else if (policy == PolicyType.Liberal)
                    {
                        GameData.EnactedLiberalPolicyCount++;
                    }

                    if (policy != PolicyType.None)
                    {
                        // TODO Presidential power
                        // TODO Win condition check? Other stuff?

                        PresidentialSuccession();
                    }
                    else
                    {
                        // TODO Veto
                    }

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

            var pass = votes.Count(_ => _);
            var fail = votes.Count() - pass;

            if (pass > fail)
            {
                MachineState = StateMachineState.AwaitingPresidentialPolicies;
                ClientProxy.GetPresidentialPolicies(GameData.President, PolicyDeck.Draw(Constants.PresidentialPolicyDrawCount));
            }
            else
            {
                if (GameDataManipulator.UpdateElectionTracker())
                {
                    // TODO Policy win condition check.
                }
                else
                {
                    PresidentialSuccession(true);
                }
            }
        }
    
        #endregion

        #region Private Methods

        private void PrepareGame()
        {
            GameDataManipulator.ResetGame();
            PresidentialSuccession();
        }

        private void PresidentialSuccession(bool failedElection = false, IPlayerInfo specialElectionPresident = null)
        {
            if (failedElection)
                GameDataManipulator.UpdateElectionTracker();
            else
                GameDataManipulator.UpdateTermLimits();

            GameData.Chancellor = null;

            if (specialElectionPresident != null)
            {
                GameData.President = GameData.Players.First(_ => _.Identifier == specialElectionPresident.Identifier);
            }
            else
            {
                GameDataManipulator.GetPresidentFromQueue();
            }

            var candidates = GameData.Players.Where(_ => _.IsAlive && !_.IsPresident && !GameData.IneligibleChancellors.Contains(_.Identifier)).ToArray();
            MachineState = StateMachineState.AwaitingNomination;
            ClientProxy.SelectPlayer(GameData.President, GameState.ChancellorNomination, candidates);
        }

        private PlayerData CoercePlayer(IPlayerInfo player) => GameData.Players.Single(_ => _.Identifier == player.Identifier);

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
