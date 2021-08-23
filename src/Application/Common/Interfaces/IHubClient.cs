using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SecretHitler.Application.LegacyEngine;
using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.Common.Interfaces
{
    public interface IHubClient
    {
        Task MessageReceived(string message);
        Task UpdateGameData(GameDataDto gameData);
        Task PlayerSelectionRequested(GameState gameState, IEnumerable<Guid> players);
        Task PlayerVoteRequested();
        Task PolicySelectionRequested(IEnumerable<PolicyType> policies, int count, bool vetoAllowed);
        Task PolicyPeek(IEnumerable<PolicyType> peeked);
        Task LoyaltyPeek(Guid player, PlayerRole loyalty);
        Task ApproveVetoRequested();
    }
}
