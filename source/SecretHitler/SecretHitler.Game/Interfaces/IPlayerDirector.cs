using System.Collections.Generic;
using SecretHitler.Game.Enums;
using System;
using System.Threading.Tasks;

using SecretHitler.Game.Entities;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Represents a SignalR hub which can communicate with the specified clients and tell them what to do.
    /// </summary>
    public interface IPlayerDirector
    {
        Task UpdateGameData(Guid player, GameDataDto gameData);

        Task Broadcast(string message);

        Task SendMessage(Guid player, string message);

        Task SelectPlayer(Guid chooser, GameState gameState, IEnumerable<Guid> candidates);

        Task GetVotes(IEnumerable<Guid> voters);

        Task GetPresidentialPolicies(Guid president, IEnumerable<PolicyType> drawnPolicies);

        Task GetEnactedPolicy(Guid chancellor, IEnumerable<PolicyType> drawnPolicies, bool allowVeto);

        Task PolicyPeek(Guid president, IEnumerable<PolicyType> deckTopThree);

        Task Reveal(Guid president, Guid revealedGuid, PlayerRole role);

        Task ApproveVeto(Guid president);
    }
}
