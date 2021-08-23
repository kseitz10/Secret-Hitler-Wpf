using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.Extensions.Logging;

using SecretHitler.Application.Common;
using SecretHitler.Application.Common.Exceptions;
using SecretHitler.Application.Common.Interfaces;
using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.LegacyEngine
{
    /// <summary>
    /// State machine for the Secret Hitler game.
    /// </summary>
    public class GameStateMachine : IPlayerResponseHandler
    {
        private readonly ILogger<GameStateMachine> _logger;

        /// <summary>
        /// Constructs a new instance of the game state machine.
        /// </summary>
        public GameStateMachine(IPlayerDirector director, ILogger<GameStateMachine> logger)
        {
            _logger = logger;
            Director = director;
            GameData = new GameData();
            GameDataManipulator = new GameDataManipulator(GameData);
            PolicyDeck = new PolicyDeck(GameData.DrawPile, GameData.DiscardPile, true);
        }

        public void LoadGameState(GameData data)
        {
            GameData = data;
            GameDataManipulator = new GameDataManipulator(GameData);
            PolicyDeck = new PolicyDeck(data.DrawPile, data.DiscardPile, false);
        }

        #region Properties

        /// <summary>
        /// The data backing the state machine.
        /// </summary>
        public GameData GameData { get; private set; }

        /// <summary>
        /// Gets the object responsible for sending a message to one or more clients.
        /// </summary>
        public IPlayerDirector Director { get; set; }

        /// <summary>
        /// Whether game is in progress.
        /// </summary>
        public bool GameInProgress => GameData.MachineState != StateMachineState.None;

        /// <summary>
        /// The policy draw and discard pile manager.
        /// </summary>
        internal ICardDeck<PolicyType> PolicyDeck { get; set; }

        /// <summary>
        /// Utility for managing the game state data.
        /// </summary>
        internal GameDataManipulator GameDataManipulator { get; set; }

        /// Whether or not vetoes are allowed.
        /// </summary>
        internal bool AllowVetoes => GameData.EnactedFascistPolicyCount >= Constants.MinFascistPolicyCountForVeto;

        #endregion

        #region Client Response Methods

        /// <summary>
        /// Start the game.
        /// </summary>
        public async Task Start()
        {
            if (GameData.MachineState != StateMachineState.None)
                throw new GameStateException("Game already in progress.");

            await PrepareGame();
        }

        /// <summary>
        /// Sends updated game data to players.
        /// </summary>
        public async Task DisseminateGameData()
        {
            await Task.WhenAll(GameData.Players.Select(player => Director.UpdateGameData(player, PrepareGameDataForPlayerDissemination(player))));
            // TODO Remove
            await Task.Delay(1000);
        }

        public GameDataDto GenerateGameDataForPlayer(Guid playerIdentifier)
        {
            var player = GameData.Players.FirstOrDefault(_ => _.Identifier == playerIdentifier) ?? throw new KeyNotFoundException(playerIdentifier.ToString());
            return PrepareGameDataForPlayerDissemination(player);
        }

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        public async Task Acknowledge(bool? acknowledge)
        {
            switch (GameData.MachineState)
            {
                case StateMachineState.AwaitingVetoResponse:
                    if (!acknowledge.HasValue)
                    {
                        throw new GameStateException("Expecting true or false response for veto approval, not null.");
                    }
                    else if (acknowledge.Value)
                    {
                        // TODO test
                        PolicyDeck.Discard(GameData.DrawnPolicies);
                        GameData.DrawnPolicies = null;
                        await UpdateElectionTrackerAndHandleChaos("The policies were successfully vetoed!");
                    }
                    else
                    {
                        // TODO test
                        await Director.Broadcast("Unsuccessful veto. The chancellor must choose a policy.");
                        GameData.MachineState = StateMachineState.AwaitingEnactedPolicy;
                        await Director.GetEnactedPolicy(GameData.Chancellor, GameData.DrawnPolicies, false);
                    }

                    break;

                case StateMachineState.AwaitingSpecialPowerAcknowledgment:
                    await PrepareNextElection();
                    break;

                default:
                    throw new GameStateException($"{nameof(Acknowledge)} called for invalid state {GameData.MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that a player has been selected by the client that was last issued a request.
        /// </summary>
        /// <param name="player">The selected player.</param>
        public async Task PlayerSelected(Guid player)
        {
            switch (GameData.MachineState)
            {
                case StateMachineState.AwaitingNomination:
                    GameData.Chancellor = CoercePlayer(player);
                    await Director.Broadcast($"The president has nominated {GameData.Chancellor.Name} as chancellor.");
                    GameData.MachineState = StateMachineState.AwaitingVotes;
                    await DisseminateGameData();
                    await Director.Broadcast("It is time to vote.");
                    await Director.GetVotes(GameData.Players.Where(_ => _.IsAlive).AsGuids());
                    break;

                case StateMachineState.AwaitingSpecialElectionPick:
                    await PrepareNextElection(specialElectionPresident: CoercePlayer(player));
                    break;

                case StateMachineState.AwaitingExecutionPick:
                    var playerToKill = CoercePlayer(player);
                    await Director.Broadcast($"The president has chosen to execute {playerToKill.Name}.");
                    playerToKill.IsAlive = false;
                    await PrepareNextElection();
                    break;

                case StateMachineState.AwaitingInvestigateLoyaltyPick:
                    var playerToReveal = CoercePlayer(player);
                    await Director.Broadcast($"The president is now aware of the loyalty of {playerToReveal.Name}.");
                    await Director.Reveal(GameData.President, playerToReveal.Identifier, playerToReveal.Role == PlayerRole.Liberal ? PlayerRole.Liberal : PlayerRole.Fascist);
                    GameData.MachineState = StateMachineState.AwaitingSpecialPowerAcknowledgment;
                    break;

                default:
                    throw new GameStateException($"{nameof(PlayerSelected)} called for invalid state {GameData.MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that one or more policies were selected by the client asked to select policies.
        /// </summary>
        /// <param name="policies">The selected policies.</param>
        public async Task PoliciesSelected(IEnumerable<PolicyType> policies)
        {
            var myPolicies = policies.ToList();
            switch (GameData.MachineState)
            {
                case StateMachineState.AwaitingEnactedPolicy:
                    // TODO Validate policy was actually drawn, delivered by correct player
                    if (myPolicies.Count != Constants.ChancellorPolicySelectionCount)
                        throw new GameStateException("Too many policies selected for the current game state.");

                    var policy = myPolicies.First();
                    if (policy == PolicyType.None)
                    {
                        if (!AllowVetoes)
                            throw new GameStateException("Currently not eligible to veto policies.");

                        await Director.Broadcast("A veto has been requested.");
                        GameData.MachineState = StateMachineState.AwaitingVetoResponse;
                        await Director.ApproveVeto(GameData.President);
                        return;
                    }

                    foreach (var p in myPolicies)
                    {
                        GameData.DrawnPolicies.Remove(p);
                    }

                    PolicyDeck.Discard(GameData.DrawnPolicies);
                    GameData.DrawnPolicies = null;

                    if (policy == PolicyType.Fascist)
                    {
                        await Director.Broadcast("A fascist policy has been enacted!");
                        GameData.EnactedFascistPolicyCount++;
                        await DisseminateGameData();

                        if (InvokeCurrentPresidentialPower())
                            return;
                    }
                    else if (policy == PolicyType.Liberal)
                    {
                        await Director.Broadcast("A liberal policy has been enacted!");
                        GameData.EnactedLiberalPolicyCount++;
                    }

                    await PrepareNextElection();
                    break;

                case StateMachineState.AwaitingPresidentialPolicies:
                    // TODO Validate policy was actually drawn, delivered by correct player
                    // TODO Test me.
                    if (myPolicies.Count != Constants.PresidentialPolicyPassCount)
                        throw new GameStateException("Too many/few policies selected for the current game state.");

                    foreach (var p in myPolicies)
                    {
                        GameData.DrawnPolicies.Remove(p);
                    }

                    PolicyDeck.Discard(GameData.DrawnPolicies);
                    GameData.DrawnPolicies = policies.ToList();

                    await Director.Broadcast("The president has offered policies to the chancellor.");
                    GameData.MachineState = StateMachineState.AwaitingEnactedPolicy;
                    await Director.GetEnactedPolicy(GameData.Chancellor, policies, AllowVetoes);
                    break;

                default:
                    throw new GameStateException($"{nameof(PoliciesSelected)} called for invalid state {GameData.MachineState}.");
            }
        }

        /// <summary>
        /// Indicates that a vote has been collected.
        /// </summary>
        /// <param name="vote">The collected vote.</param>
        public async Task VoteCollected(bool vote)
        {
            if (GameData.MachineState != StateMachineState.AwaitingVotes)
            {
                throw new GameStateException($"{nameof(VoteCollected)} called for invalid state {GameData.MachineState}.");
            }

            if (vote)
            {
                GameData.JaVoteCount++;
            }
            else
            {
                GameData.NeinVoteCount++;
            }

            if (GameData.JaVoteCount + GameData.NeinVoteCount < GameData.Players.Count(_ => _.IsAlive))
            {
                return;
            }

            var message = $"Votes have been tallied: {GameData.JaVoteCount} ja, {GameData.NeinVoteCount} nein.";
            if (GameData.JaVoteCount > GameData.NeinVoteCount)
            {
                await Director.Broadcast($"{message} The election was successful.");

                if (GameData.EnactedFascistPolicyCount >= Constants.RequiredFascistPoliciesForHitlerChancellorshipVictory && GameData.Chancellor.Role == PlayerRole.Hitler)
                {
                    GameData.MachineState = StateMachineState.None;
                    await Director.Broadcast("Congratulations on becoming chancellor, Hitler. FASCIST VICTORY!");
                    await DisseminateGameData();
                    return;
                }

                GameData.MachineState = StateMachineState.AwaitingPresidentialPolicies;
                await DisseminateGameData();
                GameData.DrawnPolicies = PolicyDeck.Draw(Constants.PresidentialPolicyDrawCount).ToList();
                await Director.GetPresidentialPolicies(GameData.President, GameData.DrawnPolicies);
            }
            else
            {
                message = $"{message} The election failed.";
                await UpdateElectionTrackerAndHandleChaos(message);
            }

            GameData.JaVoteCount = 0;
            GameData.NeinVoteCount = 0;
        }

        #endregion

        #region Private Methods

        private GameDataDto PrepareGameDataForPlayerDissemination(IPlayerInfo player)
        {
            // This data can be given to all players.
            var gd = new GameDataDto
            {
                GameGuid = GameData.GameGuid,
                EnactedFascistPolicyCount = GameData.EnactedFascistPolicyCount,
                EnactedLiberalPolicyCount = GameData.EnactedLiberalPolicyCount,
                ElectionTracker = GameData.ElectionTracker
            };

            var canDiscloseFascists = player.Role == PlayerRole.Fascist ||
                                      (player.Role == PlayerRole.Hitler && GameData.Players.Count <= 6);
            var canDiscloseHitler = player.Role != PlayerRole.Liberal;

            var playersEnumerable = GameData.Players.Select(p =>
            {
                var rtn = new PlayerDataDto()
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
            });

            gd.Players = playersEnumerable.ToList();
            return gd;
        }

        private async Task PrepareGame()
        {
            _logger.LogInformation("Resetting game state.");
            GameDataManipulator.ResetGame();
            await PrepareNextElection();
        }

        private async Task PrepareNextElection(bool updateTermLimits = true, IPlayerInfo specialElectionPresident = null)
        {
            _logger.LogInformation("Preparing election.");

            if (!GameData.Players.First(_ => _.Role == PlayerRole.Hitler).IsAlive)
            {
                GameData.MachineState = StateMachineState.None;
                await Director.Broadcast("HITLER IS DEAD! LIBERAL VICTORY!");
                await DisseminateGameData();
                return;
            }

            if (GameData.EnactedLiberalPolicyCount == Constants.LiberalPoliciesRequiredForVictory)
            {
                GameData.MachineState = StateMachineState.None;
                await Director.Broadcast("All liberal policies have been enacted. LIBERAL VICTORY!");
                await DisseminateGameData();
                return;
            }

            if (GameData.EnactedFascistPolicyCount == Constants.FascistPoliciesRequiredForVictory)
            {
                GameData.MachineState = StateMachineState.None;
                await Director.Broadcast("All fascist policies have been enacted. FASCIST VICTORY!");
                await DisseminateGameData();
                return;
            }

            if (updateTermLimits)
            {
                GameDataManipulator.UpdateTermLimits();
            }

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
            GameData.MachineState = StateMachineState.AwaitingNomination;

            await DisseminateGameData();
            await Director.Broadcast($"{GameData.President.Name} is now president and will nominate a chancellor.");
            await Director.SelectPlayer(GameData.President, GameState.ChancellorNomination, candidates.AsGuids());
        }

        private async Task UpdateElectionTrackerAndHandleChaos(string messagePrefix)
        {
            var chaos = GameDataManipulator.UpdateElectionTracker();
            await DisseminateGameData();
            if (chaos)
            {
                await Director.Broadcast($"{messagePrefix} Due to inactive government, there is chaos on the streets!".Trim());
            }
            else
            {
                await Director.Broadcast($"{messagePrefix} The election tracker is now at {GameData.ElectionTracker}. When it reaches {Constants.FailedElectionThreshold}, a policy will be enacted.".Trim());
            }

            await PrepareNextElection(false);
        }

        /// <summary>
        /// If a new fascist policy has been enacted, this method should be invoked to access
        /// the unlocked special power. If there is a special power, the method returns true to
        /// indicate that the calling code should not start the next election cycle.
        /// </summary>
        private bool InvokeCurrentPresidentialPower()
        {
            var delegates = new Dictionary<int, Func<Task>>();
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

        private async Task InvokePolicyPeek()
        {
            var policiesToDisplay = PolicyDeck.Peek(Constants.PresidentialPolicyDrawCount);
            GameData.MachineState = StateMachineState.AwaitingSpecialPowerAcknowledgment;
            await Director.Broadcast($"The president is being shown the topmost {Constants.PresidentialPolicyDrawCount} policies on the draw pile.");
            await Director.PolicyPeek(GameData.President, policiesToDisplay);
        }

        private async Task InvokeExecution()
        {
            GameData.MachineState = StateMachineState.AwaitingExecutionPick;
            await Director.Broadcast("The president is choosing a player to execute.");
            await Director.SelectPlayer(GameData.President, GameState.Execution, GameData.Players.Where(_ => _.IsAlive && !_.HasSameIdentifierAs(GameData.President)).Select(_ => _.Identifier));
        }

        private async Task InvokeInvestigateLoyalty()
        {
            GameData.MachineState = StateMachineState.AwaitingInvestigateLoyaltyPick;
            await Director.Broadcast("The president is choosing a player to investigate his/her loyalty.");
            await Director.SelectPlayer(GameData.President, GameState.InvestigateLoyalty, GameData.Players.Where(_ => _.IsAlive && !_.HasSameIdentifierAs(GameData.President)).Select(_ => _.Identifier));
        }

        private async Task InvokeSpecialElection()
        {
            GameData.MachineState = StateMachineState.AwaitingSpecialElectionPick;
            await Director.Broadcast("The president is choosing a player to serve as president in a special election.");
            await Director.SelectPlayer(GameData.President, GameState.SpecialElection, GameData.Players.Where(_ => _.IsAlive && !_.HasSameIdentifierAs(GameData.President)).Select(_ => _.Identifier));
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
