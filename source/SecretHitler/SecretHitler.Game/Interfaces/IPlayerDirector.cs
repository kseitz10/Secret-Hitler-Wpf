using System.Collections.Generic;
using SecretHitler.Game.Enums;
using System;
using SecretHitler.Game.Entities;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Represents a SignalR hub which can communicate with the specified clients and tell them what to do.
    /// </summary>
    public interface IPlayerDirector
    {
        void UpdateGameData(Guid player, GameData gameData);

        void Broadcast(string message);

        void SendMessage(Guid player, string message);

        void SelectPlayer(Guid chooser, GameState gameState, IEnumerable<Guid> candidates);

        void GetVotes(IEnumerable<Guid> voters);

        void GetPresidentialPolicies(Guid president, IEnumerable<PolicyType> drawnPolicies);

        void GetEnactedPolicy(Guid chancellor, IEnumerable<PolicyType> drawnPolicies);

        void PolicyPeek(Guid president, IEnumerable<PolicyType> deckTopThree);

        void Reveal(Guid president, Guid revealedGuid, PlayerRole role);

        void ApproveVeto(Guid president);
    }
}
