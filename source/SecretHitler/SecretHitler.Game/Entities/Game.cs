using System;
using System.Collections.Generic;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Entities
{
    /// <summary>
    /// Represents all of the game board state information.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// All known players.
        /// </summary>
        public List<Player> Players { get; set; } = new List<Player>();

        /// <summary>
        /// Current value of the election tracker.
        /// </summary>
        public int ElectionTracker { get; set; }

        /// <summary>
        /// List of items currently in the draw pile. Index 0 represents the bottom.
        /// </summary>
        public IList<PolicyType> DrawPile { get; set; } = new List<PolicyType>();

        /// <summary>
        /// List of items currently in the discard pile. Index 0 represents the bottom.
        /// </summary>
        public IList<PolicyType> DiscardPile { get; set; } = new List<PolicyType>();

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int EnactedLiberalPolicyCount { get; set; }

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int EnactedFascistPolicyCount { get; set; }

        /// <summary>
        /// The queue of presidents.
        /// </summary>
        public Queue<Guid> PresidentialQueue { get; set; } = new Queue<Guid>();

        /// <summary>
        /// Players ineligible to serve as chancellor due to term limits.
        /// </summary>
        public IList<Guid> IneligibleChancellors { get; set; } = new List<Guid>();
    }
}
