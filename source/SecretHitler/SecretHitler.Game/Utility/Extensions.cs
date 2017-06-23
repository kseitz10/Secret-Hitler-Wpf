using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Utility
{
    /// <summary>
    /// Common extension methods.
    /// </summary>
    public static class Extensions
    {
        private static Random rng = new Random();

        /// <summary>
        /// Shuffle with the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">Type of items being shuffled.</typeparam>
        /// <param name="list">Items to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// A quick way to add a range of items.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="list">List to which items are added.</param>
        /// <param name="itemsToAdd">The items to add.</param>
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> itemsToAdd)
        {
            foreach (var i in itemsToAdd)
                list.Add(i);
        }

        /// <summary>
        /// Shorthand way to get a player's GUID.
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns>Guid</returns>
        public static Guid AsGuid(this IPlayerInfo player)
        {
            return player?.Identifier ?? Guid.Empty;
        }

        /// <summary>
        /// Shorthand way to get players' GUID.
        /// </summary>
        /// <param name="players">Players</param>
        /// <returns>Guids</returns>
        public static IEnumerable<Guid> AsGuids(this IEnumerable<IPlayerInfo> players)
        {
            return players?.Select(AsGuid);
        }

        /// <summary>
        /// Shorthand way to get player objects from a collection of GUIDs.
        /// </summary>
        /// <param name="guids">Guids</param>
        /// <returns>Players</returns>
        public static IList<TPlayer> AsPlayers<TPlayer>(this IEnumerable<Guid> guids, IEnumerable<TPlayer> players) where TPlayer : IPlayerInfo
        {
            return guids.Join(players, g => g, p => p.Identifier, (g, p) => p).ToList();
        }

        /// <summary>
        /// Shorthand way to see if players have the same GUID.
        /// </summary>
        /// <param name="player">First player</param>
        /// <param name="other">Second player</param>
        /// <returns>True if same identifier.</returns>
        public static bool HasSameIdentifierAs(this IPlayerInfo player, IPlayerInfo other)
        {
            return player != null && other != null && player.Identifier == other.Identifier;
        }
    }
}
