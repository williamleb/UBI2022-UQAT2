using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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