﻿using SecretHitler.Game.Entities;
using SecretHitler.Game.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Represents an interface which can prompt a user to perform an action of some sort or update
    /// the user interface with information.
    /// </summary>
    public interface IPlayerLogic
    {
        void UpdateGameData(GameData playerData);

        void MessageReceived(string message);

        Task<Guid> SelectPlayer(GameState gameState, IEnumerable<Guid> candidates);

        Task<bool> GetVote();

        Task<IList<PolicyType>> SelectPolicies(IEnumerable<PolicyType> drawnPolicies, int allowedCount);

        void ShowPolicies(IEnumerable<PolicyType> deckTopThree);

        void RevealLoyalty(PlayerRole role);

        Task<bool> PromptForVetoApproval();
    }
}
