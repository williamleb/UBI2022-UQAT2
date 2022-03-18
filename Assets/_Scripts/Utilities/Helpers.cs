using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Helpers
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitDictionary =
            new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetWait(float time)
        {
            if (WaitDictionary.TryGetValue(time, out var wait)) return wait;
            WaitDictionary[time] = new WaitForSeconds(time);
            return WaitDictionary[time];
        }
    }
}