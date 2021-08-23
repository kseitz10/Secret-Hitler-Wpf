using System;
using System.Collections.Generic;
using System.Linq;

using SecretHitler.Application.Common.Interfaces;
using SecretHitler.Application.Common.Mappings;
using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.LegacyEngine
{
    public class PlayerDataDto : IMapFrom<PlayerData>, IPlayerInfo
    {
        /// <summary>
        /// GUID for the player.
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Display name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is the player alive?
        /// </summary>
        public bool IsAlive { get; set; }

        /// <summary>
        /// Is the player the president?
        /// </summary>
        public bool IsPresident { get; set; }

        /// <summary>
        /// Is the player the chancellor?
        /// </summary>
        public bool IsChancellor { get; set; }

        /// <summary>
        /// The player's role.
        /// </summary>
        public PlayerRole? Role { get; set; }
    }
}