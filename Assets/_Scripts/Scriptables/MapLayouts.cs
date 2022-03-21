using Systems.MapGeneration;
using UnityEngine;
using Utilities.Extensions;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Game/Create MapLayouts", fileName = "MapLayouts", order = 0)]
    public class MapLayouts : ScriptableObject
    {
        [SerializeField] private MapReference[] mapReferences;

        public MapReference GetRandomMapLayout()
        {
            return mapReferences.RandomElement();
        }
    }
}