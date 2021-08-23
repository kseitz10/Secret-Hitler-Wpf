using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SecretHitler.Application.LegacyEngine;
using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.Common.Interfaces
{
    /// <summary>
    /// Represents an interface which can prompt a user to perform an action of some sort or update
    /// the user interface with information.
    /// </summary>
    public interface IPlayerInterface
    {
        Task UpdateGameData(GameDataDto playerData);

        Task MessageReceived(string message);

        Task<Guid> SelectPlayer(GameState gameState, IEnumerable<Guid> candidates);

        Task<bool> GetVote();

        Task<IList<PolicyType>> SelectPolicies(IEnumerable<PolicyType> drawnPolicies, int allowedCount, bool allowVeto);

        Task ShowPolicies(IEnumerable<PolicyType> deckTopThree);

        Task RevealLoyalty(Guid playerGuid, PlayerRole role);

        Task<bool> PromptForVetoApproval();
    }
}
