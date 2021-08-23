using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using SecretHitler.Application.Common.Interfaces;
using SecretHitler.Application.LegacyEngine;
using SecretHitler.Domain.Enums;

namespace SecretHitler.WebUI.Legacy
{
    public class Director : IPlayerDirector
    {
        private readonly ILogger<Director> _logger;

        public Director(IHubContext<ServerHub, IHubClient> connectionContext, ILogger<Director> logger)
        {
            _logger = logger;
            Context = connectionContext;
        }

        private IHubContext<ServerHub, IHubClient> Context { get; set; }


        public async Task Broadcast(string message)
        {
            _logger.LogInformation($"Chat message: {message}");
            await Context.Clients.All.MessageReceived(message);
        }

        public async Task SendMessage(Guid player, string message)
        {
            _logger.LogInformation($"Chat message from {player}: {message}");
            await GetUser(player).MessageReceived(message);
        }

        public async Task UpdateGameData(Guid player, GameDataDto gameData)
        {
            _logger.LogDebug($"Transmit {nameof(IHubClient.UpdateGameData)} to player {player}");
            await GetUser(player).UpdateGameData(gameData);
        }

        public async Task SelectPlayer(Guid chooser, GameState gameState, IEnumerable<Guid> candidates)
        {
            var enumerable = candidates as Guid[] ?? candidates.ToArray();
            _logger.LogDebug($"Transmit {nameof(IHubClient.PlayerSelectionRequested)} to player {chooser} with state {gameState} and {enumerable.Length} candidates.");
            await GetUser(chooser).PlayerSelectionRequested(gameState, enumerable);
        }

        public async Task GetVotes(IEnumerable<Guid> voters)
        {
            var tasks = new List<Task>();
            foreach (var voter in voters)
            {
                _logger.LogDebug($"Transmit {nameof(IHubClient.PlayerVoteRequested)} to player {voter}.");
                tasks.Add(GetUser(voter).PlayerVoteRequested());
            }

            await Task.WhenAll(tasks);
        }

        public async Task GetPresidentialPolicies(Guid president, IEnumerable<PolicyType> drawnPolicies)
        {
            var policyTypes = drawnPolicies as PolicyType[] ?? drawnPolicies.ToArray();
            _logger.LogDebug($"Transmit {nameof(IHubClient.PolicySelectionRequested)} to player {president} with policies {string.Join(",", policyTypes.Select(_ => _.ToString()))}.");
            await GetUser(president).PolicySelectionRequested(policyTypes, Constants.PresidentialPolicyPassCount, false);
        }

        public async Task GetEnactedPolicy(Guid chancellor, IEnumerable<PolicyType> drawnPolicies, bool allowVeto)
        {
            var policyTypes = drawnPolicies as PolicyType[] ?? drawnPolicies.ToArray();
            _logger.LogDebug($"Transmit {nameof(IHubClient.PolicySelectionRequested)} to player {chancellor} with policies {string.Join(",", policyTypes.Select(_ => _.ToString()))}.");
            await GetUser(chancellor).PolicySelectionRequested(policyTypes, Constants.ChancellorPolicySelectionCount, allowVeto);
        }

        public async Task PolicyPeek(Guid president, IEnumerable<PolicyType> deckTopThree)
        {
            var policyTypes = deckTopThree as PolicyType[] ?? deckTopThree.ToArray();
            _logger.LogDebug($"Transmit {nameof(IHubClient.PolicyPeek)} to player {president} with revealed policies {string.Join(",", policyTypes.Select(_ => _.ToString()))}.");
            await GetUser(president).PolicyPeek(policyTypes);
        }

        public async Task Reveal(Guid president, Guid revealedGuid, PlayerRole role)
        {
            _logger.LogDebug($"Transmit {nameof(IHubClient.LoyaltyPeek)} to player {president} for player {revealedGuid} with role {role}.");
            await GetUser(president).LoyaltyPeek(revealedGuid, role);
        }

        public async Task ApproveVeto(Guid president)
        {
            _logger.LogDebug($"Transmit {nameof(IHubClient.ApproveVetoRequested)} to player {president}.");
            await GetUser(president).ApproveVetoRequested();
        }

        private IHubClient GetUser(Guid guid)
        {
            return Context.Clients.Group(guid.ToString());
        }
    }
}
