using System;
using System.Collections.Generic;
using System.Linq;

using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Entities
{
    /// <summary>
    ///     A player in the game.
    /// </summary>
    public class PlayerDto : IPlayerInfo
    {
        /// <summary>
        ///     GUID for the player.
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        ///     Display name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Is the player alive?
        /// </summary>
        public bool IsAlive { get; set; }

        /// <summary>
        ///     Is the player the president?
        /// </summary>
        public bool IsPresident { get; set; }

        /// <summary>
        ///     Is the player the chancellor?
        /// </summary>
        public bool IsChancellor { get; set; }

        /// <summary>
        ///     The player's role.
        /// </summary>
        public PlayerRole? Role { get; set; }
    }
}