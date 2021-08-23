using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.LegacyEngine
{
    /// <summary>
    /// Represents all of the game board state information.
    /// </summary>
    public class GameData
    {
        /// <summary>
        /// The Guid for this game.
        /// </summary>
        public Guid GameGuid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The current state of the machine.
        /// </summary>
        public StateMachineState MachineState { get; set; } = StateMachineState.None;

        /// <summary>
        /// All known players.
        /// </summary>
        public IList<PlayerData> Players { get; set; } = new List<PlayerData>();

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
        /// The last policies drawn from the deck. This accommodates discarding cards when getting a response
        /// from the player.
        /// </summary>
        public List<PolicyType> DrawnPolicies { get; set; }

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int EnactedLiberalPolicyCount { get; set; }

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int EnactedFascistPolicyCount { get; set; }

        /// <summary>
        /// Ja vote count
        /// </summary>
        public int JaVoteCount { get; set; }

        /// <summary>
        /// Nein vote count
        /// </summary>
        public int NeinVoteCount { get; set; }

        /// <summary>
        /// The queue of presidents.
        /// </summary>
        public Queue<Guid> PresidentialQueue { get; set; } = new Queue<Guid>();

        /// <summary>
        /// Players ineligible to serve as chancellor due to term limits.
        /// </summary>
        public IList<Guid> IneligibleChancellors { get; set; } = new List<Guid>();

        /////// <summary>
        /////// If the last election was a special election (president wasn't picked from the queue) then
        /////// this should be true.
        /////// </summary>
        ////public bool SpecialSession { get; set; }

        /// <summary>
        /// Sets the active president.
        /// </summary>
        [XmlIgnore]
        public PlayerData President
        {
            get
            {
                return Players.FirstOrDefault(_ => _.IsPresident);
            }
            set
            {
                var current = President;
                if (current != null)
                    current.IsPresident = false;

                if (value != null)
                    value.IsPresident = true;
            }
        }

        /// <summary>
        /// Sets the active chancellor.
        /// </summary>
        [XmlIgnore]
        public PlayerData Chancellor
        {
            get
            {
                return Players.FirstOrDefault(_ => _.IsChancellor);
            }
            set
            {
                var current = Chancellor;
                if (current != null)
                    current.IsChancellor = false;

                if (value != null)
                    value.IsChancellor = true;
            }
        }
    }
}
