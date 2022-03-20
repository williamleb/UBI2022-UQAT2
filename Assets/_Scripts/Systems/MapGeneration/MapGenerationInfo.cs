using System.Collections.Generic;
using UnityEngine;

namespace Systems.MapGeneration
{
    public class MapGenerationInfo : MonoBehaviour
    {
        public IEnumerable<RoomGenerationInfo> Rooms => rooms;
        [SerializeField] private RoomGenerationInfo[] rooms;
    }
}