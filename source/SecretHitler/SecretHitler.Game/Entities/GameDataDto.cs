using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Entities
{
    public class GameDataDto
    {
        /// <summary>
        /// The Guid for this game.
        /// </summary>
        public Guid GameGuid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// All known players.
        /// </summary>
        public IList<PlayerDto> Players { get; set; } = new List<PlayerDto>();

        /// <summary>
        /// Current value of the election tracker.
        /// </summary>
        public int ElectionTracker { get; set; }

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int EnactedLiberalPolicyCount { get; set; }

        /// <summary>
        /// The number of enacted liberal policies.
        /// </summary>
        public int EnactedFascistPolicyCount { get; set; }
    }
}
