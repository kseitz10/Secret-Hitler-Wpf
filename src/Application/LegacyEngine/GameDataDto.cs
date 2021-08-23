using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SecretHitler.Application.Common.Mappings;

namespace SecretHitler.Application.LegacyEngine
{
    public class GameDataDto : IMapFrom<GameData>
    {
        /// <summary>
        /// The Guid for this game.
        /// </summary>
        public Guid GameGuid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// All known players.
        /// </summary>
        public IReadOnlyList<PlayerDataDto> Players { get; set; } = new List<PlayerDataDto>();

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
