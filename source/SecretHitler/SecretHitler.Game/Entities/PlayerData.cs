using System;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Utility;

namespace SecretHitler.Game.Entities
{
    /// <summary>
    /// A player in the game.
    /// </summary>
    public class PlayerData : IPlayerInfo
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

        /// <summary>
        /// Implicitly convert a <see cref="PlayerData"/> to its <see cref="Guid"/> representation for convenience.
        /// </summary>
        /// <param name="player">The player to convert.</param>
        public static implicit operator Guid(PlayerData player) => player?.AsGuid() ?? Guid.Empty;
    }
}
