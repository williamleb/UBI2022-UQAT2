using Systems.MapGeneration;
using UnityEngine;
using Utilities.Extensions;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Game/Create MapLayouts", fileName = "MapLayouts", order = 0)]
    public class MapLayouts : ScriptableObject
    {
        [SerializeField] private MapGenerationInfo[] mapLayouts;

        public MapGenerationInfo GetRandomMapLayout()
        {
            return mapLayouts.RandomElement();
        }
    }
}