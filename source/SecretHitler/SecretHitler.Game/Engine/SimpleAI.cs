using SecretHitler.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Utility;
using SecretHitler.Game.Entities;

namespace SecretHitler.Game.Engine
{
    /// <summary>
    /// A simple AI to take the place of players.
    /// </summary>
    public class SimpleAI : IPlayerLogic
    {
        public Task<bool> GetVote()
        {
            return Task.FromResult(true);
        }

        public void MessageReceived(string message)
        {
            // Intentionally blank.
        }

        public Task<bool> PromptForVetoApproval()
        {
            return Task.FromResult(true);
        }

        public Task RevealLoyalty(Guid playerGuid, PlayerRole role)
        {
            // Intentionally blank, but a good AI would do something with this information.
            return Task.CompletedTask;
        }

        public Task<Guid> SelectPlayer(GameState gameState, IEnumerable<Guid> candidates)
        {
            return PickRandom(candidates);
        }

        public Task<IList<PolicyType>> SelectPolicies(IEnumerable<PolicyType> drawnPolicies, int allowedCount)
        {
            return PickRandom(drawnPolicies, allowedCount);
        }

        public Task ShowPolicies(IEnumerable<PolicyType> deckTopThree)
        {
            // Intentionally blank, but a good AI would do something with this information.
            return Task.CompletedTask;
        }

        public void UpdateGameData(GameData gameData)
        {
            // Intentionally blank, but a good AI would do something with this information.
        }

        private async Task<T> PickRandom<T>(IEnumerable<T> items)
        {
            return (await PickRandom(items, 1)).Single();
        }

        private Task<IList<T>> PickRandom<T>(IEnumerable<T> items, int ct)
        {
            var itemArray = items.ToArray();
            itemArray.Shuffle();
            return Task.FromResult<IList<T>>(itemArray.Take(ct).ToList());
        }
    }
}
