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

        /// <summary>
        /// The last policies drawn from the deck. This accommodates discarding cards when getting a response
        /// from the player.
        /// </summary>
        private List<PolicyType> DrawnPolicies { get; set; }

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
                        // TODO test
                        PolicyDeck.Discard(DrawnPolicies);
                        DrawnPolicies = null;
                        UpdateElectionTrackerAndHandleChaos("The policies were successfully vetoed!");
                    }
                    else
                    {
                        // TODO Implement and test
                        Director.Broadcast("Unsuccessful veto. The chancellor must choose a policy.");
                    }

                    break;

                case StateMachineState.AwaitingSpecialPowerAcknowledgment:
                    PrepareNextElection();
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
                    PrepareNextElection(specialElectionPresident: CoercePlayer(player));
                    break;

                case StateMachineState.AwaitingExecutionPick:
                    var playerToKill = CoercePlayer(player);
                    Director.Broadcast($"The president has chosen to execute {playerToKill.Name}.");
                    playerToKill.IsAlive = false;
                    PrepareNextElection();
                    break;

                case StateMachineState.AwaitingInvestigateLoyaltyPick:
                    var playerToReveal = CoercePlayer(player);
                    Director.Broadcast($"The president is now aware of the loyalty of {playerToReveal.Name}.");
                    Director.Reveal(GameData.President, playerToReveal.Identifier, playerToReveal.Role == PlayerRole.Liberal ? PlayerRole.Liberal : PlayerRole.Fascist);
                    MachineState = StateMachineState.AwaitingSpecialPowerAcknowledgment;
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

                    // Discard if a policy was selected.
                    if(policy != PolicyType.None)
                    {
                        foreach (var p in myPolicies)
                            DrawnPolicies.Remove(p);

                        PolicyDeck.Discard(DrawnPolicies);
                        DrawnPolicies = null;
                    }

                    if (policy == PolicyType.Fascist)
                    {
                        Director.Broadcast("A fascist policy has been enacted!");
                        GameData.EnactedFascistPolicyCount++;
                        DisseminateGameData();

                        if (InvokeCurrentPresidentialPower())
                            return;
                    }
                    else if (policy == PolicyType.Liberal)
                    {
                        Director.Broadcast("A liberal policy has been enacted!");
                        GameData.EnactedLiberalPolicyCount++;
                    }

                    if (policy != PolicyType.None)
                    {
                        PrepareNextElection();
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

                    foreach (var p in myPolicies)
                        DrawnPolicies.Remove(p);

                    PolicyDeck.Discard(DrawnPolicies);
                    DrawnPolicies = policies.ToList();

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

                    if(GameData.EnactedFascistPolicyCount >= Constants.RequiredFascistPoliciesForHitlerChancellorshipVictory && GameData.Chancellor.Role == PlayerRole.Hitler)
                    {
                        MachineState = StateMachineState.None;
                        Director.Broadcast("Congratulations on becoming chancellor, Hitler. FASCIST VICTORY!");
                        DisseminateGameData();
                        return;
                    }

                    MachineState = StateMachineState.AwaitingPresidentialPolicies;
                    DisseminateGameData();
                    DrawnPolicies = PolicyDeck.Draw(Constants.PresidentialPolicyDrawCount).ToList();
                    Director.GetPresidentialPolicies(GameData.President, DrawnPolicies);
                }
                else
                {
                    message = $"{message} The election failed.";
                    UpdateElectionTrackerAndHandleChaos(message);
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
            JaVoteCount = 0;
            NeinVoteCount = 0;
            PrepareNextElection();
        }

        private void PrepareNextElection(bool updateTermLimits = true, IPlayerInfo specialElectionPresident = null)
        {
            if (!GameData.Players.First(_ => _.Role == PlayerRole.Hitler).IsAlive)
            {
                MachineState = StateMachineState.None;
                Director.Broadcast("HITLER IS DEAD! LIBERAL VICTORY!");
                DisseminateGameData();
                return;
            }
            else if (GameData.EnactedLiberalPolicyCount == Constants.LiberalPoliciesRequiredForVictory)
            {
                MachineState = StateMachineState.None;
                Director.Broadcast("All liberal policies have been enacted. LIBERAL VICTORY!");
                DisseminateGameData();
                return;
            }
            else if (GameData.EnactedFascistPolicyCount == Constants.FascistPoliciesRequiredForVictory)
            {
                MachineState = StateMachineState.None;
                Director.Broadcast("All fascist policies have been enacted. FASCIST VICTORY!");
                DisseminateGameData();
                return;
            }

            if (updateTermLimits)
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

        private void UpdateElectionTrackerAndHandleChaos(string messagePrefix)
        {
            var chaos = GameDataManipulator.UpdateElectionTracker();
            DisseminateGameData();
            if (chaos)
            {
                Director.Broadcast($"{messagePrefix} Due to inactive government, there is chaos on the streets!".Trim());
                PrepareNextElection(false);
            }
            else
            {
                Director.Broadcast($"{messagePrefix} The election tracker is now at {GameData.ElectionTracker}. When it reaches {Constants.FailedElectionThreshold}, a policy will be enacted.".Trim());
                PrepareNextElection(false);
            }
        }

        /// <summary>
        /// If a new fascist policy has been enacted, this method should be invoked to access
        /// the unlocked special power. If there is a special power, the method returns true to
        /// indicate that the calling code should not start the next election cycle.
        /// </summary>
        private bool InvokeCurrentPresidentialPower()
        {
            var delegates = new Dictionary<int, Action>();
            var index = GameData.EnactedFascistPolicyCount - 1;
            delegates[3] = InvokeExecution;
            delegates[4] = InvokeExecution;

            switch (GameData.Players.Count())
            {
                case 5:
                case 6:
                    delegates[2] = InvokePolicyPeek;
                    break;

                case 7:
                case 8:
                    delegates[1] = InvokeInvestigateLoyalty;
                    delegates[2] = InvokeSpecialElection;
                    break;

                case 9:
                case 10:
                    delegates[0] = InvokeInvestigateLoyalty;
                    delegates[1] = InvokeInvestigateLoyalty;
                    delegates[2] = InvokeSpecialElection;
                    break;

                default:
                    throw new NotImplementedException("Unsupported player count when handling presidential power.");
            }

            if (delegates.ContainsKey(index))
            {
                delegates[index]();
                return true;
            }

            return false;
        }

        private void InvokePolicyPeek()
        {
            var policiesToDisplay = PolicyDeck.Peek(Constants.PresidentialPolicyDrawCount);
            MachineState = StateMachineState.AwaitingSpecialPowerAcknowledgment;
            Director.Broadcast($"The president is being shown the topmost {Constants.PresidentialPolicyDrawCount} policies on the draw pile.");
            Director.PolicyPeek(GameData.President, policiesToDisplay);
        }

        private void InvokeExecution()
        {
            MachineState = StateMachineState.AwaitingExecutionPick;
            Director.Broadcast("The president is choosing a player to execute.");
            Director.SelectPlayer(GameData.President, GameState.Execution, GameData.Players.Where(_ => _.IsAlive && !_.HasSameIdentifierAs(GameData.President)).Select(_ => _.Identifier));
        }

        private void InvokeInvestigateLoyalty()
        {
            MachineState = StateMachineState.AwaitingInvestigateLoyaltyPick;
            Director.Broadcast("The president is choosing a player to investigate his/her loyalty.");
            Director.SelectPlayer(GameData.President, GameState.InvestigateLoyalty, GameData.Players.Where(_ => _.IsAlive && !_.HasSameIdentifierAs(GameData.President)).Select(_ => _.Identifier));
        }

        private void InvokeSpecialElection()
        {
            MachineState = StateMachineState.AwaitingSpecialElectionPick;
            Director.Broadcast("The president is choosing a player to serve as president in a special election.");
            Director.SelectPlayer(GameData.President, GameState.SpecialElection, GameData.Players.Where(_ => _.IsAlive && !_.HasSameIdentifierAs(GameData.President)).Select(_ => _.Identifier));
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
