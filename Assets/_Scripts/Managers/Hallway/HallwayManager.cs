using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Singleton;

namespace Managers.Hallway
{
    public class HallwayManager : Singleton<HallwayManager>
    {
        private Dictionary<int, Hallway> hallways = new Dictionary<int, Hallway>();

        public IEnumerable<Hallway> Hallways => hallways.Values;

        [CanBeNull]
        public Hallway GetRandomHallway()
        {
            return !hallways.Any() ? null : Hallways.WeightedRandomElement();
        }

        [CanBeNull]
        public Hallway GetHallway(HallwayColor color)
        {
            foreach (var hallway in hallways.Values)
            {
                if (hallway.Color == color)
                {
                    return hallway;
                }
            }
            
            Debug.LogWarning($"Could not find hallway of color {color} in the scene.");
            return null;
        }

        public void RegisterHallway(Hallway hallway)
        {
            hallways.Add(hallway.HallwayId, hallway);
        }

        public void UnregisterHallway(Hallway hallway)
        {
            hallways.Remove(hallway.HallwayId);
        }
    }
}