using System;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Entities
{
    /// <summary>
    /// A player in the game.
    /// </summary>
    public class Player : IPlayerInfo
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
        /// Players are considered equal if their GUID matches.
        /// </summary>
        /// <param name="other">The player whose GUID should be used for comparison.</param>
        /// <returns>True if both players have the same GUID.</returns>
        public bool Equals(IPlayerInfo other) => other != null && other.Identifier.Equals(Identifier);

        /// <summary>
        /// Implicitly convert a <see cref="Player"/> to its <see cref="Guid"/> representation for convenience.
        /// </summary>
        /// <param name="player">The player to convert.</param>
        public static implicit operator Guid(Player player) => player.Identifier;
    }
}
