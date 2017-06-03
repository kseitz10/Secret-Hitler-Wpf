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
    }
}
