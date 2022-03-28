using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.MapGeneration
{
    public class MapGenerationInfo : MonoBehaviour
    {
        public IEnumerable<RoomGenerationInfo> Rooms => rooms;
        [SerializeField] private RoomGenerationInfo[] rooms;

        [Button(ButtonSizes.Large)]
        private void FindRoomsInPrefab()
        {
            rooms = GetComponentsInChildren<RoomGenerationInfo>();
        }

        private void OnValidate()
        {
            FindRoomsInPrefab();
        }
    }
}