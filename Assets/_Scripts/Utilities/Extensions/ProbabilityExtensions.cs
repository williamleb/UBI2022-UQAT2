using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class ProbabilityUtils
    {
        public static T WeightedRandomElement<T>(this IEnumerable<T> thisIEnumerable) where T : IProbabilityObject
        {
            var thisArray = thisIEnumerable as T[] ?? thisIEnumerable.ToArray();
            
            if (!thisArray.Any()) 
                return default;
            
            var sumOfProbabilities = thisArray.Sum(element => element.Probability);

            var randomFloat = Random.Range(0f, sumOfProbabilities);
            var currentProbability = 0f;
            foreach (var element in thisArray)
            {
                currentProbability += element.Probability;
                if (randomFloat <= currentProbability)
                {
                    return element;
                }
            }

            return default;
        }
    }
}