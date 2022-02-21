using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns a random element from the enumerable.
        /// </summary>
        /// <param name="thisIEnumerable">The enumerable to return a random element from.</param>
        /// <typeparam name="T">The type of the enumerable.</typeparam>
        /// <returns>A random element from the list, or the default type value if the list is empty.</returns>
        public static T RandomElement<T>(this IEnumerable<T> thisIEnumerable)
        {
            var thisArray = thisIEnumerable as T[] ?? thisIEnumerable.ToArray();
            
            if (!thisArray.Any()) 
                return default;
            
            var randomIndex = Random.Range(0, thisArray.Length);
            return thisArray[randomIndex];
        }
    }
}