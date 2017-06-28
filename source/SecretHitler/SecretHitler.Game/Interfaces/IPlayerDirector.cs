using System.Collections.Generic;
using SecretHitler.Game.Enums;
using System;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Represents a SignalR hub which can communicate with the specified clients and tell them what to do.
    /// </summary>
    public interface IPlayerDirector
    {
        void UpdatePlayerStates(IEnumerable<IPlayerInfo> playerData);

        void Broadcast(string message);

        void SendMessage(Guid player, string message);

        void SelectPlayer(Guid chooser, GameState gameState, IEnumerable<Guid> candidates);

        void GetVotes(IEnumerable<Guid> voters);

        void GetPresidentialPolicies(Guid president, IEnumerable<PolicyType> drawnPolicies);

        void GetEnactedPolicy(Guid chancellor, IEnumerable<PolicyType> drawnPolicies);

        void PolicyPeek(Guid president, IList<PolicyType> deckTopThree);

        void Reveal(Guid president, PlayerRole role);

        void ApproveVeto(Guid president);
    }
}
