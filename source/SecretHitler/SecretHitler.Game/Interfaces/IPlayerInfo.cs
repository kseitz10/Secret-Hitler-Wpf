using System;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// A player in the game.
    /// </summary>
    public interface IPlayerInfo
    {
        /// <summary>
        /// GUID for the player.
        /// </summary>
        Guid Identifier { get; }

        /// <summary>
        /// Display name of the player.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is the player alive?
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Is the player the president?
        /// </summary>
        bool IsPresident { get; }

        /// <summary>
        /// Is the player the chancellor?
        /// </summary>
        bool IsChancellor { get; }

        /// <summary>
        /// The player's role.
        /// </summary>
        PlayerRole? Role { get; }
    }
}
