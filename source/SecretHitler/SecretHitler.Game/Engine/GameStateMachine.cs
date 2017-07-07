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
    public partial class GameStateMachine : IPlayerResponseHandler
    {
        /// <summary>
        /// Constructs a new instance of the game state machine.
        /// </summary>
        public GameStateMachine(IPlayerDirector director, GameData gameData = null)
        {
            Director = director;
            GameData = gameData ?? new GameData();
            GameDataManipulator = new GameDataManipulator(GameData);
            PolicyDeck = new PolicyDeck(GameData.DrawPile, GameData.DiscardPile, gameData == null);
        }

        #region Properties

        private object _voteLock = new object();

        /// <summary>
        /// The data backing the state machine.
        /// </summary>
        public Entities.GameData GameData { get; }

        /// <summary>
        /// Gets the object responsible for sending a message to one or more clients.
        /// </summary>
        public IPlayerDirector Director { get; set; }

        /// <summary>
        /// The current state of the machine.
        /// </summary>
        public StateMachineState MachineState { get; internal set; } = StateMachineState.None;

        /// <summary>
        /// Whether game is in progress.
        /// </summary>
        public bool GameInProgress => MachineState != StateMachineState.None;

        /// <summary>
        /// The policy draw and discard pile manager.
        /// </summary>
        internal ICardDeck<PolicyType> PolicyDeck { get; set; }

        /// <summary>
        /// Utility for managing the game state data.
        /// </summary>
        internal GameDataManipulator GameDataManipulator { get; set; }

        /// <summary>
        /// Ja vote count
        /// </summary>
        internal int JaVoteCount { get; set; }

        /// <summary>
        /// Nein vote count
        /// </summary>
        internal int NeinVoteCount { get; set; }

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
        /// Sends updated game data to players.
        /// </summary>
        public void DisseminateGameData()
        {
            foreach (var p in GameData.Players)
                Director.UpdateGameData(p, PrepareGameDataForPlayerDissemination(p));
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
                        Director.Broadcast("The policies were successfully vetoed!");
                    }
                    else
                    {
                        Director.Broadcast("Unsuccessful veto. The chancellor must choose a policy.");
                    }

                    break;

                case StateMachineState.AwaitingPolicyPeekConfirmation:
                    Director.Broadcast("The president has seen the topmost policies on the draw pile.");
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
        public void PlayerSelected(Guid player)
        {
            switch (MachineState)
            {
                case StateMachineState.AwaitingNomination:
                    GameData.Chancellor = CoercePlayer(player);
                    Director.Broadcast($"The president has nominated {GameData.Chancellor.Name} as chancellor.");
                    MachineState = StateMachineState.AwaitingVotes;
                    DisseminateGameData();
                    Director.Broadcast("It is time to vote.");
                    Director.GetVotes(GameData.Players.Where(_ => _.IsAlive).AsGuids());
                    break;

                case StateMachineState.AwaitingSpecialElectionPick:
                    PresidentialSuccession(false, CoercePlayer(player));
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
                    // TODO Validate policy was actually drawn, delivered by correct player
                    if (myPolicies.Count != Constants.ChancellorPolicySelectionCount)
                        throw new GameStateException("Too many policies selected for the current game state.");

                    var policy = myPolicies.First();
                    if (policy == PolicyType.Fascist)
                    {
                        Director.Broadcast("A fascist policy has been enacted!");
                        GameData.EnactedFascistPolicyCount++;
                    }
                    else if (policy == PolicyType.Liberal)
                    {
                        Director.Broadcast("A liberal policy has been enacted!");
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
                        Director.Broadcast("A veto has been requested.");
                        // TODO Veto and dissemination
                    }

                    break;
                case StateMachineState.AwaitingPresidentialPolicies:
                    // TODO Validate policy was actually drawn, delivered by correct player
                    // TODO Test me.
                    if (myPolicies.Count != Constants.PresidentialPolicyPassCount)
                        throw new GameStateException("Too many/few policies selected for the current game state.");

                    Director.Broadcast("The president has offered policies to the chancellor.");
                    MachineState = StateMachineState.AwaitingEnactedPolicy;
                    Director.GetEnactedPolicy(GameData.Chancellor, policies);

                    break;
                default:
                    throw new GameStateException($"{nameof(PoliciesSelected)} called for invalid state {MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that a vote has been collected.
        /// </summary>
        /// <param name="vote">The collected vote.</param>
        public void VoteCollected(bool vote)
        {
            lock (_voteLock)
            {
                if (MachineState != StateMachineState.AwaitingVotes)
                    throw new GameStateException($"{nameof(VoteCollected)} called for invalid state {MachineState}.");

                if (vote)
                    JaVoteCount++;
                else
                    NeinVoteCount++;

                if (JaVoteCount + NeinVoteCount < GameData.Players.Count(_ => _.IsAlive))
                    return;

                var message = $"Votes have been tallied: {JaVoteCount} ja, {NeinVoteCount} nein.";
                if (JaVoteCount > NeinVoteCount)
                {
                    Director.Broadcast($"{message} The election was successful.");
                    MachineState = StateMachineState.AwaitingPresidentialPolicies;
                    DisseminateGameData();
                    Director.GetPresidentialPolicies(GameData.President, PolicyDeck.Draw(Constants.PresidentialPolicyDrawCount));
                }
                else
                {
                    message = $"{message} The election failed.";
                    var chaos = GameDataManipulator.UpdateElectionTracker();
                    DisseminateGameData();
                    if (chaos)
                    {
                        Director.Broadcast($"{message} Due to inactive government, there is chaos on the streets!");
                        // TODO Policy win condition check.
                        // TODO Update state. TEST ME!
                    }
                    else
                    {
                        Director.Broadcast($"{message} The election tracker is now at {GameData.ElectionTracker}. When it reaches {Constants.FailedElectionThreshold}, a policy will be enacted.");
                        PresidentialSuccession(true);
                    }
                }

                JaVoteCount = 0;
                NeinVoteCount = 0;
            }
        }
    
        #endregion

        #region Private Methods

        private GameData PrepareGameDataForPlayerDissemination(IPlayerInfo player)
        {
            // This data can be given to all players.
            var gd = new GameData
            {
                GameGuid = GameData.GameGuid,
                EnactedFascistPolicyCount = GameData.EnactedFascistPolicyCount,
                EnactedLiberalPolicyCount = GameData.EnactedLiberalPolicyCount,
                ElectionTracker = GameData.ElectionTracker,
                PresidentialQueue = new Queue<Guid>(GameData.PresidentialQueue)
            };

            var canDiscloseFascists = player.Role == PlayerRole.Fascist ||
                (player.Role == PlayerRole.Hitler && GameData.Players.Count <= 6);
            var canDiscloseHitler = player.Role != PlayerRole.Liberal;

            gd.Players.AddRange(GameData.Players.Select(p =>
            {
                var rtn = new PlayerData()
                {
                    Identifier = p.Identifier,
                    IsAlive = p.IsAlive,
                    IsChancellor = p.IsChancellor,
                    IsPresident = p.IsPresident,
                    Name = p.Name
                };

                if (p.Role == PlayerRole.Hitler && canDiscloseHitler)
                    rtn.Role = PlayerRole.Hitler;
                else if (p.Role == PlayerRole.Fascist && canDiscloseFascists)
                    rtn.Role = PlayerRole.Fascist;
                else if (p.HasSameIdentifierAs(player))
                    rtn.Role = PlayerRole.Liberal;

                return rtn;
            }));

            return gd;
        }

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

            DisseminateGameData();
            Director.Broadcast($"{GameData.President.Name} is now president and will nominate a chancellor.");
            Director.SelectPlayer(GameData.President, GameState.ChancellorNomination, candidates.AsGuids());
        }

        private PlayerData CoercePlayer(IPlayerInfo player) => GameData.Players.Single(_ => _.Identifier == player.Identifier);

        private PlayerData CoercePlayer(Guid guid) => GameData.Players.Single(_ => _.Identifier == guid);

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
