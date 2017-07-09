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
    /// Manipulates the game entity.
    /// </summary>
    public class GameDataManipulator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The game object to manipulate.</param>
        public GameDataManipulator(GameData game)
        {
            Game = game;
        }

        /// <summary>
        /// The game data entity.
        /// </summary>
        private GameData Game { get; set; }

        /// <summary>
        /// Initialize the game data for a new game.
        /// </summary>
        public virtual void ResetGame()
        {
            // Mark all players as alive and assign them some roles
            foreach (var player in Game.Players)
            {
                player.Role = PlayerRole.Liberal;
                player.IsAlive = true;
                player.IsPresident = false;
                player.IsChancellor = false;
            }

            var liberals = Game.Players.ToArray();
            liberals.Shuffle();

            var fascistCount = 1 + ((Game.Players.Count - 5) / 2);

            foreach (var fascist in liberals.Take(fascistCount))
                fascist.Role = PlayerRole.Fascist;

            liberals.Skip(fascistCount).First().Role = PlayerRole.Hitler;

            // Set up the presidential queue
            var shuffled = Game.Players.Select(_ => _.Identifier).ToList();
            shuffled.Shuffle();
            Game.PresidentialQueue = new Queue<Guid>(shuffled);

            Game.IneligibleChancellors = new List<Guid>();
            Game.EnactedFascistPolicyCount = 0;
            Game.EnactedLiberalPolicyCount = 0;
            Game.ElectionTracker = 0;
            Game.GameGuid = Guid.NewGuid();

            new PolicyDeck(Game.DrawPile, Game.DiscardPile, true);
        }

        /// <summary>
        /// Increase the election tracker count. Clears term limits and resets the
        /// election tracker if needed, as well as enacts a policy. The consumer
        /// must check if a win condition needs to be enforced as a result of the
        /// policy being enacted.
        /// </summary>
        /// <param name="policyDeck">
        /// Policy deck implementation. The default is used if null.
        /// </param>
        /// <returns>True if election tracker max value exceeded.</returns>
        public virtual bool UpdateElectionTracker(ICardDeck<PolicyType> policyDeck = null)
        {
            Game.ElectionTracker++;

            if (Game.ElectionTracker >= Constants.FailedElectionThreshold)
            {
                Game.ElectionTracker = 0;
                Game.IneligibleChancellors.Clear();
                var drawnPolicy = (policyDeck ?? new PolicyDeck(Game.DrawPile, Game.DiscardPile, false)).Draw();
                EnactPolicy(drawnPolicy.Single());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the next president from the queue, places the current president onto
        /// the queue.
        /// </summary>
        public virtual PlayerData GetPresidentFromQueue()
        {
            do
            {
                var nextPresidentGuid = Game.PresidentialQueue.Dequeue();
                Game.PresidentialQueue.Enqueue(nextPresidentGuid);

                var nextPresident = Game.Players.First(_ => _.Identifier == nextPresidentGuid);
                Game.President = nextPresident;
            }
            while (!Game.President.IsAlive);

            return Game.President;
        }

        /// <summary>
        /// Track the term limits for the current president and chancellor where
        /// applicable. This resets any existing term limits.
        /// </summary>
        public virtual void UpdateTermLimits()
        {
            Game.IneligibleChancellors.Clear();

            if (Game.Players.Count(_ => _.IsAlive) > 5)
                Game.IneligibleChancellors.Add(Game.President);

            Game.IneligibleChancellors.Add(Game.Chancellor);
        }

        private void EnactPolicy(PolicyType policy)
        {
            if (policy == PolicyType.Fascist)
                Game.EnactedFascistPolicyCount++;
            else if (policy == PolicyType.Liberal)
                Game.EnactedLiberalPolicyCount++;
            else
                throw new ArgumentException("Unexpected policy type.");
        }
    }
}
